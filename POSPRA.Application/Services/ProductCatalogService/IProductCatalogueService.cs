using POSPRA.DTOs;
using POSPRA.DTOs.ProductCatalogDtos;

namespace POSPRA.Application.Services.ProductCatalogService
{
    public interface IProductCatalogueService
    {
        Task<ApiResponse<List<ProductCatalogueDto>>> GetAllAsync();
    }
}