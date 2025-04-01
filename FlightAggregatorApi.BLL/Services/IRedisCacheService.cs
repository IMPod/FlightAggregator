using FlightAggregatorApi.BLL.Models;

namespace FlightAggregatorApi.BLL.Services;

public interface IRedisCacheService
{
    Task<List<FlightDTO>> GetCachedFlightsAsync(string key);
    Task SetCacheAsync(string key, List<FlightDTO> flights);
}