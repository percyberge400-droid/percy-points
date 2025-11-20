using Microsoft.EntityFrameworkCore;
using Pos.Application.DTOs.ClientDtos;
using Pos.Application.Interfaces.Repositories;
using Pos.Domain.Entities;
using Pos.Domain.ValueObjects;

namespace Pos.Infrastructure.Persistence.Repositories
{
    public class PosClientRepository(DbContextFactory dbContextFactory) : IPosClientRepository
    {
        private readonly DbContextFactory _dbContextFactory = dbContextFactory;

        public async Task<PosClients> GetByMacAsync(ClientValidationDto dto)
        {
            EnvironmentType evnironment = dto.Environment == "Production" ? EnvironmentType.Production : EnvironmentType.Sandbox;

            await using var dbContext = await _dbContextFactory.CreateSqlServerDbContextAsync(evnironment);

            var entity = await dbContext!.PosClients.FirstOrDefaultAsync(x => x.POSRegistrationNumber == dto.PosId);
            return entity!;
        }

        public async Task<bool> UpdatePosCLientStatus(ClientValidationDto dto)
        {
            EnvironmentType evnironment = dto.Environment == "Production" ? EnvironmentType.Production : EnvironmentType.Sandbox;

            await using var dbContext = await _dbContextFactory.CreateSqlServerDbContextAsync(evnironment);

            var entity = await dbContext!.PosClients.FirstOrDefaultAsync(x => x.POSRegistrationNumber == dto.PosId);
            if(entity is null)
                return false;
            entity!.IsActive = true;
            await dbContext.SaveChangesAsync();

            return true;
        }
    }
}
