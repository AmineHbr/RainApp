using Rain.Model;

namespace Rain.Infrastructure
{
    public interface IStockDataSqlRepository
    {
        public Task BulkInsertData(IEnumerable<StockData> entry);
        Task<IEnumerable<StockData>> GetStockData(int pageSize, int pageIndex);
    }
}
