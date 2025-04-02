using MediatR;
using FlightAggregatorApi.BLL.Models;

namespace FlightAggregatorApi.BLL.Queries;

public class GetAggregatedFlightsQuery : IRequest<AggregatedFlightsResponse>
{
    public FilterParams Filters { get; set; }
    public string SortBy { get; set; } = "price";
    public bool Descending { get; set; } = false;
}