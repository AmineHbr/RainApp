using Rain.Model;

namespace Rain.Clients
{
    public interface IAlphavantageApiClient
    {
        Task<TimeSeriesResponse> GetDailyTimeSeriesAsync(string symbol);
    }
}
