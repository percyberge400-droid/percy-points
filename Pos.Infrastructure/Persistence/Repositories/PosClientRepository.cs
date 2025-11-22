using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
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
            if (entity is not null)
            {
                // Activate the POS Client
                entity.IsActive = true;
              
                // Added default POS configuration 
                var defaultConfiguration = await context.POSConfigurations.FirstOrDefaultAsync(x => x.IsActive == true);

                POSConfigurations pOSConfiguration = new()
                {
                    POSID = dto.PosId,
                    DateCreated = DateTime.Now,
                    FilePath = defaultConfiguration is not null ? defaultConfiguration.FilePath : "",
                    FileSize = defaultConfiguration is not null ? defaultConfiguration.FileSize : 0,
                    GatewayURL = defaultConfiguration is not null ? defaultConfiguration.GatewayURL : "",
                    HeartbeatInterval = defaultConfiguration is not null ? defaultConfiguration.HeartbeatInterval : 0,
                    IMSUpdateInterval = defaultConfiguration is not null ? defaultConfiguration.IMSUpdateInterval : 0,
                    IsActive = true,
                    IsCloudSyncEnabled = true,
                    LogInterval = defaultConfiguration is not null ? defaultConfiguration.LogInterval : 0,
                    LogSyncLimit = defaultConfiguration is not null ? defaultConfiguration.LogSyncLimit : 0,
                    RecordInterval = defaultConfiguration is not null ? defaultConfiguration.RecordInterval : 0,
                    RecordSyncLimit = defaultConfiguration is not null ? defaultConfiguration.RecordSyncLimit : 0,
                    Token = defaultConfiguration is not null ? defaultConfiguration.Token : "",
                    Version = defaultConfiguration is not null ? defaultConfiguration.Version : ""
                };

                await context.POSConfigurations.AddAsync(pOSConfiguration);
                await context.SaveChangesAsync();


                return true;
            }
            return false;
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
