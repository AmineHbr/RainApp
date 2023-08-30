namespace Rain.Clients
{
    public interface IEbookApiClient
    {
        Task GetOrdersAsync(DateTime startTimeStamp, DateTime endTimeStamp, int pageSize = 100);
    }
}
