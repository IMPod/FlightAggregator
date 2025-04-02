using FlightAggregatorApi.BLL.Commands;
using FlightAggregatorApi.BLL.Models;
using FlightAggregatorApi.BLL.Services;
using MediatR;

namespace FlightAggregatorApi.BLL.Handlers;

public class BookFlightCommandHandler(IFlightSourceService flightSourceService) : IRequestHandler<BookFlightCommand, BookingResponse>
{
    public async Task<BookingResponse> Handle(BookFlightCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.Request.Source))
        {
            return new BookingResponse { Success = false, Message = "Flight source is required" };
        }

        var bookingRequest = new FlightBookingRequest
        {
            FlightId = request.Request.FlightId,
            Seats = request.Request.Seats,
            Source = request.Request.Source
        };

        return await flightSourceService.BookFlightAsync(request.Request.Source, bookingRequest, cancellationToken);
    }
}