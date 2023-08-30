using System.Data;
using System.Data.SqlClient;

namespace Rain.Infrastructure
{
    public class DatabaseConnectionFactory: IDatabaseConnectionFactory
    {
        private readonly IConfiguration _configuration;
        public DatabaseConnectionFactory (IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public IDbConnection GetDbConnection()
        {
            return new SqlConnection(_configuration.GetConnectionString("RainDB"));
        }
    }
}
