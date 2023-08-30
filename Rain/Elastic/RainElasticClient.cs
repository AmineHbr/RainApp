using Nest;

namespace Rain.Elastic
{
    public class RainElasticClient : ElasticClient
    {
        public RainElasticClient(ElasticOptions options) : base(new ConnectionSettings(new Uri(options.ClusterUrl))
                               .BasicAuthentication(options.UserName, options.Password)
                               .DefaultFieldNameInferrer(p => p.ToLower())
                               .PrettyJson().DisableDirectStreaming().EnableApiVersioningHeader())
        {
        }
    }
}
