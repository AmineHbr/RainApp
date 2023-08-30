using Rain.Model;

namespace Rain.Clients
{
    public interface IJsonPlaceholderApiClient
    {
        Task<List<User>> GetUsersAsync();
    }
}
