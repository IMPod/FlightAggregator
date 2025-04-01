namespace FlightAggregatorApi.BLL.Models;

public class FilterParams
{
    public string Airline { get; set; }
    public double? MinPrice { get; set; }
    public double? MaxPrice { get; set; }
    public DateTime? MinDepartureDate { get; set; }
    public DateTime? MaxDepartureDate { get; set; }
}

