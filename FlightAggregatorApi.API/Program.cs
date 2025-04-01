using FlightAggregatorApi.BLL;
using FlightAggregatorApi.BLL.Services;
using Microsoft.OpenApi.Models;
using StackExchange.Redis;

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

var redis = ConnectionMultiplexer.Connect("localhost");
builder.Services.AddSingleton<IConnectionMultiplexer>(redis);
builder.Services.AddSingleton<IRedisCacheService, RedisCacheService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "FlightAggregatorApi", Version = "v1" });
});

var app = builder.Build();

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