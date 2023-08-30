namespace Rain.Clients
{
    public interface IHttpClientFactory
    {
        public T CreateApiInstance<T>(HttpClient httpClient);
    }
}
