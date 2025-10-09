using POSPRA.DTOs;
using POSPRA.DTOs.ProductCatalogDtos;

namespace POSPRA.Application.Services.ProductCatalogService
{
    public interface IProductCatalogueService
    {
        Task<ApiResponse<List<ProductCatalogueDto>>> GetAllAsync(ProductCatalogueQueryDto dto);

        /// <summary>
        /// This method is used to create product catalogue in SQLite.
        /// </summary>
        /// <param name="productCatalogueDto"></param>
        /// <returns></returns>
        Task<ApiResponse<ProductCatalogueDto>> PostProductCatalog(ProductCatalogueDto productCatalogueDto);

        /// <summary>
        /// This API is used to get product catalog.
        /// </summary>
        /// <returns></returns>
        Task<ApiResponse<List<ProductCatalogueDto>>> GetProductCatalogue();

        /// <summary>
        /// This API is used to delete product catalog.
        /// </summary>
        /// <returns></returns>
        Task<ApiResponse<object>> DeleteProductCatalogue();
    }
}