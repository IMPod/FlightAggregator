using FlightAggregatorApi.BLL.Models;
using Microsoft.Extensions.Options;

namespace FlightAggregatorApi.BLL.Services;

public class FlightSourceService(HttpClient httpClient, IOptions<FlightSourceSettings> settings) : IFlightSourceService
{
    private readonly FlightSourceSettings _settings = settings.Value;

    public async Task<string?> GetFlightsAsync(string sourceName, FilterParams filterParams, CancellationToken cancellationToken)
    {
        if (!_settings.Sources.TryGetValue(sourceName, out var sourceConfig))
            return null;

        var url = BuildUrlWithFilters(sourceConfig.BaseUrl, filterParams);

        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("API-Key", sourceConfig.ApiKey);

        var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode) return null;

        return await response.Content.ReadAsStringAsync(cancellationToken);
    }

    private string BuildUrlWithFilters(string baseUrl, FilterParams filterParams)
    {
        var uriBuilder = new UriBuilder($"{baseUrl}/api/flights");

        var query = System.Web.HttpUtility.ParseQueryString(string.Empty);

        if (!string.IsNullOrEmpty(filterParams.Airline))
            query["airline"] = filterParams.Airline;

        if (filterParams.MinPrice.HasValue)
            query["minPrice"] = filterParams.MinPrice.Value.ToString();

        if (filterParams.MaxPrice.HasValue)
            query["maxPrice"] = filterParams.MaxPrice.Value.ToString();

        if (filterParams.MinDepartureDate.HasValue)
            query["minDepartureDate"] = filterParams.MinDepartureDate.Value.ToString("o"); 

        if (filterParams.MaxDepartureDate.HasValue)
            query["maxDepartureDate"] = filterParams.MaxDepartureDate.Value.ToString("o");

        uriBuilder.Query = query.ToString(); 

        return uriBuilder.ToString();
    }
}