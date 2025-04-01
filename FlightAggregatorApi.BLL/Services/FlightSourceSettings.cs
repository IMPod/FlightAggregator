namespace FlightAggregatorApi.BLL.Services;

public class FlightSourceSettings
{
    public Dictionary<string, FlightSourceConfig> Sources { get; set; } = new();
}

public class FlightSourceConfig
{
    public string BaseUrl { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
}