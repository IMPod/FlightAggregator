using Microsoft.AspNetCore.Http;
using System.Diagnostics;
using Serilog;

namespace FlightAggregatorApi.BLL.Middlewares;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;

    public RequestLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            Log.Information("Incoming Request: {Method} {Path} from {IP}",
                context.Request.Method, context.Request.Path, context.Connection.RemoteIpAddress);

            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            Log.Information("Outgoing Response: {StatusCode} for {Method} {Path} - {Elapsed} ms",
                context.Response.StatusCode, context.Request.Method, context.Request.Path, stopwatch.ElapsedMilliseconds);
        }
    }
}