namespace POSPRA.Application.Services.HttpClientService
{
    public interface IHttpService
    {
        Task<string> GetAsync(string url, Dictionary<string, string>? headers = null);
        Task<string> PostAsync(string url, object data, Dictionary<string, string>? headers = null);
    }
}