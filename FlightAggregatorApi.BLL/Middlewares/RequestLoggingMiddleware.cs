using Microsoft.AspNetCore.Http;
using System.Diagnostics;
using System.Text;
using Serilog;

namespace FlightAggregatorApi.BLL.Middlewares
{
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

            context.Request.EnableBuffering();

            var originalBodyStream = context.Response.Body;

            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            var requestBody = string.Empty;
            using (var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true)) 
            {
                requestBody = await reader.ReadToEndAsync();
                context.Request.Body.Position = 0; 
            }

            Log.Information("Incoming Request: {Method} {Path} from {IP} with body: {Body}",
                context.Request.Method, context.Request.Path, context.Connection.RemoteIpAddress, requestBody);

            await _next(context);

            context.Response.Body.Seek(0, SeekOrigin.Begin);
            context.Response.Body.Seek(0, SeekOrigin.Begin);

            stopwatch.Stop();
            Log.Information("Outgoing Response: {StatusCode} for {Method} {Path} - {Elapsed} ms",
                context.Response.StatusCode, context.Request.Method, context.Request.Path, stopwatch.ElapsedMilliseconds);
            await responseBody.CopyToAsync(originalBodyStream);
        }
    }
}