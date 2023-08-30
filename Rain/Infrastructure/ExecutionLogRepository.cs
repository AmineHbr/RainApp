using Dapper;
using Rain.Model;

namespace Rain.Infrastructure
{
    public class ExecutionLogRepository: IExecutionLogRepository
    {
        private readonly IDatabaseConnectionFactory _connectionFactory;

        public ExecutionLogRepository(IDatabaseConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task AddLog(ExecutionLog executionLog)
        {
            var sqlQuery = $@"INSERT INTO [dbo].[ExecutionLogs]
                                    ([Id]
                                    ,[ExecutionDate]
                                    ,[Type]
                                    ,[Params]
                                    ,[Status])
                                VALUES
                                    (@Id
                                    ,@ExecutionDate
                                    ,@Type
                                    ,@Params
                                    ,@Status);";

            await _connectionFactory.GetDbConnection().ExecuteAsync(sqlQuery, executionLog);
        }

        public async Task<ExecutionLog> GetLog(Guid id)
        {
            string sql = "SELECT * FROM [dbo].[ExecutionLogs] WHERE [Id] = @Id;";

            return await _connectionFactory.GetDbConnection().QuerySingleOrDefaultAsync<ExecutionLog>(sql, new { Id = id });
        }

        public async Task<IEnumerable<ExecutionLog>> GetLogs()
        {
            string sql = "SELECT * FROM [dbo].[ExecutionLogs];";

            return await _connectionFactory.GetDbConnection().QueryAsync<ExecutionLog>(sql);
        }

        public async Task SetExecutionStatus(Guid executionLogId, Status status)
        {
            string sql = "UPDATE [dbo].[ExecutionLogs] SET Status = @Status";
            sql = status == Status.InProgress ? $"{sql} , ExecutionDate = @ExecutionDate" : sql;
            sql = $"{sql} WHERE [Id] = @Id;";

            await _connectionFactory.GetDbConnection().ExecuteAsync(sql, new { Id = executionLogId, Status = status, ExecutionDate = DateTime.Now });
        }
    }
}
