using FlightAggregatorApi.API.Models;
using FlightAggregatorApi.BLL.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace FlightAggregatorApi.API.Controllers;

[Route("api/aggregator")]
[ApiController]
public class FlightsController(IFlightSourceService flightSourceService) : ControllerBase
{
    private readonly TimeSpan _timeout = TimeSpan.FromSeconds(10);

    [HttpGet("flights")]
    public async Task<IActionResult> GetAggregatedFlights()
    {
        var flightsATask = GetFlightsWithTimeoutAsync("FlightSource1");
        var flightsBTask = GetFlightsWithTimeoutAsync("FlightSource2");

        await Task.WhenAll(flightsATask, flightsBTask);

        var results = new
        {
            FlightsFromA = flightsATask.Result.Values,
            FlightsFromB = flightsBTask.Result.Values,
            Errors = flightsATask.Result.Errors.Concat(flightsBTask.Result.Errors).ToList()
        };

        return Ok(results);
    }

    private async Task<(List<FlightDTO> Values, List<string> Errors)> GetFlightsWithTimeoutAsync(string source)
    {
        using var cts = new CancellationTokenSource(_timeout);
        try
        {
            var flightsJson = await flightSourceService.GetFlightsAsync(source, cts.Token);

            if (flightsJson == null) 
                return ([], [$"{source} returned null data."]);
            var flights = JsonConvert.DeserializeObject<List<FlightDTO>>(flightsJson) ?? [];
            return (flights, []);
        }
        catch (TaskCanceledException)
        {
            return ([], [$"{source} timed out."]);
        }
        catch (Exception ex)
        {
            return ([], [$"Error fetching from {source}: {ex.Message}"]);
        }
    }
}
