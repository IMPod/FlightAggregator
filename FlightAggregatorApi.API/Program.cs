using FlightAggregatorApi.API.Infrastructure;
using FlightAggregatorApi.BLL.Services;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

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

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseMiddleware<ApiKeyMiddleware>();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();