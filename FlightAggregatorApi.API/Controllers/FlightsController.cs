using FlightAggregatorApi.BLL.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FlightAggregatorApi.API.Controllers
{
    [Route("api/aggregator")]
    [ApiController]
    public class FlightsController(IFlightSourceService flightSourceService) : ControllerBase
    {
        [HttpGet("flights")]
        public async Task<IActionResult> GetAggregatedFlights()
        {
            var flights1 = await flightSourceService.GetFlightsAsync("FlightSource1");
            var flights2 = await flightSourceService.GetFlightsAsync("FlightSource2");

            return Ok(new
            {
                FlightSource1 = flights1,
                FlightSource2 = flights2
            });
        }
    }
}
