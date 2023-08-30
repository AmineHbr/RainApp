using Rain.Model;

namespace Rain.Quartz
{
    public interface IElasticSynchronizer
    {
        Task<BatchPageResponse> SynchronizeElasticStockData(string indexName, int startIndex);
    }
}
