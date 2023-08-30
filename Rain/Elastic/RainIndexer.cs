using Nest;
using Serilog;
using System.Text.RegularExpressions;

namespace Rain.Elastic
{
    public class RainIndexer : IRainIndexer
    {
        private readonly IElasticClient _elasticClient;
        private readonly ElasticOptions _options;
     

        public RainIndexer(IElasticClient elasticClient, ElasticOptions options)
        {
            _elasticClient = elasticClient;
            _options = options;
        
        }

        public async Task DeleteIndexAsync(string indexName)
        {
            var exists = await _elasticClient.Indices.ExistsAsync(indexName);

            if (exists.Exists)
            {
                var response = await _elasticClient.Indices.DeleteAsync(indexName);

                if (!response.IsValid)
                {
                    throw new Exception(response.DebugInformation);
                }
            }
        }
        public async Task BulkIndexData<T>(string indexName, IEnumerable<T> data) where T : class
        {
            try
            {
                Log.Information($"Start indexing {data.Count()} rows to index {indexName}");

                var bulkAllObservable = _elasticClient.BulkAll(data, b => b
                                                        .Index(indexName)
                                                        .BackOffTime(TimeSpan.FromSeconds(10))
                                                        .BackOffRetries(2)
                                                        .RefreshOnCompleted(true)
                                                        .MaxDegreeOfParallelism(10)
                                                        .Size(_options.ElasticBatchSize));

                var waitHandle = new ManualResetEvent(false);
                using var observableSubscribtion = bulkAllObservable.Subscribe(new BulkAllObserver(
                    onError: exception =>
                    {
                        Log.Error(exception, $"Error while bulking data to index {indexName}");
                        throw exception;
                    },
                    onCompleted: () =>
                    {
                        waitHandle.Set();
                    }
                ));

                await Task.Run(() =>
                {
                    waitHandle.WaitOne();
                });

            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error while indexing data to index {indexName}");
            }
        }

        public async Task<T> GetLastIndexedDocumentByProperty<T>(string indexName, string propertyName) where T : class
        {
            var response = await _elasticClient.SearchAsync<T>(s => s
                                           .Index(indexName)
                                           .From(0)
                                           .Size(1)
                                           .Sort(a => a.Descending(propertyName)));

            return response.Documents.FirstOrDefault();
        }


        public async Task CreateIndexIfNotExists<T>(string indexName) where T : class
        {
            var exists = await _elasticClient.Indices.ExistsAsync(indexName);

            if (!exists.Exists)
            {
                var response = _elasticClient.Indices.Create(indexName,
                    index => index.Map<T>(
                        x => x.AutoMap()
                    ));

                if (!response.IsValid)
                {
                    throw new Exception(response.DebugInformation);
                }
            }
        }

        public async Task<string> GetLiveIndexName(string aliasName)
        {
            var response = await _elasticClient.Indices.GetAliasAsync(aliasName);

            if (response.IsValid)
            {
                return response.Indices?.Keys.FirstOrDefault()?.Name;
            }

            return null;
        }

        public async Task SetLiveIndex(string indexName, string aliasName)
        {
            var currentLiveIndex = await GetLiveIndexName(aliasName);

            if (!string.IsNullOrEmpty(currentLiveIndex))
            {
                var removeAliasResponse = await _elasticClient.Indices.DeleteAliasAsync(currentLiveIndex, aliasName);

                if (!removeAliasResponse.IsValid)
                {
                    throw new Exception(removeAliasResponse.DebugInformation);
                }
            }

            var aliasResponse = await _elasticClient.Indices.PutAliasAsync(indexName, aliasName);

            if (!aliasResponse.IsValid)
            {
                throw new Exception(aliasResponse.DebugInformation);
            }
        }

        public async Task DeleteNonLiveIndices(Dictionary<string, string> indicesDict)
        {
            var getIndicesResponse = await _elasticClient.Indices.GetAsync(new GetIndexRequest(Indices.All));

            if (!getIndicesResponse.IsValid)
            {
                throw new Exception(getIndicesResponse.DebugInformation);
            }

            var indicesToDelete = new List<string>();

            foreach (var vectorIndices in indicesDict)
            {
                string pattern = @$"^{vectorIndices.Key}(-\d+)?$";
                Regex regex = new Regex(pattern);
                var matchIndices = getIndicesResponse.Indices.Where(index => regex.IsMatch(index.Key.Name)
                                       && !index.Value.Aliases.ContainsKey(vectorIndices.Value)).Select(index => index.Key.Name).ToList();
                if (matchIndices.Any())
                {
                    indicesToDelete.AddRange(matchIndices);
                }
            }

            if (indicesToDelete.Any())
            {
                var deleteIndicesResponse = await _elasticClient.Indices.DeleteAsync(Indices.Index(indicesToDelete));

                if (!deleteIndicesResponse.IsValid)
                {
                    throw new Exception(deleteIndicesResponse.DebugInformation);
                }
            }
            else
            {
                Log.Information("No indices without alias found.");
            }
        }
    }
}
