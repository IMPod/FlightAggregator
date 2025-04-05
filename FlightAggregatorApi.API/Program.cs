using FlightAggregatorApi.BLL;
using FlightAggregatorApi.BLL.Services;
using FlightAggregatorApi.BLL.Middlewares;
using Microsoft.OpenApi.Models;
using Serilog;
using StackExchange.Redis;
using System.Reflection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = true;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(secretKey),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwtSettings["Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(Convert.ToDouble(jwtSettings["TokenLifetimeMinutes"]))
        };
#if Debug
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine("Token invalid: " + context.Exception.Message);
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Console.WriteLine("Token valid");
                return Task.CompletedTask;
            }
        };
#endif
    });

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});
builder.Services.AddAuthorization();
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

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null; 
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "FlightAggregatorApi", Version = "v1" });
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
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

app.UseHttpsRedirection();

app.UseAuthentication();  
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }