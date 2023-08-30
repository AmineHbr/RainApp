using Quartz;

namespace Rain.Quartz
{
    public static class JobExtensions
    {
        public const string MainSchedulerName = "QuartzScheduler";
        public static readonly JobKey IngestJobKey = new JobKey(" ingest batch", " batches");
        public static readonly TriggerKey IngestTriggerKey = new TriggerKey(" synchornizer trigger");
        public static readonly JobKey ElasticJobKey = new JobKey("  Elastic ingest batch", " batches");
        public static readonly TriggerKey ElasticTriggerKey  = new TriggerKey(" synchornizer trigger");
        public static JobDataMap CreateAlphaVantageJobData(DateTime? minDateTime, Guid? jobInstanceId = null)
        {
            Dictionary<string, object> data = new Dictionary<string, object>
            {
                { AlphaVantageIngestJob.SynchronizationJobParameters.IngestDateTime, minDateTime},
            };

            if (jobInstanceId.HasValue)
            {
                data.Add(AlphaVantageIngestJob.SynchronizationJobParameters.JobId, jobInstanceId.Value);
            }

            return new JobDataMap(data as IDictionary<string, object>);
        }
        public static JobDataMap CreateElasticStockJobData( Guid? jobInstanceId = null)
        {
            Dictionary<string, object> data = new Dictionary<string, object>
            {

            };

            if (jobInstanceId.HasValue)
            {
                data.Add(AlphaVantageIngestJob.SynchronizationJobParameters.JobId, jobInstanceId.Value);
            }

            return new JobDataMap(data as IDictionary<string, object>);
        }
    }
}
