using FlightAggregatorApi.BLL;
using FlightAggregatorApi.BLL.Services;
using FlightAggregatorApi.BLL.Middlewares;
using Microsoft.OpenApi.Models;
using Serilog;
using StackExchange.Redis;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(typeof(MediatRCommandAssemblyMarker).Assembly));

builder.Services.Configure<FlightSourceSettings>(builder.Configuration.GetSection("FlightSources"));
builder.Services.Configure<FlightSourceSettings>(options =>
{
    var config = builder.Configuration.GetSection("FlightSources");
    foreach (var source in config.GetChildren())
    {
        options.Sources[source.Key] = new FlightSourceConfig
        {
            BaseUrl = source.GetValue<string>("BaseUrl") ?? string.Empty,
            ApiKey = source.GetValue<string>("ApiKey") ?? string.Empty
        };
    }
});

builder.Services.AddHttpClient<IFlightSourceService, FlightSourceService>();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration) 
    .Enrich.FromLogContext()
    .Enrich.WithThreadId()
    .Enrich.WithEnvironmentName()
    .WriteTo.Console()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

var redis = ConnectionMultiplexer.Connect("localhost");
builder.Services.AddSingleton<IConnectionMultiplexer>(redis);
builder.Services.AddSingleton<IRedisCacheService, RedisCacheService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "FlightAggregatorApi", Version = "v1" });
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

var app = builder.Build();

app.UseMiddleware<RequestLoggingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "FlightAggregatorApi v1");
    });
}

//app.UseMiddleware<ApiKeyMiddleware>();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();