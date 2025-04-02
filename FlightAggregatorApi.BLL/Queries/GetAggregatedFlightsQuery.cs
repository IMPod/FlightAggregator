using MediatR;
using FlightAggregatorApi.BLL.Models;

namespace FlightAggregatorApi.BLL.Queries;

public class GetAggregatedFlightsQuery : IRequest<AggregatedFlightsResponse>
{
    public FilterParams Filters { get; init; }
    public string SortBy { get; init; } = "price";
    public bool Descending { get; init; } = false;
}