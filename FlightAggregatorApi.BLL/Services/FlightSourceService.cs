using FlightAggregatorApi.BLL.Models;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;

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

    public async Task<BookingResponse> BookFlightAsync(string sourceName, FlightBookingRequest bookingRequest, CancellationToken cancellationToken)
    {
        if (!_settings.Sources.TryGetValue(sourceName, out var sourceConfig))
        {
            return new BookingResponse { Success = false, Message = "Unknown flight source" };
        }

        var bookingUrl = $"{sourceConfig.BaseUrl}/api/flights/book";

        var requestMessage = new HttpRequestMessage(HttpMethod.Post, bookingUrl)
        {
            Content = JsonContent.Create(bookingRequest)
        };
        requestMessage.Headers.Add("API-Key", sourceConfig.ApiKey);

        var response = await httpClient.SendAsync(requestMessage, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return new BookingResponse
            {
                Success = false,
                Message = $"Failed to book flight: {response.StatusCode}"
            };
        }

        var result = await response.Content.ReadFromJsonAsync<BookingResponse>(cancellationToken: cancellationToken);

        return result ?? new BookingResponse { Success = false, Message = "Unknown error" };
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