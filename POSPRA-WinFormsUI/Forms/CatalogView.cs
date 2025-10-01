using POSPRA.Application.Services.FiscalService;
using POSPRA.Application.Services.LogService;
using POSPRA.Application.Services.ProductCatalogService;
using POSPRA.Application.Utility;
using POSPRA.Domain.Entities;
using POSPRA.DTOs.ProductCatalogDtos;
using POSPRA_WinFormsUI.AlertClasses;

namespace POSPRA_WinFormsUI.Forms
{
    public partial class CatalogView : Form
    {
        private readonly ILogService _logService;
        private readonly IFiscalService _fiscalService;
        private readonly IProductCatalogueService _productCatalogueService;
        private int _currentPage = 1;
        private int _pageSize = 50;

        private readonly System.Windows.Forms.Timer _searchDebounceTimer;
        private const int SEARCH_DEBOUNCE_MS = 300;
        private const int SEARCH_FETCH_LIMIT = 2000;
        private bool _isLoading = false;

        public CatalogView(IFiscalService fiscalService, IProductCatalogueService productCatalogueService, ILogService logService)
        {
            InitializeComponent();
            _fiscalService = fiscalService ?? throw new ArgumentNullException(nameof(fiscalService));
            _productCatalogueService = productCatalogueService ?? throw new ArgumentNullException(nameof(productCatalogueService));
            _logService = logService;

            ProductCatalogueDataGridView.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithAutoHeaderText;

            // Load button now loads FROM API and saves TO local DB
            btnLoad.Click += btnLoad_Click;

            _searchDebounceTimer = new System.Windows.Forms.Timer();
            _searchDebounceTimer.Interval = SEARCH_DEBOUNCE_MS;
            _searchDebounceTimer.Tick += async (s, e) =>
            {
                _searchDebounceTimer.Stop();
                await FilterProductsFromLocalDB();
            };

            // On form load, display data from LOCAL DB
            this.Load += async (s, e) => await LoadFromLocalDB();

            SearchBox.TextChanged += (s, e) =>
            {
                _searchDebounceTimer.Stop();
                _searchDebounceTimer.Start();
            };

            btnNext.Click += async (s, e) => await NextPage();
            btnPrev.Click += async (s, e) => await PrevPage();
        }

        /// <summary>
        /// Load button: Fetch from API, clear local DB, save to local DB
        /// </summary>
        private async void btnLoad_Click(object sender, EventArgs e)
        {
            if (_isLoading)
            {
                AlertManager.ShowInfo("Load operation is already in progress. Please wait...");
                return;
            }

            try
            {
                _isLoading = true;
                btnLoad.Enabled = false;
                btnLoad.Text = "Loading...";

                var confirm = MessageBox.Show(
                    "This will clear existing local data and fetch fresh data from the server.\n\nAre you sure you want to continue?",
                    "Confirm Data Refresh",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (confirm != DialogResult.Yes) return;

                _ = CreateLog("Starting data refresh from API", AlertType.Info);
                AlertManager.ShowInfo("Fetching fresh data from server...");

                // Step 1: Fetch all data from API using ProductCatalogueService
                var response = await _productCatalogueService.GetAllAsync(new ProductCatalogueQueryDto
                {
                    numberOfRecords = 1000, // Adjust based on your needs
                    pageNumber = 1
                });

                var apiProducts = response?.Data?.ToList() ?? new List<ProductCatalogueDto>();

                if (!apiProducts.Any())
                {
                    AlertManager.ShowWarning("No products found on the server.");
                    _ = CreateLog("No products returned from API", AlertType.Warning);
                    return;
                }

                AlertManager.ShowInfo($"Fetched {apiProducts.Count} products from server. Clearing local database...");
                _ = CreateLog($"Fetched {apiProducts.Count} products from API", AlertType.Info);

                // Step 2: Clear old data from local DB using FiscalService
                //var clearResult = await _fiscalService.ClearProductCatalog();

                //if (clearResult.StatusCode != ApiStatusCode.Success)
                //{
                //    AlertManager.ShowError($"Failed to clear local database: {clearResult.Message}");
                //    _ = CreateLog($"Failed to clear local DB: {clearResult.Message}", AlertType.Error);
                //    return;
                //}

                _ = CreateLog("Local database cleared successfully", AlertType.Info);
                AlertManager.ShowInfo($"Saving {apiProducts.Count} products to local database...");

                // Step 3: Save fetched data to local DB using FiscalService
                int successCount = 0;
                int failCount = 0;
                var failedProducts = new List<string>();

                foreach (var dto in apiProducts)
                {
                    try
                    {
                        var output = await _fiscalService.PostProductCatalog(dto);

                        if (output.StatusCode == ApiStatusCode.Success)
                        {
                            successCount++;
                        }
                        else
                        {
                            failCount++;
                            failedProducts.Add($"{dto.ProductCode}: {output.Message}");
                            _ = CreateLog($"Failed to save product {dto.ProductCode}: {output.Message}", AlertType.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        failCount++;
                        failedProducts.Add($"{dto.ProductCode}: {ex.Message}");
                        _ = CreateLog($"Exception saving product {dto.ProductCode}: {ex.Message}", AlertType.Error);
                    }

                    // Update UI periodically
                    if ((successCount + failCount) % 50 == 0)
                    {
                        btnLoad.Text = $"Loading... ({successCount + failCount}/{apiProducts.Count})";
                        Application.DoEvents();
                    }
                }

                var resultMessage = $"Load completed: {successCount} succeeded, {failCount} failed.";

                if (failCount > 0)
                {
                    var failureDetails = string.Join("\n", failedProducts.Take(10));
                    if (failedProducts.Count > 10)
                        failureDetails += $"\n... and {failedProducts.Count - 10} more errors";

                    AlertManager.ShowWarning($"{resultMessage}\n\nFailed items:\n{failureDetails}");
                    _ = CreateLog($"Load completed with errors: {successCount} success, {failCount} failed", AlertType.Warning);
                }
                else
                {
                    AlertManager.ShowSuccess($"Successfully loaded {successCount} products to local database!");
                    _ = CreateLog($"Load completed successfully: {successCount} products saved to local DB", AlertType.Success);
                }

                // Step 4: Refresh the grid from local DB
                _currentPage = 1;
                await LoadFromLocalDB();
            }
            catch (Exception ex)
            {
                AlertManager.ShowError($"Error loading catalogue: {ex.Message}");
                _ = CreateLog($"Critical error loading catalogue: {ex.Message}", AlertType.Error);
            }
            finally
            {
                _isLoading = false;
                btnLoad.Enabled = true;
                btnLoad.Text = "Load";
            }
        }

        /// <summary>
        /// Load products from LOCAL database for display
        /// </summary>
        private async Task LoadFromLocalDB()
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(SearchBox.Text))
                {
                    await FilterProductsFromLocalDB();
                    return;
                }

                // Fetch from local DB using FiscalService
                var response = await _fiscalService.GetProductCatalogue();


                var list = response?.Data ?? Enumerable.Empty<ProductCatalogueDto>();
                PopulateGrid(list);

                lblPageNumber.Text = $"Page {_currentPage}";
                btnNext.Enabled = list.Count() >= _pageSize; // Enable next if full page
                btnPrev.Enabled = _currentPage > 1;
            }
            catch (Exception ex)
            {
                AlertManager.ShowError($"Error loading products from local database: {ex.Message}");
                _ = CreateLog($"Error loading from local DB: {ex.Message}", AlertType.Error);
            }
        }

