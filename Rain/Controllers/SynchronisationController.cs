using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Quartz;
using Rain.Infrastructure;
using Rain.Model;
using Rain.Quartz;

namespace Rain.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SynchronisationController : ControllerBase
    {
        private readonly ISchedulerFactory _schedulerFactory;
        private readonly IExecutionLogRepository _executionLogRepository;
        public SynchronisationController(ISchedulerFactory schedulerFactory, IExecutionLogRepository executionLog)
        {
            _schedulerFactory = schedulerFactory;
            _executionLogRepository = executionLog;
        }

        [HttpPost("AplhaVantage")]
        public async Task<ActionResult> SynchronizeAlphaVantage()
        {
            var guid = Guid.NewGuid();
            var jobDta = JobExtensions.CreateAlphaVantageJobData(null, guid);
            var execution = await StartSynchronizationJob(jobDta, guid, JobExtensions.IngestJobKey);
            return Ok(execution);
        }
        private async Task<ExecutionLog> StartSynchronizationJob(JobDataMap jobData,Guid guid, JobKey jobKey)
        {
            var scheduler = await _schedulerFactory.GetScheduler(JobExtensions.MainSchedulerName);
            await scheduler.TriggerJob(jobKey, jobData);
            return await _executionLogRepository.GetLog(guid);
        }
    }
}
