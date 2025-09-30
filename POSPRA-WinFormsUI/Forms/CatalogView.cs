using POSPRA.Application.Services.FiscalService;
using POSPRA.Application.Services.ProductCatalogService;
using POSPRA.DTOs.ProductCatalogDtos;
using POSPRA_WinFormsUI.AlertClasses;

namespace POSPRA_WinFormsUI.Forms
{
    public partial class CatalogView : Form
    {
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

        public CatalogView(IFiscalService fiscalService, IProductCatalogueService productCatalogueService)
        {
            InitializeComponent();
            _fiscalService = fiscalService ?? throw new ArgumentNullException(nameof(fiscalService));
            _productCatalogueService = productCatalogueService ?? throw new ArgumentNullException(nameof(productCatalogueService));

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

        private void btnSave_Click(object sender, EventArgs e)
        {
            // your save logic here
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