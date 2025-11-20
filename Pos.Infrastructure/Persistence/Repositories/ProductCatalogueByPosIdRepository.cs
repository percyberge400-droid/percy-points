using Microsoft.Extensions.Configuration;
using Pos.Application.DTOs.ProductCatalogDtos;
using Pos.Application.Interfaces;
using Pos.Application.Interfaces.Repositories;

namespace Pos.Infrastructure.Persistence.Repositories.ProductCatalogue
{
    public class ProductCatalogueByPosIdRepository : IProductCatalogueByPosIdRepository
    {
        private readonly DbContextFactory _dbContextFactory;
        public ProductCatalogueByPosIdRepository(
            ISqlServerRepositoryFactory sqlRepositoryFactory, IConfiguration configuration, DbContextFactory dbContextFactory)
        {
            // Get the DbContext from the factory or inject it directly
            _dbContextFactory = dbContextFactory;
        }

        public async Task<IEnumerable<ProductCatalogueDto>> GetProductCatalogueByPosIdAsync(int posId)
        {
            // Use the DbContextFactory to always get Production DB instance
            await using var dbContext = await _dbContextFactory.CreateSqlServerDbContextAsync(forceProduction: true);

            // Query the data
            var output =
                from client in dbContext.PosClients
                join branch in dbContext.POSBranches
                    on client.POSBranchID equals branch.POSBranchID
                join pc in dbContext.ProductCatalogue
                    on branch.POSMASTERID equals pc.POSMasterId
                where client.POSRegistrationNumber == posId
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

            return output.ToList();
        }
    }
}