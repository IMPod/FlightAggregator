using FlightAggregatorApi.BLL.Models;

namespace FlightAggregatorApi.BLL.Services;

public interface IRedisCacheService
{
    Task<IEnumerable<FlightDTO>> GetCachedFlightsAsync(string key);
    Task SetCacheAsync(string key, IEnumerable<FlightDTO> flights);
}