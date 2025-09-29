namespace POSPRA.API.Middlewares.Options
{
    public class ValidationMiddlewareOptions
    {
        public string[] BypassUrls { get; set; } = Array.Empty<string>();
    }
}
