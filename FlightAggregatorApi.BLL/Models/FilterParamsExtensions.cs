using System.Reflection;

namespace FlightAggregatorApi.BLL.Models;

public static class FilterParamsExtensions
{
    public static string GenerateCacheKey(this FilterParams filters)
    {
        var properties = typeof(FilterParams).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        var filterString = string.Join("_", properties
            .Where(p => p.GetValue(filters) != null) 
            .Select(p => $"{p.Name}:{p.GetValue(filters)}")); 

        return $"AggregatedFlights:{filterString}";
    }
}