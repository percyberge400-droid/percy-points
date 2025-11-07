using POSPRA.Application.Services.LogService;
using POSPRA.Application.Services.ProductCatalogService;
using POSPRA.Application.Utility;
using POSPRA.DTOs.ProductCatalogDtos;
using POSPRA_WinFormsUI.AlertClasses;

namespace POSPRA_WinFormsUI.Forms
{
    public partial class CatalogView : Form
    {
        private readonly ILogService _logService;
        private readonly IProductCatalogueService _productCatalogueService;

        // State management
        private int _currentPage = 1;
        private int _pageSize = 50;
        private CancellationTokenSource _currentOperationCts;
        private readonly object _loadingLock = new object();
        private bool _isLoading = false;

        // Search
        private System.Windows.Forms.Timer _searchDebounceTimer; // Remove readonly
        private const int SEARCH_DEBOUNCE_MS = 300;
        private string _lastSearchTerm = string.Empty;

        public CatalogView(IProductCatalogueService productCatalogueService, ILogService logService)
        {
            InitializeComponent();
            _productCatalogueService = productCatalogueService ?? throw new ArgumentNullException(nameof(productCatalogueService));
            _logService = logService;

            InitializeComponentEvents();
            StyleProductDataGridView();
        }

        private void InitializeComponentEvents()
        {
            this.Load += async (s, e) => await SafeLoadFromLocalDB();
            this.Resize += (s, e) => CenterProgressBar();

            ProductCatalogueDataGridView.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithAutoHeaderText;
            if (progressBar != null) progressBar.Visible = false;

            btnLoad.Click += btnLoad_Click;

            // Search debounce - initialize here instead of field initializer
            _searchDebounceTimer = new System.Windows.Forms.Timer();
            _searchDebounceTimer.Interval = SEARCH_DEBOUNCE_MS;
            _searchDebounceTimer.Tick += async (s, e) =>
            {
                _searchDebounceTimer.Stop();
                await SafeSearchProducts();
            };

            // Remove duplicate event handler registration
            SearchBox.TextChanged += SearchBox_TextChanged_Handler;
            btnNext.Click += async (s, e) => await SafeNextPage();
            btnPrev.Click += async (s, e) => await SafePrevPage();
        }

        #region State Management & Cancellation
        private bool BeginOperation()
        {
            lock (_loadingLock)
            {
                if (_isLoading) return false;
                _isLoading = true;

                // Cancel any previous operation
                _currentOperationCts?.Cancel();
                _currentOperationCts = new CancellationTokenSource();
                return true;
            }
        }

        private void EndOperation()
        {
            lock (_loadingLock)
            {
                _isLoading = false;
            }
        }

        private void CancelCurrentOperation()
        {
            _currentOperationCts?.Cancel();
        }

        private bool IsOperationCanceled()
        {
            return _currentOperationCts?.Token.IsCancellationRequested ?? false;
        }
        #endregion

        #region Safe Wrappers for Async Operations
        private async Task SafeLoadFromLocalDB()
        {
            if (!BeginOperation()) return;

            try
            {
                await RunWithProgressBar(async () =>
                {
                    await LoadFromLocalDB(_currentOperationCts.Token);
                });
            }
            catch (OperationCanceledException)
            {
                // Operation was cancelled - this is normal
            }
            catch (Exception ex)
            {
                AlertManager.ShowError($"Error loading products: {ex.Message}");
                await CreateLog($"Error loading from local DB: {ex.Message}", "Error");
            }
            finally
            {
                EndOperation();
            }
        }

        private async Task SafeSearchProducts()
        {
            if (!BeginOperation()) return;

            try
            {
                await RunWithProgressBar(async () =>
                {
                    await FilterProductsFromLocalDB(_currentOperationCts.Token);
                });
            }
            catch (OperationCanceledException)
            {
                // Operation was cancelled - this is normal
            }
            catch (Exception ex)
            {
                AlertManager.ShowError($"Error searching products: {ex.Message}");
                await CreateLog($"Error searching products: {ex.Message}", "Error");
            }
            finally
            {
                EndOperation();
            }
        }

        private async Task SafeNextPage()
        {
            _currentPage++;
            await SafeLoadFromLocalDB();
        }

        private async Task SafePrevPage()
        {
            if (_currentPage > 1)
            {
                _currentPage--;
                await SafeLoadFromLocalDB();
            }
        }
        #endregion

