using Newtonsoft.Json;
using Rain.Model;
using RestEase.Implementation;

namespace Rain.Clients
{
    public class AlphavantageApiClient : IAlphavantageApiClient
    {
        private const string sectionName = "alphaVantage";
        private readonly AlphaVantageOption _alphaVantageOption;
        private readonly IAlphaVantageApi _instance;

        public AlphavantageApiClient(AlphaVantageOption alphaVantageOption, IHttpClientFactory httpClientFactory)
        {
            _alphaVantageOption = alphaVantageOption;

            var httpClient = new HttpClient()
            {
                BaseAddress = new Uri(alphaVantageOption.ApiBaseUrl),
                Timeout = TimeSpan.FromMinutes(5)
            };

            _instance = httpClientFactory.CreateApiInstance<IAlphaVantageApi>(httpClient);
        }

        public async Task<TimeSeriesResponse> GetDailyTimeSeriesAsync(string symbol)
        {
            var apiUrl = _alphaVantageOption.ApiBaseUrl + $"?function=TIME_SERIES_DAILY&symbol={symbol}&apikey={_alphaVantageOption.ApiKey}";
            var result = await _instance.GetDailyTimeSeriesAsync(apiUrl);
            return result;
        }
    }
}
