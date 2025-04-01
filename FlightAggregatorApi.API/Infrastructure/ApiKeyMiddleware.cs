namespace FlightAggregatorApi.API.Infrastructure;

public class ApiKeyMiddleware(RequestDelegate next)
{
    private readonly IDictionary<string, string> _validApiKeys = new Dictionary<string, string>
    {
        { "FlightSource1", "FlightSource1ApiKey" },
        { "FlightSource2", "FlightSource2ApiKey" }
    };

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue("API-Key", out var apiKey) ||
            !_validApiKeys.Values.Contains(apiKey))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Unauthorized: Invalid API Key");
            return;
        }

        await next(context);
    }
}