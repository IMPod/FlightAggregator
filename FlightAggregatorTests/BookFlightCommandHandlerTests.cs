using FlightAggregatorApi.BLL.Commands;
using FlightAggregatorApi.BLL.Handlers;
using FlightAggregatorApi.BLL.Models;
using FlightAggregatorApi.BLL.Services;
using Moq;
using FluentAssertions;

namespace FlightAggregatorTests
{
    public class BookFlightCommandHandlerTests
    {
        private readonly Mock<IFlightSourceService> _flightSourceServiceMock;
        private readonly BookFlightCommandHandler _handler;

        public BookFlightCommandHandlerTests()
        {
            _flightSourceServiceMock = new Mock<IFlightSourceService>();
            _handler = new BookFlightCommandHandler(_flightSourceServiceMock.Object);
        }

        [Fact]
        public async Task Handle_ReturnsSuccess_WhenBookingIsSuccessful()
        {
            // Arrange
            var command = new BookFlightCommand(new FlightBookingRequest
            {
                FlightId = 123,
                Seats = 2,
                Source = "FlightSource1"
            });

            var expectedResponse = new BookingResponse { Success = true, Message = "Booking confirmed" };

            _flightSourceServiceMock
                .Setup(x => x.BookFlightAsync(command.Request.Source, It.IsAny<FlightBookingRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().BeEquivalentTo(expectedResponse);
        }

        [Fact]
        public async Task Handle_ReturnsError_WhenSourceIsMissing()
        {
            // Arrange
            var command = new BookFlightCommand(new FlightBookingRequest
            {
                FlightId = 123,
                Seats = 2,
                Source = "" // Пустой источник
            });

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().BeEquivalentTo(new BookingResponse
            {
                Success = false,
                Message = "Flight source is required"
            });

            _flightSourceServiceMock.Verify(x => x.BookFlightAsync(It.IsAny<string>(), It.IsAny<FlightBookingRequest>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ReturnsFailure_WhenBookingFails()
        {
            // Arrange
            var command = new BookFlightCommand(new FlightBookingRequest
            {
                FlightId = 123,
                Seats = 2,
                Source = "FlightSource1"
            });

            var expectedResponse = new BookingResponse { Success = false, Message = "Booking failed" };

            _flightSourceServiceMock
                .Setup(x => x.BookFlightAsync(command.Request.Source, It.IsAny<FlightBookingRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().BeEquivalentTo(expectedResponse);
        }
    }
}
