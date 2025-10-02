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
        private readonly IProductCatalogueService _productCatalogueService;
        private int _currentPage = 1;
        private int _pageSize = 50;

        private readonly System.Windows.Forms.Timer _searchDebounceTimer;
        private const int SEARCH_DEBOUNCE_MS = 300;
        private const int SEARCH_FETCH_LIMIT = 2000;
        private bool _isSaving = false;

        public CatalogView(IProductCatalogueService productCatalogueService, ILogService logService)
        {
            InitializeComponent();
            _productCatalogueService = productCatalogueService ?? throw new ArgumentNullException(nameof(productCatalogueService));
            _logService = logService;

            ProductCatalogueDataGridView.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithAutoHeaderText;

            btnSave.Click += btnSave_Click;

            _searchDebounceTimer = new System.Windows.Forms.Timer();
            _searchDebounceTimer.Interval = SEARCH_DEBOUNCE_MS;
            _searchDebounceTimer.Tick += async (s, e) =>
            {
                _searchDebounceTimer.Stop();
                await FilterProducts();
            };

            this.Load += async (s, e) => await LoadProductCatalogue();
            SearchBox.TextChanged += (s, e) =>
            {
                _searchDebounceTimer.Stop();
                _searchDebounceTimer.Start();
            };

            btnNext.Click += async (s, e) => await NextPage();
            btnPrev.Click += async (s, e) => await PrevPage();
        }

        private async void btnSave_Click(object sender, EventArgs e)
        {
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

                // Collect all rows from DataGridView into DTOs
                var itemsToSave = new List<ProductCatalogueDto>();
                var invalidRows = new List<string>();

                foreach (DataGridViewRow row in ProductCatalogueDataGridView.Rows)
                {
                    if (row.IsNewRow) continue;

                    // Validate ProductCode exists and is valid
                    if (!long.TryParse(row.Cells[0].Value?.ToString(), out var code))
                    {
                        invalidRows.Add($"Row {row.Index + 1}: Invalid or missing Product Code");
                        continue;
                    }

                    var dto = new ProductCatalogueDto
                    {
                        ProductCode = code,
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

                if (invalidRows.Any())
                {
                    var invalidMessage = string.Join("\n", invalidRows.Take(5));
                    if (invalidRows.Count > 5)
                        invalidMessage += $"\n... and {invalidRows.Count - 5} more";

                    WindowsLocalAppNotification.Show("Validation Warning", $"Some rows have invalid data:\n{invalidMessage}");
                    AlertManager.ShowWarning($"Found {invalidRows.Count} invalid rows. They will be skipped.");
                    _ = CreateLog($"Validation: {invalidRows.Count} invalid rows found", AlertType.Warning);
                }

                if (!itemsToSave.Any())
                {
                    WindowsLocalAppNotification.Show("Validation Error", "No valid items found in catalogue.");
                    AlertManager.ShowError("No valid items found in catalogue.");
                    _ = CreateLog("Validation Error: No valid items", AlertType.Error);
                    return;
                }

                // Check for duplicates in the current batch
                var duplicateCodes = itemsToSave
                    .GroupBy(x => x.ProductCode)
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key)
                    .ToList();

                if (duplicateCodes.Any())
                {
                    WindowsLocalAppNotification.Show("Duplicate Error",
                        $"Found duplicate Product Codes in the grid: {string.Join(", ", duplicateCodes.Take(5))}");
                    AlertManager.ShowError($"Please remove duplicate Product Codes before saving.");
                    _ = CreateLog($"Duplicate Product Codes found: {string.Join(", ", duplicateCodes)}", AlertType.Error);
                    return;
                }

                // Fetch existing product codes from database
                AlertManager.ShowInfo("Checking for existing products...");
                var existingProducts = await GetExistingProductCodes();
                var existingCodesSet = new HashSet<long>(existingProducts);

                // Separate new vs existing products
                var newProducts = itemsToSave.Where(p => !existingCodesSet.Contains(p.ProductCode.Value)).ToList();
                var existingProductsToUpdate = itemsToSave.Where(p => existingCodesSet.Contains(p.ProductCode.Value)).ToList();

                if (existingProductsToUpdate.Any())
                {
                    var existingCodes = string.Join(", ", existingProductsToUpdate.Select(p => p.ProductCode).Take(10));
                    if (existingProductsToUpdate.Count > 10)
                        existingCodes += $"... and {existingProductsToUpdate.Count - 10} more";

                    // Use Invoke to ensure MessageBox is shown on UI thread properly
                    DialogResult updateConfirm = DialogResult.Cancel;

                    if (InvokeRequired)
                    {
                        Invoke(new Action(() =>
                        {
                            updateConfirm = MessageBox.Show(
                                this,
                                $"Found {existingProductsToUpdate.Count} products that already exist in the database:\n{existingCodes}\n\n" +
                                $"Do you want to update these existing products?\n\n" +
                                $"Yes = Update existing products\n" +
                                $"No = Skip existing products\n" +
                                $"Cancel = Abort save operation",
                                "Existing Products Found",
                                MessageBoxButtons.YesNoCancel,
                                MessageBoxIcon.Question);
                        }));
                    }
                    else
                    {
                        updateConfirm = MessageBox.Show(
                            this,
                            $"Found {existingProductsToUpdate.Count} products that already exist in the database:\n{existingCodes}\n\n" +
                            $"Do you want to update these existing products?\n\n" +
                            $"Yes = Update existing products\n" +
                            $"No = Skip existing products\n" +
                            $"Cancel = Abort save operation",
                            "Existing Products Found",
                            MessageBoxButtons.YesNoCancel,
                            MessageBoxIcon.Question);
                    }

                    if (updateConfirm == DialogResult.Cancel)
                    {
                        AlertManager.ShowInfo("Save operation cancelled by user.");
                        _ = CreateLog("Save operation cancelled by user", AlertType.Info);
                        return;
                    }

                    if (updateConfirm == DialogResult.No)
                    {
                        // Skip existing products - only save new ones
                        itemsToSave = newProducts;
                        AlertManager.ShowInfo($"Skipping {existingProductsToUpdate.Count} existing products. Saving {newProducts.Count} new products.");
                        _ = CreateLog($"Skipping {existingProductsToUpdate.Count} existing, saving {newProducts.Count} new", AlertType.Info);
                    }
                    else
                    {
                        // Yes was selected - will attempt to save all (may fail on duplicates)
                        AlertManager.ShowInfo($"Attempting to save {itemsToSave.Count} products (including {existingProductsToUpdate.Count} existing)...");
                        _ = CreateLog($"Attempting to save/update {itemsToSave.Count} products", AlertType.Info);
                    }
                }
                else
                {
                    AlertManager.ShowInfo($"Saving {newProducts.Count} new products to database...");
                    _ = CreateLog($"Saving {newProducts.Count} new products", AlertType.Info);
                }

                if (!itemsToSave.Any())
                {
                    AlertManager.ShowInfo("No products to save after filtering.");
                    _ = CreateLog("No products to save after filtering", AlertType.Info);
                    return;
                }

                _ = CreateLog($"Starting save: {itemsToSave.Count} products", AlertType.Info);

                int successCount = 0;
                int failCount = 0;
                var failedProducts = new List<string>();

                foreach (var dto in itemsToSave)
                {
                    try
                    {
                        var output = await _productCatalogueService.PostProductCatalog(dto);

                        if (output.StatusCode == ApiStatusCode.Success)
                        {
                            successCount++;
                            _ = CreateLog($"Saved product {dto.ProductCode}: {dto.ProductDescription}", AlertType.Info);
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

                    // Update UI periodically to show progress
                    if ((successCount + failCount) % 10 == 0)
                    {
                        btnSave.Text = $"Saving... ({successCount + failCount}/{itemsToSave.Count})";
                        Application.DoEvents();
                    }
                }

                var resultMessage = $"Saved {successCount} products successfully.";

                if (failCount > 0)
                {
                    resultMessage = $"Save completed: {successCount} succeeded, {failCount} failed.";
                }

                WindowsLocalAppNotification.Show("Save Completed", resultMessage);

                if (failCount > 0)
                {
                    // Show detailed failure info separately
                    var failureDetails = string.Join("\n", failedProducts.Take(10));
                    if (failedProducts.Count > 10)
                        failureDetails += $"\n... and {failedProducts.Count - 10} more errors";

                    AlertManager.ShowWarning($"{resultMessage}\n\nFailed items:\n{failureDetails}");

                    // Log all failures
                    _ = CreateLog($"Save completed with errors: {successCount} success, {failCount} failed. First error: {failedProducts.FirstOrDefault()}", AlertType.Warning);
                }
                else
                {
                    AlertManager.ShowSuccess($"Successfully saved all {successCount} products!");
                    _ = CreateLog($"Save completed successfully: {successCount} products saved", AlertType.Info);
                }

                // Refresh grid if all succeeded
                if (failCount == 0 && successCount > 0)
                {
                    _currentPage = 1;
                    lblPageNumber.Text = "Page 1";
                    await LoadProductCatalogue();
                }
            }
            catch (Exception ex)
            {
                WindowsLocalAppNotification.Show("Error", $"Error saving catalogue: {ex.Message}");
                AlertManager.ShowError($"Error saving catalogue: {ex.Message}");
                _ = CreateLog($"Critical error saving catalogue: {ex.Message}", AlertType.Error);
            }
            finally
            {
                _isSaving = false;
                btnSave.Enabled = true;
                btnSave.Text = "Save";
            }
        }

        private async Task<List<long>> GetExistingProductCodes()
        {
            try
            {
                // Fetch all existing products (or a large batch)
                var response = await _productCatalogueService.GetAllAsync(new ProductCatalogueQueryDto
                {
                    numberOfRecords = 10000, // Adjust based on your catalog size
                    pageNumber = 1
                });

                return response?.Data?
                    .Where(p => p.ProductCode.HasValue)
                    .Select(p => p.ProductCode.Value)
                    .ToList() ?? new List<long>();
            }
            catch (Exception ex)
            {
                _ = CreateLog($"Error fetching existing product codes: {ex.Message}", AlertType.Warning);
                return new List<long>(); // Return empty list on error, letting duplicates be caught by DB
            }
        }

        private async Task CreateLog(string message, string type)
        {
            var log = new Logs
            {
                Message = message,
                Type = type,
            };

            await _logService.LogAsync(log);
        }

        private async Task LoadProductCatalogue()
        {
            try
            {
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
                btnNext.Enabled = true;
                btnPrev.Enabled = _currentPage > 1;
            }
            catch (Exception ex)
            {
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
                    btnNext.Enabled = true;
                    btnPrev.Enabled = _currentPage > 1;
                    await LoadProductCatalogue();
                    return;
                }

                btnNext.Enabled = false;
                btnPrev.Enabled = false;

                var response = await _productCatalogueService.GetAllAsync(new ProductCatalogueQueryDto
                {
                    numberOfRecords = SEARCH_FETCH_LIMIT,
                    pageNumber = 1
                });

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
                AlertManager.ShowError($"Error filtering products: {ex.Message}");
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