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

        // Debounce timer to avoid firing search for every keystroke
        private readonly System.Windows.Forms.Timer _searchDebounceTimer;
        private const int SEARCH_DEBOUNCE_MS = 300;
        // How many records to fetch client-side when user is searching.
        // Increase if you expect larger catalog and have API rate/size to allow it.
        private const int SEARCH_FETCH_LIMIT = 2000;
        // Flag to avoid multiple save button clicks
        private bool _isSaving = false;


        public CatalogView(IFiscalService fiscalService, IProductCatalogueService productCatalogueService, ILogService logService)
        {
            InitializeComponent();
            _fiscalService = fiscalService ?? throw new ArgumentNullException(nameof(fiscalService));
            _productCatalogueService = productCatalogueService ?? throw new ArgumentNullException(nameof(productCatalogueService));
            _logService = logService;

            // Enable clipboard copy functionality
            ProductCatalogueDataGridView.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithAutoHeaderText;

            btnSave.Click += btnSave_Click;

            // Debounce timer
            _searchDebounceTimer = new System.Windows.Forms.Timer();
            _searchDebounceTimer.Interval = SEARCH_DEBOUNCE_MS;
            _searchDebounceTimer.Tick += async (s, e) =>
            {
                _searchDebounceTimer.Stop();
                await FilterProducts();
            };

            // Event wiring
            this.Load += async (s, e) => await LoadProductCatalogue();
            SearchBox.TextChanged += (s, e) =>
            {
                // Restart debounce timer on each keypress
                _searchDebounceTimer.Stop();
                _searchDebounceTimer.Start();
            };

            btnNext.Click += async (s, e) => await NextPage();
            btnPrev.Click += async (s, e) => await PrevPage();
        }

        private async void btnSave_Click(object sender, EventArgs e)
        {
            // Prevent multiple simultaneous saves
            if (_isSaving)
            {
                WindowsLocalAppNotification.Show("Information", "Save operation is already in progress. Please wait...");
                AlertManager.ShowInfo("Save operation is already in progress. Please wait...");
                return;
            }

            try
            {
                _isSaving = true;
                btnSave.Enabled = false;
                btnSave.Text = "Saving...";

                if (ProductCatalogueDataGridView.Rows.Count == 0)
                {
                    WindowsLocalAppNotification.Show("Validation Error", "Product Catalogue is empty!");
                    AlertManager.ShowError("Product Catalogue is empty!");
                    _ = CreateLog("Validation Error: Product Catalogue is empty", AlertType.Error);
                    return;
                }

                var confirm = MessageBox.Show(
                    "Are you sure you want to save all catalogue items?",
                    "Confirm Save",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (confirm != DialogResult.Yes) return;

                // -----------------------------
                // Collect all rows from DataGridView into DTOs
                // -----------------------------
                var itemsToSave = new List<ProductCatalogueDto>();
                foreach (DataGridViewRow row in ProductCatalogueDataGridView.Rows)
                {
                    if (row.IsNewRow) continue; // Skip empty row at end

                    var dto = new ProductCatalogueDto
                    {
                        ProductCode = int.TryParse(row.Cells[0].Value?.ToString(), out var code) ? code : null,
                        ProductDescription = row.Cells[1].Value?.ToString(),
                        HSCode = row.Cells[2].Value?.ToString(),
                        SaleType = row.Cells[3].Value?.ToString(),
                        PosUnitOfMeasurement = row.Cells[4].Value?.ToString(),
                        TaxRate = row.Cells[5].Value?.ToString(),
                        SroScheduleNumber = row.Cells[6].Value?.ToString(),
                        ItemSerialNumber = row.Cells[7].Value?.ToString()
                    };

                    itemsToSave.Add(dto);
                }

                if (!itemsToSave.Any())
                {
                    WindowsLocalAppNotification.Show("Validation Error", "No valid items found in catalogue.");
                    AlertManager.ShowError("No valid items found in catalogue.");
                    _ = CreateLog("Validation Error: No valid items", AlertType.Error);
                    return;
                }

                AlertManager.ShowInfo("Saving catalogue to local database...");
                _ = CreateLog("Saving product catalogue", AlertType.Info);

                int successCount = 0;
                int failCount = 0;

                foreach (var dto in itemsToSave)
                {
                    var output = await _fiscalService.PostProductCatalog(dto);

                    if (output.StatusCode == ApiStatusCode.Success)
                    {
                        successCount++;
                        _ = CreateLog($"Saved product {dto.ProductDescription}", AlertType.Info);
                    }
                    else
                    {
                        failCount++;
                        _ = CreateLog($"Failed to save product {dto.ProductDescription}: {output.Message}", AlertType.Error);
                    }
                }

                WindowsLocalAppNotification.Show("Save Completed", $"Saved {successCount} products, failed {failCount}.");
                AlertManager.ShowInfo($"Saved {successCount} products, failed {failCount}.");

                if (failCount == 0)
                {
                    // clear only if everything succeeded
                    ProductCatalogueDataGridView.Rows.Clear();
                    lblPageNumber.Text = "Page 1";
                    _currentPage = 1;
                }

            }
            catch (Exception ex)
            {
                WindowsLocalAppNotification.Show("Error", $"Error saving catalogue: {ex.Message}");
                AlertManager.ShowError($"Error saving catalogue: {ex.Message}");
                _ = CreateLog("Error saving catalogue", AlertType.Error);
            }
            finally
            {
                // Always restore button state
                _isSaving = false;
                btnSave.Enabled = true;
                btnSave.Text = "Save";
            }
        }
        private async Task CreateLog(string message, string type)
        {
            var log = new Logs
            {
                Message = message,   // pass any message
                Type = type,         // comes from AlertType constants

            };

            await _logService.LogAsync(log);
        }

        private async Task LoadProductCatalogue()
        {
            try
            {
                // If a search is active, we use FilterProducts instead
                if (!string.IsNullOrWhiteSpace(SearchBox.Text))
                {
                    await FilterProducts();
                    return;
                }

                var response = await _productCatalogueService.GetAllAsync(new ProductCatalogueQueryDto
                {
                    numberOfRecords = _pageSize,
                    pageNumber = _currentPage
                });

                var list = response?.Data ?? Enumerable.Empty<ProductCatalogueDto>();
                PopulateGrid(list);

                lblPageNumber.Text = $"Page {_currentPage}";
                // Pagination available when not searching
                btnNext.Enabled = true;
                btnPrev.Enabled = _currentPage > 1;
            }
            catch (Exception ex)
            {
                // optional: show notification
                AlertManager.ShowError($"Error loading products: {ex.Message}");
            }
        }

        private async Task FilterProducts()
        {
            try
            {
                string searchText = SearchBox.Text?.Trim();
                if (string.IsNullOrWhiteSpace(searchText))
                {
                    // no search => load page normally
                    btnNext.Enabled = true;
                    btnPrev.Enabled = _currentPage > 1;
                    await LoadProductCatalogue();
                    return;
                }

                // Disable paging controls while searching (client-side)
                btnNext.Enabled = false;
                btnPrev.Enabled = false;

                // Fetch a reasonably large chunk for client-side filtering.
                // If you have server search, replace this with a server call that accepts a filter.
                var response = await _productCatalogueService.GetAllAsync(new ProductCatalogueQueryDto
                {
                    numberOfRecords = SEARCH_FETCH_LIMIT,
                    pageNumber = 1
                });

                var allItems = response?.Data ?? Enumerable.Empty<ProductCatalogueDto>();

                // Case-insensitive check across ProductCode, ProductDescription, and HSCode only
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
                AlertManager.ShowError($"Error filtering products: {ex.Message}");
            }
        }

        // Small helper to keep grid population logic in one place
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
            await LoadProductCatalogue();
        }

        private async Task PrevPage()
        {
            if (_currentPage > 1)
            {
                _currentPage--;
                await LoadProductCatalogue();
            }
        }
    }
}