using Microsoft.AspNetCore.Mvc;
using Rain.Clients;
using Rain.Model;

namespace Rain.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        public readonly IJsonPlaceholderApiClient _jsonPlaceholderApiClient;

        public UserController(IJsonPlaceholderApiClient jsonPlaceholderApiClient)
        {
            _jsonPlaceholderApiClient = jsonPlaceholderApiClient;
        }
        [HttpGet(Name = "GetUsers")]
        public async Task<IEnumerable<User>> GetUsers()
        {
            var result = await _jsonPlaceholderApiClient.GetUsersAsync();
            return result;
        }
      
    }
}
