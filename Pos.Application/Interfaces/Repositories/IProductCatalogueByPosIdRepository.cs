using Pos.Application.DTOs.ProductCatalogDtos;

namespace Pos.Application.Interfaces.Repositories
{
    public interface IProductCatalogueByPosIdRepository
    {
        /// <summary>
        /// Retrieves product catalogue with joined data by POS ID.
        /// </summary>
        Task<IEnumerable<ProductCatalogueDto>> GetProductCatalogueByPosIdAsync(int posId);
    }
}