        #region Progress Bar Management
        private async Task RunWithProgressBar(Func<Task> work)
        {
            try
            {
                progressBar.InvokeIfRequired(() =>
                {
                    progressBar.Style = ProgressBarStyle.Marquee;
                    progressBar.MarqueeAnimationSpeed = 30;
                    progressBar.Visible = true;
                    progressBar.BringToFront();
                    CenterProgressBar();
                });

                await work();
            }
            finally
            {
                progressBar.InvokeIfRequired(() =>
                {
                    progressBar.Visible = false;
                    progressBar.Style = ProgressBarStyle.Continuous;
                });
            }
        }

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
        #endregion

        #region Core Operations
        private async void btnLoad_Click(object sender, EventArgs e)
        {
            var confirm = MessageBox.Show(
                "This will clear existing local data and fetch fresh data from the server.\n\nAre you sure you want to continue?",
                "Confirm Data Refresh",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirm != DialogResult.Yes) return;

            // Cancel any ongoing operations
            CancelCurrentOperation();

            if (!BeginOperation()) return;

            try
            {
                await RunWithProgressBar(async () =>
                {
                    await RefreshDataFromServer(_currentOperationCts.Token);
                });
            }
            catch (OperationCanceledException)
            {
                AlertManager.ShowInfo("Data refresh was cancelled.");
            }
            catch (Exception ex)
            {
                AlertManager.ShowError($"Error refreshing data: {ex.Message}");
                await CreateLog($"Error refreshing data: {ex.Message}", "Error");
            }
            finally
            {
                EndOperation();
                btnLoad.InvokeIfRequired(() =>
                {
                    btnLoad.Text = "🔄Sync Products From Cloud";
                });
            }
        }

        private async Task RefreshDataFromServer(CancellationToken cancellationToken = default)
        {
            await CreateLog("Starting data refresh from API", "Info");

            // Step 1: Fetch from API
            var response = await _productCatalogueService.GetAllAsync(new ProductCatalogueQueryDto
            {
                numberOfRecords = 1000,
                pageNumber = 1
            });

            cancellationToken.ThrowIfCancellationRequested();

            var apiProducts = response?.Data?.ToList() ?? new List<ProductCatalogueDto>();

            if (!apiProducts.Any())
            {
                AlertManager.ShowWarning("No products found on the server.");
                return;
            }

            // Step 2: Clear local database
            var clearResult = await _productCatalogueService.DeleteProductCatalogue();
            cancellationToken.ThrowIfCancellationRequested();

            if (clearResult.StatusCode != ApiStatusCode.Success && clearResult.StatusCode != ApiStatusCode.NotFound)
            {
                throw new Exception($"Failed to clear local database: {clearResult.Message}");
            }

            // Step 3: Save to local database with progress
            int successCount = 0;
            var saveTasks = new List<Task>();

            foreach (var dto in apiProducts)
            {
                if (cancellationToken.IsCancellationRequested) break;

                var task = _productCatalogueService.PostProductCatalog(dto)
                    .ContinueWith(t =>
                    {
                        if (t.Result.StatusCode == ApiStatusCode.Success)
                        {
                            Interlocked.Increment(ref successCount);
                        }
                    }, cancellationToken);

                saveTasks.Add(task);

                // Batch processing to avoid overwhelming the system
                if (saveTasks.Count >= 10)
                {
                    await Task.WhenAll(saveTasks);
                    saveTasks.Clear();

                    // Update UI progress periodically
                    btnLoad.InvokeIfRequired(() =>
                    {
                        btnLoad.Text = $"Loading... ({successCount}/{apiProducts.Count})";
                    });
                }
            }

            // Wait for remaining tasks
            if (saveTasks.Any())
            {
                await Task.WhenAll(saveTasks);
            }

            cancellationToken.ThrowIfCancellationRequested();

            // Step 4: Reset and reload
            _currentPage = 1;
            SearchBox.InvokeIfRequired(() =>
            {
                SearchBox.Text = string.Empty;
            });
            await LoadFromLocalDB(cancellationToken);

            AlertManager.ShowSuccess($"Successfully loaded {successCount} products!");
            await CreateLog($"Refresh completed: {successCount} products", "Success");
        }

        private async Task LoadFromLocalDB(CancellationToken cancellationToken = default)
        {
            var response = await _productCatalogueService.GetProductCatalogue();
            cancellationToken.ThrowIfCancellationRequested();

            var allItems = response?.Data?.OrderBy(p => p.ItemSerialNumber).ToList()
                ?? new List<ProductCatalogueDto>();

            UpdateDataGridWithPagination(allItems, cancellationToken);
        }

