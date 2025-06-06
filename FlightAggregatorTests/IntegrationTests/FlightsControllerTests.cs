﻿using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using FlightAggregatorApi.API.Controllers;
using FlightAggregatorApi.BLL.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json.Linq;
using Xunit;

namespace FlightAggregatorTests.IntegrationTests;

public class FlightsControllerTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();

    private async Task<string?> GetJwtToken()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Username = "admin",
            Password = "password" 
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        response.EnsureSuccessStatusCode();

        var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>();
        return tokenResponse?.Token;
    }

    [Fact]
    public async Task GetAggregatedFlights_Returns_Ok_With_Flights()
    {
        // Arrange
        var token = await GetJwtToken();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var query = new
        {
            airline = "Airline 1",
            minPrice = 100.0,
            maxPrice = 200.0,
            minDepartureDate = "2025-01-01",
            maxDepartureDate = "2025-12-31",
            sortBy = "price",
            descending = false
        };

        // Act
        var response = await _client.GetAsync($"/api/aggregator/flights?airline={query.airline}&minPrice={query.minPrice}&maxPrice={query.maxPrice}&minDepartureDate={query.minDepartureDate}&maxDepartureDate={query.maxDepartureDate}&sortBy={query.sortBy}&descending={query.descending}");

        // Assert
        response.EnsureSuccessStatusCode();

        var flights = await response.Content.ReadFromJsonAsync<AggregatedFlightsResponse>();
        Assert.NotNull(flights);
        Assert.NotEmpty(flights.Flights);
    }

    [Fact]
    public async Task BookFlight_Returns_Ok_With_Confirmation()
    {
        // Arrange
        var token = await GetJwtToken();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var bookingRequest = new FlightBookingRequest
        {
            FlightId = 11,
            Source = "FlightSource1",
            Seats = 1
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/aggregator/book-flight", bookingRequest);

        // Assert
        response.EnsureSuccessStatusCode();
        var confirmation = await response.Content.ReadFromJsonAsync<BookingResponse>();
        Assert.NotNull(confirmation);
        Assert.True(confirmation.Success, "Booking should be successful");
    }
}

public class TokenResponse
{
    public string Token { get; set; }
}