using Pos.Application.DTOs;
using Pos.Application.DTOs.PageResponseDTOs;
using Pos.Application.DTOs.ProductCatalogDtos;

namespace Pos.Application.Services.ProductCatalogService
{
    public interface IProductCatalogueService
    {
        Task<ApiResponse<List<ProductCatalogueDto>>> GetAllAsync();

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
        /// This API is used to get product catalog from local db with pagination implemented.
        /// </summary>
        /// <returns></returns>
        Task<ApiResponse<PageResponseDto<ProductCatalogueDto>>> GetProductCatalogueWithPagination(ProductCatalogueQueryDto query);
        /// <summary>
        /// This API is used to delete product catalog.
        /// </summary>
        /// <returns></returns>
        Task<ApiResponse<object>> DeleteProductCatalogue();
    }
}