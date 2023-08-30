using Microsoft.AspNetCore.Mvc;
using Rain.Model;
using RestEase;

namespace Rain.Clients
{
    public interface IAlphaVantageApi
    {
        [Get("{url}")]
        Task<TimeSeriesResponse> GetDailyTimeSeriesAsync([Path(UrlEncode =false)] string url);
        
    }
}
