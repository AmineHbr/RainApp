using Rain.Model;

namespace Rain.Infrastructure
{
    public interface IExecutionLogRepository
    {
        Task AddLog(ExecutionLog executionLog);
        Task<ExecutionLog> GetLog(Guid id);
        Task<IEnumerable<ExecutionLog>> GetLogs();
        Task SetExecutionStatus(Guid executionLogId, Status status);
    }
}
