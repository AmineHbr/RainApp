using Rain.Clients;
using Rain.Elastic;
using Rain.Infrastructure;
using Rain.Model;
using Serilog;
using System.Drawing.Printing;

namespace Rain.Quartz
{
 
    public class ElasticSynchronizer : IElasticSynchronizer
    {
        private const int PageSize = 50;
        private readonly IStockDataSqlRepository _stockDataSqlRepository;
        private readonly IRainIndexer _rainIndexer;
        private readonly ElasticOptions _options;
        public ElasticSynchronizer(IStockDataSqlRepository stockDataSqlRepository, IRainIndexer rainIndexer, ElasticOptions options)
        {
            _stockDataSqlRepository = stockDataSqlRepository;
            _rainIndexer = rainIndexer;
            _options = options;
        }

        public async Task<BatchPageResponse> SynchronizeElasticStockData(string indexName,int startIndex)
        {
            if (startIndex == 0)
            {

                Log.Information($"Start indexing elastic stockdata");
            }

            int pageIndex = startIndex;

            Log.Information($"Getting batch {pageIndex + 1} from stockdata");

            var data = await _stockDataSqlRepository.GetStockData(PageSize, pageIndex);

            if (data.Any())
            {
               await _rainIndexer.BulkIndexData<StockData>(_options.StockDataIndexName, data);
                Log.Information("indexing the data", data.Count());
            }

            return new BatchPageResponse
            {
                NextPageIndex = data.Count() >= PageSize ? pageIndex + 1 : null,
                PageSize = data.Count(),
            };
        }
    }
}