        private async Task FilterProductsFromLocalDB(CancellationToken cancellationToken = default)
        {
            string searchText = SearchBox.Text?.Trim();
            _lastSearchTerm = searchText;

            if (string.IsNullOrWhiteSpace(searchText))
            {
                await LoadFromLocalDB(cancellationToken);
                return;
            }

            var response = await _productCatalogueService.GetProductCatalogue();
            cancellationToken.ThrowIfCancellationRequested();

            var allItems = response?.Data ?? Enumerable.Empty<ProductCatalogueDto>();

            var filtered = allItems.Where(p =>
                (p.ProductCode.HasValue && p.ProductCode.Value.ToString().Contains(searchText)) ||
                (p.ProductDescription?.Contains(searchText, StringComparison.OrdinalIgnoreCase) == true) ||
                (p.HSCode?.Contains(searchText, StringComparison.OrdinalIgnoreCase) == true)
            ).ToList();

            // If search term changed during operation, ignore results
            if (_lastSearchTerm != searchText) return;

            UpdateDataGridWithSearchResults(filtered, searchText);
        }
        #endregion

        #region UI Updates
        private void UpdateDataGridWithPagination(List<ProductCatalogueDto> allItems, CancellationToken cancellationToken = default)
        {
            if (IsOperationCanceled()) return;

            int totalRecords = allItems.Count;
            int totalPages = (int)Math.Ceiling((double)totalRecords / _pageSize);

            // Adjust current page if needed
            if (_currentPage > totalPages && totalPages > 0)
                _currentPage = totalPages;
            if (_currentPage < 1) _currentPage = 1;

            var pageData = allItems
                .Skip((_currentPage - 1) * _pageSize)
                .Take(_pageSize)
                .ToList();

            ProductCatalogueDataGridView.InvokeIfRequired(() =>
            {
                PopulateGrid(pageData);
                lblTotalRecords.Text = $"Total {totalRecords} Products";
                lblPageNumber.Text = $"Page {_currentPage} of {totalPages}";
                btnNext.Enabled = _currentPage < totalPages;
                btnPrev.Enabled = _currentPage > 1;
            });
        }

        private void UpdateDataGridWithSearchResults(List<ProductCatalogueDto> filtered, string searchText)
        {
            if (IsOperationCanceled()) return;

            ProductCatalogueDataGridView.InvokeIfRequired(() =>
            {
                PopulateGrid(filtered);
                lblTotalRecords.Text = $"Found {filtered.Count} products";
                lblPageNumber.Text = $"Search: '{searchText}'";
                btnNext.Enabled = false;
                btnPrev.Enabled = false;
            });
        }

        private void PopulateGrid(IEnumerable<ProductCatalogueDto> items)
        {
            ProductCatalogueDataGridView.SuspendLayout();
            ProductCatalogueDataGridView.Rows.Clear();

            int srNo = 1;
            foreach (var product in items)
            {
                int rowIndex = ProductCatalogueDataGridView.Rows.Add();
                var row = ProductCatalogueDataGridView.Rows[rowIndex];

                row.Cells["colSrNo"].Value = srNo++;
                SetCell(row, "colItemSrno", product.ItemSerialNumber);
                SetCell(row, "colProductCode", product.ProductCode);
                SetCell(row, "colProductDesc", product.ProductDescription);
                SetCell(row, "colHScode", product.HSCode);
                SetCell(row, "colSaleType", product.SaleType);
                SetCell(row, "colPosUOM", product.PosUnitOfMeasurement);
                SetCell(row, "colPrice", product.Price ?? 0);
                SetCell(row, "colTaxRate", product.TaxRate);
                SetCell(row, "colSROno", product.SroScheduleNumber);
            }

            ProductCatalogueDataGridView.ResumeLayout();
        }

        private void SetCell(DataGridViewRow row, string columnName, object value)
        {
            if (ProductCatalogueDataGridView.Columns.Contains(columnName))
                row.Cells[columnName].Value = value ?? "";
        }
        #endregion

        #region Event Handlers
        // Renamed to avoid duplicate method
        private void SearchBox_TextChanged_Handler(object sender, EventArgs e)
        {
            _searchDebounceTimer.Stop();
            _searchDebounceTimer.Start();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Clean up resources
            CancelCurrentOperation();
            _searchDebounceTimer?.Stop();
            _searchDebounceTimer?.Dispose();
            _currentOperationCts?.Dispose();
            base.OnFormClosing(e);
        }
        #endregion

        private async Task CreateLog(string message, string type)
        {
            try
            {
                // Replace 'Logs' with your actual log entity class name
                // If you don't have a Logs entity, use a simpler approach
                var log = new
                {
                    Message = message,
                    Type = type,
                    Timestamp = DateTime.Now
                };

                // If you have a proper log service, use it like this:
                // await _logService.CreateLogAsync(log);
                Console.WriteLine($"[{type}] {message}");
            }
            catch
            {
                // Suppress logging errors to avoid cascading failures
            }
        }

