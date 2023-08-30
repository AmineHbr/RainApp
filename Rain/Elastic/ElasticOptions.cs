namespace Rain.Elastic
{
    public class ElasticOptions
    {
        public string ClusterUrl { get; set; }

        public string StockDataIndexName { get; set; }

        public int ElasticBatchSize { get; set; }
        public string UserName { get; set; }

        public string Password { get; set; }
    }
}
