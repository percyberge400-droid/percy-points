using POSPRA.Domain.Entities;
using POSPRA.Repositories.BaseRepository.Repository;

namespace POSPRA.Repositories.ProductCatalogueRepository
{
    public interface IProductCatalogueSQLServerRepository : IRepository<ProductCatalogue>
    {
    }

    public interface IProductCatalogueSQLiteRepository : IRepository<ProductCatalogue>
    {
    }
}
