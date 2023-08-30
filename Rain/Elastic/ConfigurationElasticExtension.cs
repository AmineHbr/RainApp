using Nest;
using Rain.Clients;

namespace Rain.Elastic
{
    public static class ConfigurationElasticExtension
    {
        private const string SectionName = "elastic";

        public static IServiceCollection AddRainElasticClient(this IServiceCollection builder, string sectionName = SectionName)
        {
            if (string.IsNullOrWhiteSpace(sectionName))
            {
                sectionName = SectionName;
            }

            ServiceProvider provider = builder.BuildServiceProvider();

            var options = provider.GetService<IConfiguration>().GetOptions<ElasticOptions>(sectionName);
            return builder.AddVectorElasticClient(options);
        }

        public static IServiceCollection AddVectorElasticClient(this IServiceCollection builder, ElasticOptions options)
        {
            builder.AddSingleton(options);
            builder.AddTransient<IElasticClient, RainElasticClient>();
            builder.AddTransient<IRainIndexer, RainIndexer>();
            return builder;
        }
    }
}
