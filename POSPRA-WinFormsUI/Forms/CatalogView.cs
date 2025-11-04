using POSPRA.Application.Services.LogService;
using POSPRA.Application.Services.ProductCatalogService;
using POSPRA.Application.Utility;
using POSPRA.Domain.Entities;
using POSPRA.DTOs.LogDTOs;
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
        private int _isLoadingFlag = 0;
        private bool _isLoading = false;
        private const int SEARCH_FETCH_LIMIT = 2000;
        private readonly System.Windows.Forms.Timer _searchDebounceTimer;
        private const int SEARCH_DEBOUNCE_MS = 300;


        public CatalogView(IProductCatalogueService productCatalogueService, ILogService logService)
        {
            InitializeComponent();

            this.Load += (s, e) => CenterProgressBar();
            this.Resize += (s, e) => CenterProgressBar();

            _productCatalogueService = productCatalogueService ?? throw new ArgumentNullException(nameof(productCatalogueService));
            _logService = logService;

            ProductCatalogueDataGridView.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithAutoHeaderText;

            if (progressBar != null) progressBar.Visible = false;

            btnLoad.Click += btnLoad_Click;

            _searchDebounceTimer = new System.Windows.Forms.Timer();
            _searchDebounceTimer.Interval = SEARCH_DEBOUNCE_MS;
            _searchDebounceTimer.Tick += async (s, e) =>
            {
                _searchDebounceTimer.Stop();

                if (string.IsNullOrWhiteSpace(SearchBox.Text))
                {
                    await LoadFromLocalDB();
                }
                else
                {
                    await FilterProductsFromLocalDB();
                }
            };

            this.Load += async (s, e) => await LoadFromLocalDB();

            // ✅ Use named event handler for easy removal
            SearchBox.TextChanged += SearchBox_TextChanged;

            btnNext.Click += async (s, e) => await NextPage();
            btnPrev.Click += async (s, e) => await PrevPage();
            StyleProductDataGridView();
        }

        #region progress bar
        private void CenterProgressBar()
        {
            if (progressBar != null && ProductCatalogueDataGridView != null)
            {
                var gridBounds = ProductCatalogueDataGridView.Bounds;
                progressBar.Left = gridBounds.Left + (gridBounds.Width - progressBar.Width) / 2;
                progressBar.Top = gridBounds.Top + (gridBounds.Height - progressBar.Height) / 2;
                progressBar.BringToFront();
            }
        }

        private async Task RunSingleLoad(Func<Task> work)
        {
            if (Interlocked.Exchange(ref _isLoadingFlag, 1) == 1) return;

            try
            {
                if (progressBar != null)
                {
                    progressBar.InvokeIfRequired(() =>
                    {
                        progressBar.Style = ProgressBarStyle.Marquee;
                        progressBar.MarqueeAnimationSpeed = 30;
                        progressBar.Visible = true;
                        progressBar.BringToFront();
                        progressBar.Update();
                    });
                }

                await work();
            }
            finally
            {
                try
                {
                    if (progressBar != null)
                    {
                        progressBar.InvokeIfRequired(() =>
                        {
                            progressBar.Visible = false;
                            progressBar.Style = ProgressBarStyle.Continuous;
                        });
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠ Progress bar cleanup failed: {ex.Message}");
                }
                finally
                {
                    Interlocked.Exchange(ref _isLoadingFlag, 0);
                }
            }
        }

        #endregion




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

            // Disable button immediately
            btnLoad.Enabled = false;
            btnLoad.Text = "Loading...";

            var confirm = MessageBox.Show(
                "This will clear existing local data and fetch fresh data from the server.\n\nAre you sure you want to continue?",
                "Confirm Data Refresh",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirm != DialogResult.Yes)
            {
                // Re-enable button if user cancels
                btnLoad.Enabled = true;
                btnLoad.Text = "🔄Sync Products From Cloud";
                return;
            }

            await RunSingleLoad(async () =>
            {
                try
                {
                    _isLoading = true;

                    // STEP 0: Perform hard reset BEFORE starting
                    await PerformHardReset();

                    _ = CreateLog("Starting data refresh from API", AlertType.Info);
                    AlertManager.ShowInfo("Fetching fresh data from server...");

                    // Step 1: Fetch all data from API using ProductCatalogueService
                    var response = await _productCatalogueService.GetAllAsync(new ProductCatalogueQueryDto
                    {
                        numberOfRecords = 1000,
                        pageNumber = 1
                    });

                    var apiProducts = response?.Data?.ToList() ?? new List<ProductCatalogueDto>();

                    if (!apiProducts.Any())
                    {
                        AlertManager.ShowWarning("No products found on the server.");
                        _ = CreateLog("No products returned from API", AlertType.Warning);

                        // ✅ Hard reset after failure
                        await PerformHardReset();
                        return;
                    }

                    AlertManager.ShowInfo($"Fetched {apiProducts.Count} products from server. Clearing local database...");
                    _ = CreateLog($"Fetched {apiProducts.Count} products from API", AlertType.Info);

                    // Step 2: Clear old data from local DB using FiscalService
                    var clearResult = await _productCatalogueService.DeleteProductCatalogue();

                    if (clearResult.StatusCode != ApiStatusCode.Success && clearResult.StatusCode != ApiStatusCode.NotFound)
                    {
                        AlertManager.ShowError($"Failed to clear local database: {clearResult.Message}");
                        _ = CreateLog($"Failed to clear local DB: {clearResult.Message}", AlertType.Error);

                        // ✅ Hard reset after failure
                        await PerformHardReset();
                        return;
                    }

                    _ = CreateLog("Local database cleared (or already empty)", AlertType.Info);
                    AlertManager.ShowInfo($"Saving {apiProducts.Count} products to local database...");

                    // Update progress bar for saving
                    if (progressBar != null && apiProducts.Count > 0)
                    {
                        progressBar.Style = ProgressBarStyle.Continuous;
                        progressBar.Minimum = 0;
                        progressBar.Maximum = apiProducts.Count;
                        progressBar.Value = 0;
                        progressBar.Visible = true;
                        progressBar.Update();
                    }

                    // Step 3: Save fetched data to local DB using FiscalService
                    int successCount = 0;
                    int failCount = 0;
                    var failedProducts = new List<string>();

                    foreach (var dto in apiProducts)
                    {
                        try
                        {
                            var output = await _productCatalogueService.PostProductCatalog(dto);

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

                        // Update progress bar
                        if (progressBar != null)
                        {
                            progressBar.Value = Math.Min(successCount + failCount, progressBar.Maximum);
                            progressBar.Update();
                        }

                        // Update button text periodically
                        if ((successCount + failCount) % 10 == 0)
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

                    // ✅ STEP 4: Perform hard reset AFTER loading
                    await PerformHardReset();

                    // ✅ STEP 5: Reload fresh data from local DB
                    _currentPage = 1;
                    await LoadFromLocalDB();

                    // ✅ STEP 6: Final UI update
                    AlertManager.ShowSuccess("Data refresh completed! All filters and states have been reset.");
                }
                catch (Exception ex)
                {
                    AlertManager.ShowError($"Error loading catalogue: {ex.Message}");
                    _ = CreateLog($"Critical error loading catalogue: {ex.Message}", AlertType.Error);

                    // ✅ Hard reset after exception
                    await PerformHardReset();
                }
                finally
                {
                    _isLoading = false;
                    btnLoad.Enabled = true;
                    btnLoad.Text = "🔄Sync Products From Cloud";

                    if (progressBar != null)
                    {
                        progressBar.Visible = false;
                    }
                }
            });
        }
        /// <summary>
        /// Load products from LOCAL database for display
        /// </summary>
        private async Task LoadFromLocalDB()
        {
            await RunSingleLoad(async () =>
            {
                try
                {
                    var response = await _productCatalogueService.GetProductCatalogue();
                    var allItems = response?.Data?.OrderBy(p => p.ItemSerialNumber).ToList() ?? new List<ProductCatalogueDto>();

                    // ✅ Compute total pages
                    int totalRecords = allItems.Count;
                    int totalPages = (int)Math.Ceiling((double)totalRecords / _pageSize);
                    lblTotalRecords.Text = $"Total {Convert.ToString(totalRecords)} Products";
                    if (_currentPage > totalPages && totalPages > 0)
                        _currentPage = totalPages;

                    // ✅ Apply pagination
                    var pageData = allItems
                        .Skip((_currentPage - 1) * _pageSize)
                        .Take(_pageSize)
                        .ToList();

                    // ✅ Bind data to grid
                    PopulateGrid(pageData);

                    // ✅ Update pagination controls
                    lblPageNumber.Text = $"Page {_currentPage} of {totalPages}";
                    btnNext.Enabled = _currentPage < totalPages;
                    btnPrev.Enabled = _currentPage > 1;
                }
                catch (Exception ex)
                {
                    AlertManager.ShowError($"Error loading products from local database: {ex.Message}");
                    _ = CreateLog($"Error loading from local DB: {ex.Message}", AlertType.Error);
                }
            });
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


        /// <summary>
        /// Filter products from LOCAL database based on search
        /// </summary>
        private async Task FilterProductsFromLocalDB()
        {
            _currentPage = 1;
            await RunSingleLoad(async () =>
            {
                try
                {
                    string searchText = SearchBox.Text?.Trim();
                    if (string.IsNullOrWhiteSpace(searchText))
                    {
                        // If search is cleared, reload full data
                        btnNext.Enabled = true;
                        btnPrev.Enabled = _currentPage > 1;

                        var response = await _productCatalogueService.GetProductCatalogue();
                        var list = response?.Data ?? Enumerable.Empty<ProductCatalogueDto>();
                        PopulateGrid(list);
                        lblPageNumber.Text = $"Page {_currentPage}";
                        return;
                    }

                    btnNext.Enabled = false;
                    btnPrev.Enabled = false;

                    // Fetch all items from local DB and filter in memory (original logic)
                    var allResponse = await _productCatalogueService.GetProductCatalogue();

                    var allItems = allResponse?.Data ?? Enumerable.Empty<ProductCatalogueDto>();

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
            });
        }

        private void PopulateGrid(IEnumerable<ProductCatalogueDto> items)
        {
            ProductCatalogueDataGridView.Rows.Clear();

            foreach (var product in items)
            {
                int rowIndex = ProductCatalogueDataGridView.Rows.Add();
                var row = ProductCatalogueDataGridView.Rows[rowIndex];

                void SetCell(string columnName, object? value)
                {
                    if (ProductCatalogueDataGridView.Columns.Contains(columnName))
                        row.Cells[columnName].Value = value ?? "";
                    else
                        Console.WriteLine($"⚠ Missing column: {columnName}");
                }

                SetCell("colItemSrno", product.ItemSerialNumber);
                SetCell("colProductCode", product.ProductCode);
                SetCell("colProductDesc", product.ProductDescription);
                SetCell("colHScode", product.HSCode);
                SetCell("colSaleType", product.SaleType);
                SetCell("colPosUOM", product.PosUnitOfMeasurement);
                SetCell("colTaxRate", product.TaxRate);
                SetCell("colSROno", product.SroScheduleNumber);
            }
        }

        private async Task CreateLog(string message, string type)
        {
            try
            {
                var log = new CreateLogDto
                {
                    Message = message,
                    Type = type,
                };

                await _logService.CreateLogAsync(log);
            }
            catch
            {
                // Suppress logging errors to avoid cascading failures
            }
        }

        #region HardReset
        private void SearchBox_TextChanged(object sender, EventArgs e)
        {
            _searchDebounceTimer.Stop();
            _searchDebounceTimer.Start();
        }

        private async Task PerformHardReset()
        {
            try
            {
                // 1. Stop any ongoing operations
                _searchDebounceTimer?.Stop();

                // 2. Reset all state variables
                _currentPage = 1;
                _isLoading = false;
                _isLoadingFlag = 0;

                // 3. Clear and reset search box
                SearchBox.TextChanged -= SearchBox_TextChanged; // Temporarily remove handler
                SearchBox.Clear();
                SearchBox.Text = string.Empty;
                SearchBox.TextChanged += SearchBox_TextChanged; // Re-add handler

                // 4. Clear DataGridView completely
                ProductCatalogueDataGridView.DataSource = null;
                ProductCatalogueDataGridView.Rows.Clear();
                ProductCatalogueDataGridView.Refresh();

                // 5. Reset pagination controls
                lblPageNumber.Text = "Page 1";
                btnNext.Enabled = false;
                btnPrev.Enabled = false;

                // 6. Reset button states
                btnLoad.Enabled = true;
                btnLoad.Text = "Load";

                // 7. Hide progress bar
                if (progressBar != null)
                {
                    progressBar.Visible = false;
                    progressBar.Value = 0;
                    progressBar.Style = ProgressBarStyle.Continuous;
                }

                // 8. Force garbage collection to free memory
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                // 9. Small delay to ensure UI updates
                await Task.Delay(100);

                // 10. Force form refresh
                this.Refresh();
                Application.DoEvents();

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during hard reset: {ex.Message}");
            }
        }
        #endregion


        #region datagrid style

        private void StyleDataGridView(DataGridView dgv)
        {
            // General settings
            dgv.BorderStyle = BorderStyle.None;
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
            dgv.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgv.GridColor = Color.FromArgb(226, 232, 240);
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(59, 130, 246);
            dgv.DefaultCellStyle.SelectionForeColor = Color.White;
            dgv.BackgroundColor = Color.White;
            dgv.RowHeadersVisible = false;
            dgv.EnableHeadersVisualStyles = false;
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            dgv.AllowUserToResizeRows = false;
            dgv.ReadOnly = true;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.MultiSelect = false;

            // Prevent selecting headers
            dgv.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(51, 51, 51);
            dgv.ColumnHeadersDefaultCellStyle.SelectionForeColor = Color.White;
            dgv.RowHeadersDefaultCellStyle.SelectionBackColor = Color.White;
            dgv.RowHeadersDefaultCellStyle.SelectionForeColor = Color.Black;

            // Column header style
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(51, 51, 51);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dgv.ColumnHeadersDefaultCellStyle.Padding = new Padding(12, 10, 12, 10);
            dgv.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgv.ColumnHeadersHeight = 52;
            dgv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;

            // Cell style
            dgv.DefaultCellStyle.BackColor = Color.White;
            dgv.DefaultCellStyle.ForeColor = Color.FromArgb(55, 65, 81);
            dgv.DefaultCellStyle.Font = new Font("Segoe UI", 10F);
            dgv.DefaultCellStyle.Padding = new Padding(12, 6, 12, 6);
            dgv.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

            // Adjust row height
            dgv.RowTemplate.Height = dgv.DefaultCellStyle.Font.Height + dgv.DefaultCellStyle.Padding.Vertical + 12;
        }

        private void StyleProductDataGridView()
        {
            // Clear existing columns
            ProductCatalogueDataGridView.Columns.Clear();

            // Apply base style
            StyleDataGridView(ProductCatalogueDataGridView);

            // Define product catalogue columns
            var colItemSrno = new DataGridViewTextBoxColumn
            {
                Name = "colItemSrno",
                HeaderText = "Sr. No.",
                Width = 80,
                ReadOnly = true,
                SortMode = DataGridViewColumnSortMode.Automatic,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    ForeColor = Color.FromArgb(107, 114, 128),
                    Font = new Font("Segoe UI", 9F)
                }
            };

            var colProductCode = new DataGridViewTextBoxColumn
            {
                Name = "colProductCode",
                HeaderText = "Product Code",
                Width = 150,
                ReadOnly = true,
                SortMode = DataGridViewColumnSortMode.Automatic,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(30, 58, 138)
                }
            };

            var colProductDesc = new DataGridViewTextBoxColumn
            {
                Name = "colProductDesc",
                HeaderText = "Product Description",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                MinimumWidth = 240,
                ReadOnly = true,
                SortMode = DataGridViewColumnSortMode.Automatic
            };

            var colHScode = new DataGridViewTextBoxColumn
            {
                Name = "colHScode",
                HeaderText = "HS Code",
                Width = 130,
                ReadOnly = true,
                SortMode = DataGridViewColumnSortMode.Automatic,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Font = new Font("Consolas", 9.5F),
                    ForeColor = Color.FromArgb(55, 65, 81)
                }
            };

            var colSaleType = new DataGridViewTextBoxColumn
            {
                Name = "colSaleType",
                HeaderText = "Sale Type",
                Width = 120,
                ReadOnly = true,
                SortMode = DataGridViewColumnSortMode.Automatic
            };

            var colPosUOM = new DataGridViewTextBoxColumn
            {
                Name = "colPosUOM",
                HeaderText = "POS UOM",
                Width = 100,
                ReadOnly = true,
                SortMode = DataGridViewColumnSortMode.Automatic,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Alignment = DataGridViewContentAlignment.MiddleCenter
                }
            };

            var colTaxRate = new DataGridViewTextBoxColumn
            {
                Name = "colTaxRate",
                HeaderText = "Tax Rate (%)",
                Width = 110,
                ReadOnly = true,
                SortMode = DataGridViewColumnSortMode.Automatic,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Alignment = DataGridViewContentAlignment.MiddleRight,
                    Format = "0.00",
                    ForeColor = Color.FromArgb(5, 150, 105),
                    Font = new Font("Segoe UI", 9.5F, FontStyle.Bold)
                }
            };

            var colSROno = new DataGridViewTextBoxColumn
            {
                Name = "colSROno",
                HeaderText = "SRO Schedule No.",
                Width = 150,
                ReadOnly = true,
                SortMode = DataGridViewColumnSortMode.Automatic
            };

            // Add all columns
            ProductCatalogueDataGridView.Columns.AddRange(new DataGridViewColumn[]
            {
                colItemSrno,
                colProductCode,
                colProductDesc,
                colHScode,
                colSaleType,
                colPosUOM,
                colTaxRate,
                colSROno
            });
        }

        #endregion
    }
    public static class ControlExtensions
    {
        public static void InvokeIfRequired(this Control control, Action action)
        {
            if (control.InvokeRequired)
            {
                try { control.Invoke(action); }
                catch (ObjectDisposedException) { }
            }
            else
            {
                action();
            }
        }
    }
}