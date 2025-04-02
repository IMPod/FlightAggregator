using Moq;
using FluentAssertions;
using FlightAggregatorApi.BLL.Handlers;
using FlightAggregatorApi.BLL.Models;
using FlightAggregatorApi.BLL.Queries;
using FlightAggregatorApi.BLL.Services;

namespace FlightAggregatorTests;

public class GetAggregatedFlightsQueryHandlerTests
{
    private readonly Mock<IFlightSourceService> _flightSourceServiceMock;
    private readonly Mock<IRedisCacheService> _redisCacheServiceMock;
    private readonly GetAggregatedFlightsQueryHandler _handler;

    public GetAggregatedFlightsQueryHandlerTests()
    {
        _flightSourceServiceMock = new Mock<IFlightSourceService>();
        _redisCacheServiceMock = new Mock<IRedisCacheService>();

        _handler = new GetAggregatedFlightsQueryHandler(
            _flightSourceServiceMock.Object,
            _redisCacheServiceMock.Object
        );
    }

    [Fact]
    public async Task Handle_ReturnsCachedFlights_WhenCacheExists()
    {
        // Arrange
        var filters = new FilterParams { Airline = "Airline1" };
        var request = new GetAggregatedFlightsQuery { Filters = filters, SortBy = "price", Descending = false };
        var cachedFlights = new List<FlightDTO> { new() { Price = 100 } };

        _redisCacheServiceMock
            .Setup(x => x.GetCachedFlightsAsync(It.IsAny<string>()))
            .ReturnsAsync(cachedFlights);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Flights.Should().BeEquivalentTo(cachedFlights);
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ReturnsFlightsFromSource_WhenCacheMisses()
    {
        // Arrange
        var filters = new FilterParams { Airline = "Airline2" };
        var request = new GetAggregatedFlightsQuery { Filters = filters, SortBy = "price", Descending = false };

        _redisCacheServiceMock
            .Setup(x => x.GetCachedFlightsAsync(It.IsAny<string>()))
            .ReturnsAsync((List<FlightDTO>?)null);

        _flightSourceServiceMock
            .Setup(x => x.GetFlightsAsync("FlightSource1", filters, It.IsAny<CancellationToken>()))
            .ReturnsAsync("[{\"Price\":200}]");

        _flightSourceServiceMock
            .Setup(x => x.GetFlightsAsync("FlightSource2", filters, It.IsAny<CancellationToken>()))
            .ReturnsAsync("[{\"Price\":300}]");

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Flights.Should().HaveCount(2);
        result.Flights.Should().ContainEquivalentOf(new FlightDTO { Price = 200 });
        result.Flights.Should().ContainEquivalentOf(new FlightDTO { Price = 300 });

        _redisCacheServiceMock.Verify(x => x.SetCacheAsync(It.IsAny<string>(), It.IsAny<List<FlightDTO>>()), Times.Once);
    }

    [Fact]
    public async Task Handle_SortsByPriceAscending_WhenSortByIsPrice()
    {
        // Arrange
        var filters = new FilterParams();
        var request = new GetAggregatedFlightsQuery { Filters = filters, SortBy = "price", Descending = false };

        _redisCacheServiceMock
            .Setup(x => x.GetCachedFlightsAsync(It.IsAny<string>()))
            .ReturnsAsync((List<FlightDTO>?)null);

        _flightSourceServiceMock
            .Setup(x => x.GetFlightsAsync(It.IsAny<string>(), filters, It.IsAny<CancellationToken>()))
            .ReturnsAsync("[{\"Price\":500}, {\"Price\":100}, {\"Price\":300}]");

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Flights.Should().BeInAscendingOrder(f => f.Price);
    }

    [Fact]
    public async Task Handle_SortsByDepartureDescending_WhenSortByIsDeparture()
    {
        // Arrange
        var filters = new FilterParams();
        var request = new GetAggregatedFlightsQuery { Filters = filters, SortBy = "departure", Descending = true };

        _redisCacheServiceMock
            .Setup(x => x.GetCachedFlightsAsync(It.IsAny<string>()))
            .ReturnsAsync((List<FlightDTO>?)null);

        _flightSourceServiceMock
            .Setup(x => x.GetFlightsAsync(It.IsAny<string>(), filters, It.IsAny<CancellationToken>()))
            .ReturnsAsync("[{\"DepartureDate\":\"2025-04-10\"}, {\"DepartureDate\":\"2025-04-01\"}, {\"DepartureDate\":\"2025-04-05\"}]");

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Flights.Should().BeInDescendingOrder(f => f.DepartureDate);
    }

    [Fact]
    public async Task Handle_ReturnsError_WhenSourceTimesOut()
    {
        // Arrange
        var filters = new FilterParams();
        var request = new GetAggregatedFlightsQuery { Filters = filters, SortBy = "price", Descending = false };

        _redisCacheServiceMock
            .Setup(x => x.GetCachedFlightsAsync(It.IsAny<string>()))
            .ReturnsAsync((List<FlightDTO>?)null);

        _flightSourceServiceMock
            .Setup(x => x.GetFlightsAsync("FlightSource1", filters, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TaskCanceledException());

        _flightSourceServiceMock
            .Setup(x => x.GetFlightsAsync("FlightSource2", filters, It.IsAny<CancellationToken>()))
            .ReturnsAsync("[{\"Price\":100}]");

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Errors.Should().Contain(e => e.Contains("FlightSource1 timed out"));
    }
}