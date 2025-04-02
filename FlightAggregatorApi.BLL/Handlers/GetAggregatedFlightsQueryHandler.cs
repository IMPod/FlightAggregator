using FlightAggregatorApi.BLL.Models;
using FlightAggregatorApi.BLL.Queries;
using FlightAggregatorApi.BLL.Services;
using MediatR;
using Newtonsoft.Json;

namespace FlightAggregatorApi.BLL.Handlers;

public class GetAggregatedFlightsQueryHandler(
    IFlightSourceService flightSourceService,
    IRedisCacheService redisCacheService)
    : IRequestHandler<GetAggregatedFlightsQuery, AggregatedFlightsResponse>
{
    private const int CancelledAfterSeconds = 15;

    public async Task<AggregatedFlightsResponse> Handle(GetAggregatedFlightsQuery request, CancellationToken cancellationToken)
    {
        var cacheKey =request.Filters.GenerateCacheKey();

        var cachedFlights = await redisCacheService.GetCachedFlightsAsync(cacheKey);
        if (cachedFlights != null)
        {
            return new AggregatedFlightsResponse { Flights = cachedFlights, Errors = [] };
        }

        var flightsATask = GetFlightsWithTimeoutAsync("FlightSource1", request.Filters, cancellationToken);
        var flightsBTask = GetFlightsWithTimeoutAsync("FlightSource2", request.Filters, cancellationToken);

        await Task.WhenAll(flightsATask, flightsBTask);

        var result = new AggregatedFlightsResponse
        {
            Flights = flightsATask.Result.Values.Concat(flightsBTask.Result.Values).ToList(),
            Errors = flightsATask.Result.Errors.Concat(flightsBTask.Result.Errors).ToList()
        };

        if (!result.Errors.Any())
        {
            await redisCacheService.SetCacheAsync(cacheKey, result.Flights);
        }

        result.Flights = request.SortBy.ToLower() switch
        {
            "price" => request.Descending ? result.Flights.OrderByDescending(f => f.Price).ToList() : result.Flights.OrderBy(f => f.Price).ToList(),
            "departure" => request.Descending ? result.Flights.OrderByDescending(f => f.DepartureDate).ToList() : result.Flights.OrderBy(f => f.DepartureDate).ToList(),
            "stops" => request.Descending ? result.Flights.OrderByDescending(f => f.Seats).ToList() : result.Flights.OrderBy(f => f.Seats).ToList(),
            _ => result.Flights
        };

        return result;
    }

    private async Task<(IEnumerable<FlightDTO> Values, IEnumerable<string> Errors)> GetFlightsWithTimeoutAsync(string source, FilterParams filters, CancellationToken cancellationToken)
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(CancelledAfterSeconds));
        var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, cancellationToken);

        try
        {
            var flightsJson = await flightSourceService.GetFlightsAsync(source, filters, linkedCts.Token);
            if (string.IsNullOrEmpty(flightsJson))
            {
                return ([], [$"{source} returned null data."]);
            }

            var flights = JsonConvert.DeserializeObject<List<FlightDTO>>(flightsJson) ?? [];

            return (flights, []);
        }
        catch (TaskCanceledException)
        {
            return ([], [$"{source} timed out."]);
        }
        catch (Exception ex)
        {
            return ([], [$"Error fetching from {source}: {ex.Message}"]);
        }
    }
}