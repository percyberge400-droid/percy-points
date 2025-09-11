namespace POSPRA.Application.Services.HelperService
{
    public interface IRequestHeaderService
    {
        int? GetPosId();
        string? GetMacAddress();
    }
}
