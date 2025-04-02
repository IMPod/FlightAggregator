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

var flights = new List<FlightDTO>
{
    new() { Id = 1, Airline = "Airline 3", Price = 100, Stops = 0, DepartureDate = new DateTime(2025, 4, 10), AvailableSeats = 50 },
    new() { Id = 2, Airline = "Airline 4", Price = 150, Stops = 1, DepartureDate = new DateTime(2025, 4, 14), AvailableSeats = 30 },
    new() { Id = 3, Airline = "Airline 3", Price = 200, Stops = 2, DepartureDate = new DateTime(2025, 4, 16), AvailableSeats = 20 },
};

app.MapGet("/api/flights", async (HttpContext httpContext) =>
{
    var queryParams = httpContext.Request.Query;

    var filteredFlights = flights.Where(flight =>
        (string.IsNullOrEmpty(queryParams["airline"]) || flight.Airline.Equals(queryParams["airline"], StringComparison.OrdinalIgnoreCase)) &&
        (!double.TryParse(queryParams["minPrice"], out var minPrice) || flight.Price >= minPrice) &&
        (!double.TryParse(queryParams["maxPrice"], out var maxPrice) || flight.Price <= maxPrice) &&
        (!DateTime.TryParse(queryParams["minDepartureDate"], out var minDepartureDate) || flight.DepartureDate >= minDepartureDate) &&
        (!DateTime.TryParse(queryParams["maxDepartureDate"], out var maxDepartureDate) || flight.DepartureDate <= maxDepartureDate)
    ).ToList();

    return Results.Ok(filteredFlights);
})
.WithName("GetFlights");

app.MapPost("/api/flights/book", async (HttpContext context) =>
    {
        var bookingRequest = await context.Request.ReadFromJsonAsync<BookingRequest>();
        if (bookingRequest is not { FlightId: > 0, Seats: > 0 })
        {
            return Results.BadRequest("Invalid booking request.");
        }

        var flight = flights.FirstOrDefault(f => f.Id == bookingRequest.FlightId);
        if (flight == null)
        {
            return Results.NotFound("Flight not found.");
        }

        if (flight.AvailableSeats < bookingRequest.Seats)
        {
            return Results.BadRequest("Not enough available seats.");
        }

        flight.AvailableSeats -= bookingRequest.Seats;
        return Results.Ok(new { Message = $"Booking confirmed for flight {bookingRequest.FlightId}.", RemainingSeats = flight.AvailableSeats });
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

public class FlightDTO
{
    public int Id { get; set; }
    public string Airline { get; set; }
    public double Price { get; set; }
    public int Stops { get; set; }
    public DateTime DepartureDate { get; set; }
    public int AvailableSeats { get; set; }
    public string Source { get; } = "FlightSource2";
}
