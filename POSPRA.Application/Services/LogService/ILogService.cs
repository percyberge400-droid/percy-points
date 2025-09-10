using POSPRA.Domain.Entities;

namespace POSPRA.Application.Services.LogService
{
    public interface ILogService
    {
        Task LogAsync(Logs model, int retry = 0);
    }
}
