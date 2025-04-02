namespace FlightAggregatorApi.BLL.Models;

public class FlightBookingRequest
{
    public int FlightId { get; set; }
    public int Seats { get; set; }
    public string Source { get; set; } 
}