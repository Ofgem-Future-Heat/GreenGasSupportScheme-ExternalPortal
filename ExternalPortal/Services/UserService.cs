using Newtonsoft.Json;
using Ofgem.API.GGSS.Domain.Models;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ExternalPortal.Services
{
    public class UserService// : IUserService
    {
        private readonly HttpClient _httpClient;

        public UserService(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<UserModel> GetSignedInUserAsync(CancellationToken cancellationToken = default)
        {
            var data = await _httpClient.GetStringAsync("/users");
            var user = JsonConvert.DeserializeObject<UserModel>(data);
            return await Task.FromResult(user);
        }

        public async Task<UserModel> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var data = await _httpClient.GetStringAsync($"/users/{userId}");
            var user = JsonConvert.DeserializeObject<UserModel>(data);
            return await Task.FromResult(user);
        }

        public async Task<UserModel> RegisterUserAsync(string displayName, string emailAddress, CancellationToken cancellationToken = default)
        {
            var payload = new { DisplayName = displayName, EmailAddress = emailAddress };
            var saveAsync = await _httpClient.PostAsync("/users", payload.ToHttpContent(), cancellationToken);
            var contentJson = await saveAsync.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<UserModel>(contentJson);
        }
    }
}
