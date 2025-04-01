using FlightAggregatorApi.BLL.Models;
using FlightAggregatorApi.BLL.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace FlightAggregatorApi.API.Controllers;

[Route("api/aggregator")]
[ApiController]
public class FlightsController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Get aggregated flights with optional filters.
    /// </summary>
    /// <param name="airline">The airline name to filter the flights.</param>
    /// <param name="minPrice">The minimum price for filtering flights.</param>
    /// <param name="maxPrice">The maximum price for filtering flights.</param>
    /// <param name="minDepartureDate">The minimum departure date for filtering flights.</param>
    /// <param name="maxDepartureDate">The maximum departure date for filtering flights.</param>
    /// <returns>
    /// <para><b>200 OK</b> - A list of aggregated flights that match the filters.</para>
    /// <para><b>400 Bad Request</b> - Invalid input data (e.g., date format or non-numeric price).</para>
    /// <para><b>500 Internal Server Error</b> - A server error occurred while processing the request.</para>
    /// </returns>
    [HttpGet("flights")]
    [SwaggerOperation(
        Summary = "Get aggregated flights",
        Description = "Retrieves a list of aggregated flights based on optional filters provided in the query parameters."
    )]
    [SwaggerResponse(200, "A list of aggregated flights that match the filters.", typeof(IEnumerable<FlightDTO>))]
    [SwaggerResponse(400, "Invalid input data.", typeof(string))]
    [SwaggerResponse(500, "A server error occurred.", typeof(string))]
    public async Task<IActionResult> GetAggregatedFlights(
        [FromQuery] string airline = null,
        [FromQuery] double? minPrice = null,
        [FromQuery] double? maxPrice = null,
        [FromQuery] DateTime? minDepartureDate = null,
        [FromQuery] DateTime? maxDepartureDate = null)
    {
        var query = new GetAggregatedFlightsQuery
        {
            Filters = new()
            {
                Airline = airline,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                MinDepartureDate = minDepartureDate,
                MaxDepartureDate = maxDepartureDate
            }
        };

        var response = await mediator.Send(query);
        return Ok(response);
    }
}
