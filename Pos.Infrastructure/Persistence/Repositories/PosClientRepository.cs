using Microsoft.EntityFrameworkCore;
using Pos.Application.DTOs.ClientDtos;
using Pos.Application.Interfaces.Repositories;
using Pos.Domain.Entities;

namespace Pos.Infrastructure.Persistence.Repositories
{
    public class PosClientRepository(DbContextFactory dbContextFactory) : IPosClientRepository
    {
        private readonly DbContextFactory _dbContextFactory = dbContextFactory;

        public async Task<PosClients> GetByMacAsync(ClientValidationDto dto)
        {
            bool evnironment = dto.Environment == "Production" ? true : false;

            await using var dbContext = await _dbContextFactory.CreateSqlServerDbContextAsync(forceProduction: evnironment);

            var entity = await dbContext!.PosClients.FirstOrDefaultAsync(x => x.POSRegistrationNumber == dto.PosId);
            return entity!;
        }
    }
}
