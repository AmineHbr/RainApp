using Microsoft.AspNetCore.Mvc;
using Rain.Clients;
using Rain.Helpers;
using Rain.Infrastructure;
using Rain.Model;
using System;
using System.Globalization;

namespace Rain.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AlphaVantageController : ControllerBase
    {

        private readonly IAlphavantageApiClient _alphavantageApiClient;
        private readonly IStockDataSqlRepository _stockDataSqlRepository;

        public AlphaVantageController(IAlphavantageApiClient alphavantageApiClient,IStockDataSqlRepository stockDataSqlRepository)
        {

            _alphavantageApiClient = alphavantageApiClient;
            _stockDataSqlRepository = stockDataSqlRepository;

        }
        [HttpGet("GetTimeSeries")]
        public async Task<TimeSeriesResponse> GetTimeSeries()
        {
            var result = await _alphavantageApiClient.GetDailyTimeSeriesAsync("IBM");

            var stockDatas = result.TimeSeriesDaily.Select(x => x.Value.ToStockData("IBM", x.Key, result.MetaData.TimeZone)).ToList();
            _stockDataSqlRepository.BulkInsertData(stockDatas);
            return result;
        }
     
    }
}
