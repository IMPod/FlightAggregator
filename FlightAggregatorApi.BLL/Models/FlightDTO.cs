namespace FlightAggregatorApi.BLL.Models;

public class FlightDTO
{
    public int Id { get; set; }
    public string Airline { get; set; }
    public double Price { get; set; }
    public int Stops { get; set; }
    public DateTime DepartureDate { get; set; }
    public int AvailableSeats { get; set; }
    public string Source { get; set; }
}