using Rain.Model;

namespace Rain.Clients
{
    public static class Extensions
    {
        private const string SectionName = "ebook";
        private const string SectionVantageName = "alphaVantage";
        public static IServiceCollection AddEbookClient(this IServiceCollection builder, string sectionName = SectionName)
        {
            if (string.IsNullOrWhiteSpace(sectionName))
            {
                sectionName = SectionName;
            }

            ServiceProvider provider = builder.BuildServiceProvider();

            var options = provider.GetService<IConfiguration>().GetOptions<EbookOptions>(sectionName);
            return builder.AddEbookClient(options);
        }

        public static IServiceCollection AddEbookClient(this IServiceCollection builder, EbookOptions options)
        {
            builder.AddSingleton(options);
            builder.AddTransient<IHttpClientFactory, HttpClientFactory>();
            builder.AddTransient<IEbookApiClient, EbookApiClient>();
            return builder;
        }
        public static IServiceCollection AddAlphaVantageClient(this IServiceCollection builder, string sectionVantageName = SectionVantageName)
        {
            if (string.IsNullOrWhiteSpace(sectionVantageName))
            {
                sectionVantageName = SectionVantageName;
            }

            ServiceProvider provider = builder.BuildServiceProvider();

            var options = provider.GetService<IConfiguration>().GetOptions<AlphaVantageOption>(sectionVantageName);
            return builder.AddAlphaVantageClient(options);
        }
        public static IServiceCollection AddAlphaVantageClient(this IServiceCollection builder,AlphaVantageOption options)
        {
            builder.AddSingleton(options);
            builder.AddTransient<IHttpClientFactory, HttpClientFactory>();
            builder.AddTransient<IAlphavantageApiClient, AlphavantageApiClient>();
            return builder;
        }

        public static TModel GetOptions<TModel>(this IConfiguration configuration, string sectionName) where TModel : new()
        {
            TModel model = new TModel();
            configuration.GetSection(sectionName).Bind(model);
            return model;
        }
    }
}
public class EbookOptions
{
    public string IdentityProviderUrl { get; set; }
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
    public string ApiBaseUrl { get; set; }
}