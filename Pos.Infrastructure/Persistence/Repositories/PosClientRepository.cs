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


        private async Task<SqlServerDbContext> SetEnvironmentAsync(string env)
        {
            EnvironmentType evnironment = env == "Production" ? EnvironmentType.Production : EnvironmentType.Sandbox;
            var dbContext = await _dbContextFactory.CreateSqlServerDbContextAsync(evnironment);
            return dbContext!;
        }

        public async Task<PosClients> GetByMacAsync(ClientValidationDto dto)
        {
            var context = await SetEnvironmentAsync(dto.Environment);
            var entity = await context!.PosClients.FirstOrDefaultAsync(x => x.POSRegistrationNumber == dto.PosId);
            return entity!;
        }

        public async Task<PosClients> GetByPosId(long posId, string env)
        {
            var context = await SetEnvironmentAsync(env);
            var entity = await context!.PosClients.FirstOrDefaultAsync(x => x.POSRegistrationNumber == posId);
            return entity!;
        }

        public async Task<bool> UpdatePosCLientStatus(ClientValidationDto dto)
        {
            var context = await SetEnvironmentAsync(dto.Environment);

            var entity = await context!.PosClients.FirstOrDefaultAsync(x => x.POSRegistrationNumber == dto.PosId);
            if (entity is null)
                return false;
            entity!.IsActive = true;
            await context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> UpdatePosCLientStatus(long posId, bool isActive, string envrionment)
        {
            var context = await SetEnvironmentAsync(envrionment);

            var entity = await context!.PosClients.FirstOrDefaultAsync(x => x.POSRegistrationNumber == posId);
            if (entity is null)
                return false;
            entity!.IsActive = isActive;
            await context.SaveChangesAsync();

            return true;
        }

        public async Task<PosClients> UpdatePosClientHeartBeat(long posId, string env)
        {
            var context = await SetEnvironmentAsync(env);

            var entity = await context!.PosClients.FirstOrDefaultAsync(x => x.POSRegistrationNumber == posId);
            if (entity != null)
            {

                entity.IsConnected = true;
                entity.HeartbeatUpdatedOn = DateTime.Now;
                entity.StoreStatus = "Connected";

                await context.SaveChangesAsync();
            }

            return entity;
        }

        public async Task<PosClients?> GetByTokenAsync(string token, string env)
        {
            var context = await SetEnvironmentAsync(env);

            return await context.PosClients
                .FirstOrDefaultAsync(x => x.Token == token);
        }
    }
}
