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
    public async Task<AggregatedFlightsResponse> Handle(GetAggregatedFlightsQuery request, CancellationToken cancellationToken)
    {
        var cacheKey =request.Filters.GenerateCacheKey();

        var cachedFlights = await redisCacheService.GetCachedFlightsAsync(cacheKey);
        if (cachedFlights != null)
        {
            return new AggregatedFlightsResponse { Flights = cachedFlights, Errors = new List<string>() };
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

        return result;
    }

    private async Task<(List<FlightDTO> Values, List<string> Errors)> GetFlightsWithTimeoutAsync(string source, FilterParams filters, CancellationToken cancellationToken)
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, cancellationToken);

        try
        {
            var flightsJson = await flightSourceService.GetFlightsAsync(source, filters, linkedCts.Token);
            if (string.IsNullOrEmpty(flightsJson))
            {
                return (new List<FlightDTO>(), new List<string> { $"{source} returned null data." });
            }

            var flights = JsonConvert.DeserializeObject<List<FlightDTO>>(flightsJson) ?? new List<FlightDTO>();
            return (flights, new List<string>());
        }
        catch (TaskCanceledException)
        {
            return (new List<FlightDTO>(), new List<string> { $"{source} timed out." });
        }
        catch (Exception ex)
        {
            return (new List<FlightDTO>(), new List<string> { $"Error fetching from {source}: {ex.Message}" });
        }
    }
}