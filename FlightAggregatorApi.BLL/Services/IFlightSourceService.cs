using FlightAggregatorApi.BLL.Models;

namespace FlightAggregatorApi.BLL.Services;

public interface IFlightSourceService
{
    Task<string?> GetFlightsAsync(string sourceNamee, FilterParams filterParams, CancellationToken cancellationToken);
}