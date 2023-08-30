using Rain.Model;
using System.Globalization;
using System.Runtime.CompilerServices;
using static System.Net.Mime.MediaTypeNames;

namespace Rain.Helpers
{
    public static class TimesSeriesEntryHelper
    {
        public static StockData ToStockData(this TimeSeriesEntry request, string symbole, string serieDate, string timeZone) => new StockData
        {
            Close = double.Parse(request.Close, CultureInfo.InvariantCulture),
            High = double.Parse(request.High, CultureInfo.InvariantCulture),
            Low = double.Parse(request.Low, CultureInfo.InvariantCulture),
            Open = double.Parse(request.Open, CultureInfo.InvariantCulture),
            Symbole = symbole,
            Volume = double.Parse(request.Volume, CultureInfo.InvariantCulture),
            SerieDate = DateTime.Parse(serieDate),
            TimeZone = timeZone
        };
    }
}
