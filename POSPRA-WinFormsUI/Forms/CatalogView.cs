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
        public CatalogView(IFiscalService fiscalService, IProductCatalogueService productCatalogueService)
        {
            InitializeComponent();
            _fiscalService = fiscalService;
            _productCatalogueService = productCatalogueService;
            btnSave.Click += btnSave_Click;

            this.Load += async (s, e) => await loadProductCatalogue();
        }


        private void btnSave_Click(object sender, EventArgs e)
        {
        }

        private async Task loadProductCatalogue()
        {
            try
            {
                var response = await _productCatalogueService.GetAllAsync(new ProductCatalogueQueryDto
                {
                    numberOfRecords = 1000,
                    pageNumber = 1
                });

                ProductCatalogueDataGridView.Rows.Clear();

                if (response?.Data == null || !response.Data.Any())
                {
                    WindowsLocalAppNotification.Show("Product Catalogue", "No Products in the Catalogue");
                    AlertManager.ShowWarning("No Products in the Catalogue");
                    return;
                }

                // loop and add rows
                foreach (var product in response.Data)
                {
                    ProductCatalogueDataGridView.Rows.Add(
                        product.ProductCode,
                        product.ProductDescription,
                        product.HSCode,
                        product.SaleType,
                        product.PosUnitOfMeasurement,
                        product.TaxRate,
                        product.SroScheduleNumber,
                        product.ItemSerialNumber
                    );
                    ProductCatalogueDataGridView.ResumeLayout(true);
                }
            }
            catch (Exception ex)
            {
                AlertManager.ShowError($"Error loading products: {ex.Message}");
            }
        }

    }
}
