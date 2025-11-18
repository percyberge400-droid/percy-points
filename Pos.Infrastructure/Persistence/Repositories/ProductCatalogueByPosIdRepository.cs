using Microsoft.EntityFrameworkCore;
using Pos.Application.DTOs.ProductCatalogDtos;
using Pos.Application.Interfaces;
using Pos.Application.Interfaces.ProductCatalogue;

namespace Pos.Infrastructure.Persistence.Repositories.ProductCatalogue
{
    public class ProductCatalogueByPosIdRepository : IProductCatalogueByPosIdRepository
    {
        private readonly IRepository<Pos.Domain.Entities.ProductCatalogue> _sqlProductCatalogueRepository;
        private readonly IRepository<Pos.Domain.Entities.PosClients> _sqlPosClientRepository;

        public ProductCatalogueByPosIdRepository(
            ISqlServerRepositoryFactory sqlRepositoryFactory)
        {
            // Get the DbContext from the factory or inject it directly
            _sqlProductCatalogueRepository = sqlRepositoryFactory.CreateRepository<Pos.Domain.Entities.ProductCatalogue>();
            _sqlPosClientRepository = sqlRepositoryFactory.CreateRepository<Pos.Domain.Entities.PosClients>(); ;
        }

        public async Task<IEnumerable<ProductCatalogueDto>> GetProductCatalogueByPosIdAsync(int posId)
        {
            var output = from pc in _sqlProductCatalogueRepository.Query()
                         join pos in _sqlPosClientRepository.Query()
                         on pc.POSMasterId equals pos.POSBranchID
                         where pos.POSRegistrationNumber == posId
                         select new ProductCatalogueDto
                         {
                             ItemSerialNumber = pc.ItemSerialNumber,
                             ProductCode = pc.ProductCode,
                             ProductDescription = pc.ProductDescription,
                             HSCode = pc.HSCode,
                             SaleType = pc.SaleType,
                             PosUnitOfMeasurement = pc.PosUnitOfMeasurement,
                             Price = pc.Price,
                             TaxRate = pc.TaxRate,
                             SroScheduleNumber = pc.SroScheduleNumber
                         };

            return await output.ToListAsync();

        }
    }
}