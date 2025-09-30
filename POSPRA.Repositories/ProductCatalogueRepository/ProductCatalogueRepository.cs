using POSPRA.Domain.Entities;
using POSPRA.Infrastructure.Context;
using POSPRA.Repositories.BaseRepository;

namespace POSPRA.Repositories.ProductCatalogueRepository
{
    public class ProductCatalogueSQLServerRepository : SqlServerRepository<ProductCatalogue>, IProductCatalogueSQLServerRepository
    {
        public ProductCatalogueSQLServerRepository(SqlServerDbContext context) : base(context)
        {
        }
    }

    public class ProductCatalogueSQLiteRepository : SqliteRepository<ProductCatalogue>, IProductCatalogueSQLiteRepository
    {
        public ProductCatalogueSQLiteRepository(SqliteDbContext context) : base(context)
        {
        }
    }
}
