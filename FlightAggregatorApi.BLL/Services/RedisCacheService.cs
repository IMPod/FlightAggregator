using FlightAggregatorApi.BLL.Models;
using Newtonsoft.Json;
using StackExchange.Redis;


namespace FlightAggregatorApi.BLL.Services
{
    public class RedisCacheService : IRedisCacheService
    {
        private readonly IDatabase _database;
        private int _cacheExpirationSeconds = 15;

        public RedisCacheService(IConnectionMultiplexer connectionMultiplexer)
        {
            _database = connectionMultiplexer.GetDatabase();
        }

        public async Task<IEnumerable<FlightDTO>> GetCachedFlightsAsync(string key)
        {
            var cachedData = await _database.StringGetAsync(key);
            return string.IsNullOrEmpty(cachedData) ?
                null : JsonConvert.DeserializeObject<IEnumerable<FlightDTO>>(cachedData);
        }

        public async Task SetCacheAsync(string key, IEnumerable<FlightDTO> flights)
        {
            var value = JsonConvert.SerializeObject(flights);
            await _database.StringSetAsync(key, value, TimeSpan.FromSeconds(_cacheExpirationSeconds));
        }
    }
}
