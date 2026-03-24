namespace Pos.Application.Services.ReferenceService.SyncOnButtonClickedService
{
    public interface ISyncOnButtonClickedService
    {
        Task SyncAllReferenceDataAsync(string environment, string password);
    }
}
