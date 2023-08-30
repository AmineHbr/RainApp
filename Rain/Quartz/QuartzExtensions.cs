using Quartz;
using Quartz.AspNetCore;

namespace Rain.Quartz
{
    public static class QuartzExtensions
    {
        public static IServiceCollection AddJobScheduling(this IServiceCollection builder)
        {
            builder.AddQuartz(q =>
            {
                // handy when part of cluster or you want to otherwise identify multiple schedulers
                q.SchedulerId = "vector-ingest-scheduler";

                q.UseMicrosoftDependencyInjectionJobFactory();

                q.AddJob<AlphaVantageIngestJob>(j => j
                    .StoreDurably()
                    .WithIdentity(JobExtensions.IngestJobKey)
                    .WithDescription("ingest job")
                );
                q.AddJob<ElasticIngestJob>(j => j
                    .StoreDurably()
                    .WithIdentity(JobExtensions.IngestJobKey)
                    .WithDescription("ingest job"));

                var JobData = JobExtensions.CreateAlphaVantageJobData(null);
                var JobDataElastic = JobExtensions.CreateElasticStockJobData(null);

                q.AddTrigger(t => t
                    .WithIdentity(JobExtensions.IngestTriggerKey)
                    .ForJob(JobExtensions.IngestJobKey).UsingJobData(JobData)
                    .StartNow()
                    .WithSimpleSchedule(x => x.WithInterval(TimeSpan.FromMinutes(50)).RepeatForever())
                    .WithDescription("trigger  ingest every day 3m")
                );

                q.AddTrigger(t => t
                  .WithIdentity(JobExtensions.ElasticTriggerKey)
                  .ForJob(JobExtensions.ElasticJobKey).UsingJobData(JobDataElastic)
                  .StartNow()
                  .WithSimpleSchedule(x => x.WithInterval(TimeSpan.FromMinutes(30)).RepeatForever())
                  .WithDescription("trigger  ingest every day 30m")
              );
            });

            // ASP.NET Core hosting
            builder.AddQuartzServer(options =>
            {
                // when shutting down we want jobs to complete gracefully
                options.WaitForJobsToComplete = true;
            });

            return builder;
        }
    }
}
