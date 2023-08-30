using System.Data;

namespace Rain.Infrastructure
{
    public interface IDatabaseConnectionFactory
    {
        public IDbConnection GetDbConnection();
    }
}
