
using Quartz;
using Rain.Infrastructure;
using Rain.Model;
using Serilog;
using Serilog.Context;
using System.Text.Json;

namespace Rain.Quartz
{
    [DisallowConcurrentExecution]
    public class AlphaVantageIngestJob : IJob
    {
        public static class SynchronizationJobParameters
        {
            public const string IngestDateTime = "IngestDateTime";
            public const string JobId = "JobId";
        }

        private readonly IExecutionLogRepository _executionLogRepository;
        private readonly IAlphaVantageSynchronizer _alphaVantageSynchronizer;
        public AlphaVantageIngestJob(IExecutionLogRepository executionLogRepository, IAlphaVantageSynchronizer alphaVantageSynchronizer)
        {
            _alphaVantageSynchronizer = alphaVantageSynchronizer;
            _executionLogRepository = executionLogRepository;
        }
        public async  Task Execute(IJobExecutionContext context)
        {
            try
            {
                JobDataMap dataMap = context.MergedJobDataMap;
                var guid = dataMap.GetNullableGuid("JobId") ?? Guid.NewGuid();
                using (LogContext.PushProperty("ExecutionId", guid))
                {

                    Log.Information("Starting job ...");
                    var minDateTime = ParseJobParameters(dataMap);

                    await _executionLogRepository.AddLog(new ExecutionLog
                    {
                        Id = guid,
                        Params = JsonSerializer.Serialize(new { Type = "AlphaVantage", MinDateTime = minDateTime }),
                        Type = ExecutionType.Automatic,
                        Status = Status.Scheduled
                    });

                    try
                    {
                        int totalIngesteItems = 0;
                        Log.Information("Start ingesting data...");
                        await _executionLogRepository.SetExecutionStatus(guid, Status.InProgress);

                        totalIngesteItems = await SynchronizeData(context.CancellationToken);

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
        private static DateTime? ParseJobParameters(JobDataMap dataMap)
        {
            DateTime minDateTime = dataMap.GetDateTime(SynchronizationJobParameters.IngestDateTime);

            return minDateTime == default(DateTime) ? null : minDateTime;
        }
        private async Task<int> SynchronizeData(CancellationToken cancellationToken)
        {
            Dapper.SqlMapper.Settings.CommandTimeout = 0;
            int totalIndexedCartItems = 0;
            int index = 0;
         
            while (index >= 0 && !cancellationToken.IsCancellationRequested)
            {
                var p = await _alphaVantageSynchronizer.Synchronize(index);
                index = p.NextPageIndex ?? -1;
                totalIndexedCartItems += p.PageSize;
            }

            return totalIndexedCartItems;
        }
    }
}