        /// <summary>
        /// Filter products from LOCAL database based on search
        /// </summary>
        private async Task FilterProductsFromLocalDB()
        {
            try
            {
                string searchText = SearchBox.Text?.Trim();
                if (string.IsNullOrWhiteSpace(searchText))
                {
                    btnNext.Enabled = true;
                    btnPrev.Enabled = _currentPage > 1;
                    await LoadFromLocalDB();
                    return;
                }

                btnNext.Enabled = false;
                btnPrev.Enabled = false;

                // Fetch all items from local DB and filter in memory (original logic)
                var response = await _fiscalService.GetProductCatalogue();

                var allItems = response?.Data ?? Enumerable.Empty<ProductCatalogueDto>();

                var filtered = allItems.Where(p =>
                     (p.ProductCode.HasValue && p.ProductCode.Value.ToString().IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0)
                  || (p.ProductDescription?.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0)
                  || (p.HSCode?.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0)
                ).ToList();

                PopulateGrid(filtered);

                lblPageNumber.Text = $"Search results ({filtered.Count})";
            }
            catch (Exception ex)
            {
                AlertManager.ShowError($"Error searching products: {ex.Message}");
                _ = CreateLog($"Error searching in local DB: {ex.Message}", AlertType.Error);
            }
        }

        private void PopulateGrid(IEnumerable<ProductCatalogueDto> items)
        {
            ProductCatalogueDataGridView.Rows.Clear();

            foreach (var product in items)
            {
                ProductCatalogueDataGridView.Rows.Add(
                    product.ProductCode?.ToString() ?? "",
                    product.ProductDescription ?? "",
                    product.HSCode ?? "",
                    product.SaleType ?? "",
                    product.PosUnitOfMeasurement ?? "",
                    product.TaxRate ?? "",
                    product.SroScheduleNumber ?? "",
                    product.ItemSerialNumber ?? ""
                );
            }
        }

        private async Task NextPage()
        {
            _currentPage++;
            await LoadFromLocalDB();
        }

        private async Task PrevPage()
        {
            if (_currentPage > 1)
            {
                _currentPage--;
                await LoadFromLocalDB();
            }
        }

        private async Task CreateLog(string message, string type)
        {
            try
            {
                var log = new Logs
                {
                    Message = message,
                    Type = type,
                };

                await _logService.LogAsync(log);
            }
            catch
            {
                // Suppress logging errors to avoid cascading failures
            }
        }
    }
}