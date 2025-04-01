using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection("ApiSettings"));
builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<ApiSettings>>().Value);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("ApiKey", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "API Key required",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Name = "API-Key",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Scheme = "ApiKeyScheme"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "ApiKey"
                }
            },
            new string[] { }
        }
    });
});

var app = builder.Build();

app.UseExceptionHandler(exceptionHandlerApp =>
{
    exceptionHandlerApp.Run(async context =>
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";

        var exceptionHandlerFeature = context.Features.Get<IExceptionHandlerFeature>();
        if (exceptionHandlerFeature != null)
        {
            var exception = exceptionHandlerFeature.Error;
            await context.Response.WriteAsJsonAsync(new
            {
                error = exception.Message,
                stackTrace = exception.StackTrace
            });
        }
    });
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Use(async (context, next) =>
{
    var random = new Random();
    var delaySeconds = random.Next(1, 11);

    Console.WriteLine($"Adding random delay of {delaySeconds} seconds for request to {context.Request.Path}");

    await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
    await next();
});

app.Use(async (context, next) =>
{
    var apiSettings = context.RequestServices.GetRequiredService<ApiSettings>();

    if (!context.Request.Headers.TryGetValue("API-Key", out var apiKey) || apiKey != apiSettings.ApiKey)
    {
        context.Response.StatusCode = 401; // Unauthorized
        await context.Response.WriteAsync("Unauthorized: Invalid API Key");
        return;
    }
    await next();
});

app.UseHttpsRedirection();

app.MapGet("/api/flights", async () =>
{
    var flights = new List<object>
    {
        new { Id = 1, Airline = "Airline 1", Price = 100, Stops = 0, DepartureDate = "2025-04-10", AvailableSeats = 50 },
        new { Id = 2, Airline = "Airline 2", Price = 150, Stops = 1, DepartureDate = "2025-04-12", AvailableSeats = 30 }
    };
    return Results.Ok(flights);
})
.WithName("GetFlights");

app.MapPost("/api/flights/book", async (HttpContext context) =>
{
    var bookingRequest = await context.Request.ReadFromJsonAsync<BookingRequest>();

    if (bookingRequest == null || bookingRequest.FlightId <= 0)
    {
        return Results.BadRequest("Invalid booking request.");
    }

    return Results.Ok(new { Message = $"Booking confirmed for flight {bookingRequest.FlightId}." });
})
.WithName("BookFlight");

app.Run();

public class BookingRequest
{
    public int FlightId { get; set; }
    public int Seats { get; set; }
}

public class ApiSettings
{
    public string ApiKey { get; set; }
}
