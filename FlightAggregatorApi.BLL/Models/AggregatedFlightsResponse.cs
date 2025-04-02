namespace FlightAggregatorApi.BLL.Models;

public class AggregatedFlightsResponse
{
    public IEnumerable<FlightDTO> Flights { get; set; }
    public IEnumerable<string> Errors { get; set; } = [];
}