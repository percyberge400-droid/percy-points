namespace POSPRA.Application.Services.HttpClientService
{
    public interface IHttpService
    {
        Task<string> GetAsync(string url);
        Task<string> PostAsync(string url, object data);
    }
}