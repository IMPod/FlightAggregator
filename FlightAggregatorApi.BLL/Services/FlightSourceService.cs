using Microsoft.Extensions.Options;

namespace FlightAggregatorApi.BLL.Services;

public class FlightSourceService(HttpClient httpClient, IOptions<FlightSourceSettings> settings) : IFlightSourceService
{
    private readonly FlightSourceSettings _settings = settings.Value;

    public async Task<string?> GetFlightsAsync(string sourceName, CancellationToken cancellationToken)
    {
        if (!_settings.Sources.TryGetValue(sourceName, out var sourceConfig))
            return null;

        var request = new HttpRequestMessage(HttpMethod.Get, $"{sourceConfig.BaseUrl}/api/flights");
        request.Headers.Add("API-Key", sourceConfig.ApiKey);

        var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode) return null;

        return await response.Content.ReadAsStringAsync(cancellationToken);
    }
}