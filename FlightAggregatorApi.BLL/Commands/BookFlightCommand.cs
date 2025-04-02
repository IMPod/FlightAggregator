using MediatR;
using FlightAggregatorApi.BLL.Models;

namespace FlightAggregatorApi.BLL.Commands;

public class BookFlightCommand(FlightBookingRequest request) : IRequest<BookingResponse>
{
    public FlightBookingRequest Request { get; } = request;
}