using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ExternalPortal.Services
{
    public interface IGetUserByProviderIdService
    {
        Task<GetUserResponse> Get(GetUserRequest request);
    }

    public class GetUserByProviderIdService : IGetUserByProviderIdService
    {
        private readonly HttpClient _client;

        public GetUserByProviderIdService(HttpClient client)
        {
            _client = client;
        }

        public async Task<GetUserResponse> Get(GetUserRequest request)
        {
            var response = new GetUserResponse()
            {
                Found = false
            };
            
            var result = await _client.GetAsync($"/users/find?ProviderId={request.ProviderId}");

            if (result.StatusCode == HttpStatusCode.OK)
            {
                var content = await result.Content.ReadAsStringAsync();
                var user = JsonConvert.DeserializeObject<GetUserResponse>(content);
                response.Found = true;
                response.UserId = user.UserId;
            }

            return response;
        }
    }

    public class GetUserRequest
    {
        public string ProviderId { get; set; }
    }

    public class GetUserResponse
    {
        public bool Found { get; set; }
        public string UserId { get; set; }
    }
}