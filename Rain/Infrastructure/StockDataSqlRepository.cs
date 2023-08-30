using Dapper;
using Dapper.Contrib.Extensions;
using Rain.Model;
using System.Data;
using System.Data.SqlClient;
using Z.Dapper.Plus;

namespace Rain.Infrastructure
{
    public class StockDataSqlRepository : IStockDataSqlRepository
    {
        private readonly string _connectionString = "Data Source=DESKTOP-VI8JR5D\\SQLEXPRESS;Initial Catalog=RAINDB;Integrated Security=True;";
        private readonly IDatabaseConnectionFactory _databaseFactory;
        public StockDataSqlRepository(IDatabaseConnectionFactory databaseFactory) => _databaseFactory = databaseFactory;
        public async Task BulkInsertData(IEnumerable<StockData> entry)
        {
            //using (IDbConnection dbConnection = new SqlConnection(_connectionString))
            //{
            //    dbConnection.Open();
            //    dbConnection.BulkMerge(entry);
            //}
            var connectionString = _databaseFactory.GetDbConnection();
            using var sqlConnection = _databaseFactory.GetDbConnection() as SqlConnection;
                sqlConnection.Open();
                await sqlConnection.BulkUpsert("StockData", typeof(StockData), entry, new[] { nameof(StockData.SerieDate) });
            

        }

        public async Task<IEnumerable<StockData>> GetStockData(int pageSize,int pageIndex)
        {
            var query = @$"Select * from StockData ORDER BY SerieDate DESC OFFSET {pageSize}*{pageIndex} ROWS FETCH NEXT {pageSize} ROWS ONLY";
            using var sqlConnection = _databaseFactory.GetDbConnection() as SqlConnection;
            sqlConnection.Open();
            var data = await sqlConnection.QueryAsync<StockData>(query);
            return data.ToList();
        }
   
    }

}
