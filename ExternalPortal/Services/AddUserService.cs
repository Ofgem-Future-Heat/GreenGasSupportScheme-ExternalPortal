using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ExternalPortal.Services
{
    public interface IAddUserService
    {
        public Task<AddUserResponse> Add(AddUserRequest request, CancellationToken cancellationToken);
    }

    public class AddUserService : IAddUserService
    {
        private readonly HttpClient _client;

        public AddUserService(HttpClient client)
        {
            _client = client;
        }

        public async Task<AddUserResponse> Add(AddUserRequest request, CancellationToken cancellationToken)
        {
            var serviceResponse = await _client.PostAsJsonAsync("/users", request, cancellationToken);
            var content = await serviceResponse.Content.ReadAsStringAsync();
            var response = JsonConvert.DeserializeObject<AddUserResponse>(content);

            return response;
        }
    }

    public class AddUserRequest
    {
        public string ProviderId { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }
    }

    public class AddUserResponse
    {
        public string UserId { get; set; }
    }
}