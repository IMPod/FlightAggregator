namespace FlightAggregatorApi.BLL.Services;

public interface IFlightSourceService
{
    Task<string?> GetFlightsAsync(string sourceName);
}