        #region Cleanup Methods
        private async Task PerformCleanup()
        {
            try
            {
                // Stop any ongoing operations
                _searchDebounceTimer?.Stop();

                // Reset state variables
                _currentPage = 1;
                _isLoading = false;

                // Clear search box safely
                SearchBox.InvokeIfRequired(() =>
                {
                    SearchBox.TextChanged -= SearchBox_TextChanged_Handler;
                    SearchBox.Clear();
                    SearchBox.TextChanged += SearchBox_TextChanged_Handler;
                });

                // Clear DataGridView
                ProductCatalogueDataGridView.InvokeIfRequired(() =>
                {
                    ProductCatalogueDataGridView.DataSource = null;
                    ProductCatalogueDataGridView.Rows.Clear();
                });

                // Reset pagination controls
                lblPageNumber.InvokeIfRequired(() => lblPageNumber.Text = "Page 1");
                btnNext.InvokeIfRequired(() => btnNext.Enabled = false);
                btnPrev.InvokeIfRequired(() => btnPrev.Enabled = false);

                // Small delay to ensure UI updates
                await Task.Delay(100);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during cleanup: {ex.Message}");
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
            ProductCatalogueDataGridView.Columns.Clear();
            StyleDataGridView(ProductCatalogueDataGridView);

            // Responsive settings
            ProductCatalogueDataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            ProductCatalogueDataGridView.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
            ProductCatalogueDataGridView.AllowUserToResizeColumns = true;
            ProductCatalogueDataGridView.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.False;

            // New "Sr. No." column (auto row numbers)
            var colSrNo = new DataGridViewTextBoxColumn
            {
                Name = "colSrNo",
                HeaderText = "Sr. No.",
                ReadOnly = true,
                FillWeight = 6
            };
            colSrNo.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colSrNo.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;

            // Product Code
            var colProductCode = new DataGridViewTextBoxColumn
            {
                Name = "colProductCode",
                HeaderText = "Product Code",
                ReadOnly = true,
                FillWeight = 10
            };
            colProductCode.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colProductCode.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;

            // Product Description
            var colProductDesc = new DataGridViewTextBoxColumn
            {
                Name = "colProductDesc",
                HeaderText = "Product Description",
                ReadOnly = true,
                FillWeight = 30
            };
            colProductDesc.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colProductDesc.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;

            // Item Serial Number (moved here)
            var colItemSrno = new DataGridViewTextBoxColumn
            {
                Name = "colItemSrno",
                HeaderText = "Item Sr. No.",
                ReadOnly = true,
                FillWeight = 8
            };
            colItemSrno.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colItemSrno.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;

            // HS Code
            var colHScode = new DataGridViewTextBoxColumn
            {
                Name = "colHScode",
                HeaderText = "   HS Code",
                ReadOnly = true,
                FillWeight = 8
            };
            colHScode.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            colHScode.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;

            // Sale Type
            var colSaleType = new DataGridViewTextBoxColumn
            {
                Name = "colSaleType",
                HeaderText = "Sale Type",
                ReadOnly = true,
                FillWeight = 12
            };
            colSaleType.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colSaleType.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;

            // POS UOM
            var colPosUOM = new DataGridViewTextBoxColumn
            {
                Name = "colPosUOM",
                HeaderText = "POS UOM",
                ReadOnly = true,
                FillWeight = 8
            };
            colPosUOM.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colPosUOM.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;

            // Price Column
            var colPrice = new DataGridViewTextBoxColumn
            {
                Name = "colPrice",
                HeaderText = "   Price",
                ReadOnly = true,
                FillWeight = 6
            };
            colPrice.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            colPrice.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
            colPrice.DefaultCellStyle.Format = "N2";

            // Tax Rate
            var colTaxRate = new DataGridViewTextBoxColumn
            {
                Name = "colTaxRate",
                HeaderText = "Tax Rate (%)",
                ReadOnly = true,
                FillWeight = 8
            };
            colTaxRate.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colTaxRate.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;

            // SRO Schedule
            var colSROno = new DataGridViewTextBoxColumn
            {
                Name = "colSROno",
                HeaderText = "SRO Schedule No.",
                ReadOnly = true,
                FillWeight = 10
            };
            colSROno.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colSROno.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;

            // columns in desired order
            ProductCatalogueDataGridView.Columns.AddRange(new DataGridViewColumn[]
            {
                colSrNo,
                colProductCode,
                colProductDesc,
                colItemSrno,
                colHScode,
                colSaleType,
                colPosUOM,
                colPrice,
                colTaxRate,
                colSROno
            });


            // Polish header appearance
            ProductCatalogueDataGridView.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            ProductCatalogueDataGridView.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(51, 51, 51);
            ProductCatalogueDataGridView.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            ProductCatalogueDataGridView.EnableHeadersVisualStyles = false;
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