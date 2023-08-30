using Rain.Clients;
using Serilog;
using Rain.Helpers;
using Rain.Infrastructure;
using Rain.Model;

namespace Rain.Quartz
{
    public class AlphaVantageSynchronizer : IAlphaVantageSynchronizer
    {
        private const int PageSize = 10000;
        private readonly IStockDataSqlRepository _stockDataSqlRepository;
        private readonly IAlphavantageApiClient _alphavantageApiClient;


        public AlphaVantageSynchronizer(IStockDataSqlRepository stockDataSqlRepository, IAlphavantageApiClient alphavantageApiClient)
        {
            _stockDataSqlRepository = stockDataSqlRepository;
            _alphavantageApiClient = alphavantageApiClient;
        }
        public async Task<BatchPageResponse> Synchronize(int startIndex)
        {
            if (startIndex == 0)
            {
                
                Log.Information($"Start indexing ebook data");
            }

            int pageIndex = startIndex;

            Log.Information($"Getting batch {pageIndex + 1} from ebook");

            var data = await _alphavantageApiClient.GetDailyTimeSeriesAsync("IBM");
            var stockDatas =data.TimeSeriesDaily.Select(x => x.Value.ToStockData("IBM", x.Key, data.MetaData.TimeZone)).ToList();
            if (stockDatas.Any())
            {
                await _stockDataSqlRepository.BulkInsertData(stockDatas);
            }

            return new BatchPageResponse
            {
                NextPageIndex = stockDatas.Count() >= PageSize ? pageIndex + 1 : null,
                PageSize = stockDatas.Count(),
            };
        }
    }
}
