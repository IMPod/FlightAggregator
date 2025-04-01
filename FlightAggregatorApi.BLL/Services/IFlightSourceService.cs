namespace FlightAggregatorApi.BLL.Services;

public interface IFlightSourceService
{
    Task<string?> GetFlightsAsync(string sourceNamee, CancellationToken cancellationToken);
}