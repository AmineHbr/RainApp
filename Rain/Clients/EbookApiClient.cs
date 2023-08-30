using IdentityModel.Client;
using RestEase.Implementation;

namespace Rain.Clients
{
    public class EbookApiClient : IEbookApiClient
    {
        private readonly AccessTokensCacheManager _accessTokensCacheManager = new AccessTokensCacheManager();
        private readonly IEbookApi _instance;
        private readonly EbookOptions _ebookOptions;

        public EbookApiClient(EbookOptions ebookOptions, IHttpClientFactory httpClientFactory)
        {
            _ebookOptions = ebookOptions;

            var httpClient = new HttpClient(new ModifyingClientHttpHandler(async (request, cancellationToken) =>
            {
                var auth = request.Headers.Authorization;
                if (auth != null)
                {
                    var token = _accessTokensCacheManager.GetToken(_ebookOptions.ClientId);
                    if (token == null)
                    {
                        token = await GetToken();
                        _accessTokensCacheManager.AddOrUpdateToken(_ebookOptions.ClientId, token);
                    }

                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(auth.Scheme, token.AccessToken);
                }
            }))
            {
                BaseAddress = new Uri(ebookOptions.ApiBaseUrl),
                Timeout = TimeSpan.FromMinutes(5)
            };

            _instance = httpClientFactory.CreateApiInstance<IEbookApi>(httpClient);
        }

        private async Task<TokenResponse> GetToken()
        {
            var client = new HttpClient();

            return await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = _ebookOptions.IdentityProviderUrl + "/connect/token",
                ClientId = _ebookOptions.ClientId,
                ClientSecret = _ebookOptions.ClientSecret
            });
        }

        public Task GetOrdersAsync(DateTime startTimeStamp, DateTime endTimeStamp, int pageSize = 100)
        {
            return _instance.GetOrdersAsync();
        }

    }
}
