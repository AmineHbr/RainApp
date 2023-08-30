using RestEase;

namespace Rain.Clients
{
    public interface IEbookApi
    {
        [Get("issuebook/deals/v3/orders")]
        [Header("Authorization", "Bearer")]
        Task GetOrdersAsync();
        [Get("{url}")]
        [Header("Authorization", "Bearer")]
        Task GetPrdersByUrlAsync(string url);
    }
}
