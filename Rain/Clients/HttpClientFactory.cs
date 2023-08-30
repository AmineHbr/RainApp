using RestEase;

namespace Rain.Clients
{
    public class HttpClientFactory : IHttpClientFactory
    {
        public T CreateApiInstance<T>(HttpClient httpClient)
        {
            return RestClient.For<T>(httpClient);
        }
    }
}
