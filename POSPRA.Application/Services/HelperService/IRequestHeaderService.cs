namespace POSPRA.Application.Services.HelperService
{
    public interface IRequestHeaderService
    {
        long GetPosId();
        string? GetMacAddress();
    }
}
