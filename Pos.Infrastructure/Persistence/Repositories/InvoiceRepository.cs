using Microsoft.EntityFrameworkCore;
using Pos.Application.DTOs.InvoiceDtos;
using Pos.Domain.Entities;
using Pos.Domain.ValueObjects;
using Pos.Infrastructure.Persistence;

namespace Pos.Application.Interfaces.Repositories
{
    public class InvoiceRepository(DbContextFactory dbContextFactory) : IInvoiceRepository
    {
        private readonly DbContextFactory _dbContextFactory = dbContextFactory;

        private async Task<SqlServerDbContext> SetEnvironmentAsync(string env)
        {
            EnvironmentType evnironment = env == "Production" ? EnvironmentType.Production : EnvironmentType.Sandbox;
            var dbContext = await _dbContextFactory.CreateSqlServerDbContextAsync(evnironment);
            return dbContext!;
        }


        public async Task<IEnumerable<Invoice>> GetInvoicesAsync(InvoiceFilterDto dto, string env)
        {
            try
            {
                var context = await SetEnvironmentAsync(env);

                IQueryable<Invoice> query = context.Invoice.AsQueryable();

                // Only filter POS if given
                if (dto.PosId > 0 && dto.PosId is not null)
                    query = query.Where(i => i.POSID == dto.PosId);

                if (dto.FromDate.HasValue)
                    query = query.Where(i => i.EntryDate.Date >= dto.FromDate.Value.Date);

                if (dto.ToDate.HasValue)
                    query = query.Where(i => i.EntryDate.Date <= dto.ToDate.Value.Date);

                return await query.ToListAsync();
            }
            catch
            {
                throw;
            }
        }

        public async Task AddAsync(Invoice invoice, string env)
        {
            var context = await SetEnvironmentAsync(env);

            await context.Invoice.AddAsync(invoice);

            await context.SaveChangesAsync();

        }
    }
}
