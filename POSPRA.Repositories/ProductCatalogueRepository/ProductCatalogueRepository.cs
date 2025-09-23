using POSPRA.Domain.Entities;
using POSPRA.Infrastructure.Context;
using POSPRA.Repositories.BaseRepository;

namespace POSPRA.Repositories.ProductCatalogueRepository
{
    public class ProductCatalogueRepository : SqlServerRepository<ProductCatalogue>, IProductCatalogueRepository
    {
        public ProductCatalogueRepository(SqlServerDbContext context) : base(context)
        {
        }
    }
}
