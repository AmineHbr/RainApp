using Dapper.Contrib.Extensions;

namespace Rain.Model
{
    [Table("StockData")]
    public class StockData
    {
        public DateTime SerieDate { get; set; }
        public string Symbole { get; set; }
        public string TimeZone { get; set; }
        public double? Open { get; set; }
        public double? High { get; set; }
        public double? Low { get; set; }
        public double? Close { get; set; }
        public double? Volume { get; set; }
    }
}
