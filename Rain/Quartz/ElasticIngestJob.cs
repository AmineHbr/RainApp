using Quartz;
using Rain.Elastic;
using Rain.Infrastructure;
using Rain.Model;
using Serilog;
using Serilog.Context;
using System.Text.Json;

namespace Rain.Quartz
{

    [DisallowConcurrentExecution]
    public class ElasticIngestJob : IJob
    {
        private readonly IElasticSynchronizer _elasticSynchronizer;
        private readonly IExecutionLogRepository _executionLogRepository;
        private readonly ElasticOptions _options;
        private readonly IRainIndexer _rainIndexer;
        public const string JobId = "JobId";

        private string _stockDataIndexName;
        private string _stockdataAlias;
        public ElasticIngestJob(IElasticSynchronizer elasticSynchronizer, IExecutionLogRepository executionLogRepository, ElasticOptions options, IRainIndexer rainIndexer)
        {
            _elasticSynchronizer = elasticSynchronizer;
            _executionLogRepository = executionLogRepository;
            _rainIndexer = rainIndexer;
            _options = options;

            _stockdataAlias = $"{_options.StockDataIndexName}-latest";
        }
        public async Task Execute(IJobExecutionContext context)
        {
            
                try
                {
                    JobDataMap dataMap = context.MergedJobDataMap;
                    var guid = dataMap.GetNullableGuid("JobId") ?? Guid.NewGuid();
                    using (LogContext.PushProperty("ExecutionId", guid))
                    {

                        Log.Information("Starting job ...");

                        await _executionLogRepository.AddLog(new ExecutionLog
                        {
                            Id = guid,
                            Params = JsonSerializer.Serialize(new { Type = "ELASTICBATCH" }),
                            Type = ExecutionType.Automatic,
                            Status = Status.Scheduled
                        });

                        try
                        {
                            int totalIngesteItems = 0;
                            Log.Information("Start ingesting data...");
                            await _executionLogRepository.SetExecutionStatus(guid, Status.InProgress);

                            await PrepareIndices();
                            totalIngesteItems = await SynchronizeData(context.CancellationToken);
                            await SwitchLiveIndices();
                           await _rainIndexer.DeleteNonLiveIndices(new Dictionary<string, string> { { _options.StockDataIndexName, _stockdataAlias } });
                            if (context.CancellationToken.IsCancellationRequested)
                            {
                                Log.Warning($"Job {guid} aborted");
                                await _executionLogRepository.SetExecutionStatus(guid, Status.Aborted);
                            }
                            else
                            {
                                Log.Information($"Job {guid} successfully executed");
                                await _executionLogRepository.SetExecutionStatus(guid, Status.Succeded);

                                using (LogContext.PushProperty("totalIngestedItems", totalIngesteItems))
                                {
                                    Log.Information($"End ingesting  data, total ingested items = {totalIngesteItems}...");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex, "Error while ingesting  data");
                            await _executionLogRepository.SetExecutionStatus(guid, Status.Failed);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Fatal(ex, "Error  ingesting job");
                }
            }
        
        private async Task<int> SynchronizeData(CancellationToken cancellationToken)
        {
            Dapper.SqlMapper.Settings.CommandTimeout = 0;
            int totalIndexedCartItems = 0;
            int index = 0;

            while (index >= 0 && !cancellationToken.IsCancellationRequested)
            {
                var p = await _elasticSynchronizer.SynchronizeElasticStockData(_stockDataIndexName, index);
                index = p.NextPageIndex ?? -1;
                totalIndexedCartItems += p.PageSize;
            }

            return totalIndexedCartItems;
        }

        private async Task PrepareIndices()
        {
            _stockDataIndexName = await _rainIndexer.GetLiveIndexName(_stockdataAlias) ?? _options.StockDataIndexName;


            _stockDataIndexName = $"{_options.StockDataIndexName}-{DateTime.Now.Ticks}";


            await _rainIndexer.CreateIndexIfNotExists<StockData>(_stockDataIndexName);

        }

        private async Task SwitchLiveIndices()
        {
            await _rainIndexer.SetLiveIndex(_stockDataIndexName, _stockdataAlias);

        }
    }
}
