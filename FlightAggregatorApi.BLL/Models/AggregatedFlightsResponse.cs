namespace FlightAggregatorApi.BLL.Models;

public class AggregatedFlightsResponse
{
    public List<FlightDTO> Flights { get; set; }
    public List<string> Errors { get; set; } = [];
}