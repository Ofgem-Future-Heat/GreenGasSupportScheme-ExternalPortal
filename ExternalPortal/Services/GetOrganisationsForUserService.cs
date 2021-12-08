using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Ofgem.API.GGSS.Domain.ModelValues;

namespace ExternalPortal.Services
{
    public interface IGetOrganisationsForUserService
    {
        public Task<GetOrganisationsForUserResponse> Get(
            GetOrganisationsForUserRequest getOrganisationsForUserRequest,
            CancellationToken none);
    }

    public class GetOrganisationsForUserService : IGetOrganisationsForUserService
    {
        private readonly HttpClient _httpClient;

        public GetOrganisationsForUserService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<GetOrganisationsForUserResponse> Get(GetOrganisationsForUserRequest getOrganisationsForUserRequest,
            CancellationToken none)
        {
            var serviceResponse = await _httpClient.GetAsync($"/users/{getOrganisationsForUserRequest.UserId}/organisations");

            if (!serviceResponse.IsSuccessStatusCode)
            {
                return new GetOrganisationsForUserResponse();
            }
            var content = await serviceResponse.Content.ReadAsStringAsync();
            
            return JsonConvert.DeserializeObject<GetOrganisationsForUserResponse>(content);
        }
    }

    public class GetOrganisationsForUserResponse
    {
        public List<GetOrganisationForUserResponse> Organisations { get; set; } =
            new List<GetOrganisationForUserResponse>();
    }

    public class GetOrganisationForUserResponse
    {
        public string OrganisationName { get; set; }
        public string OrganisationId { get; set; }
        public int NumberOfApplications { get; set; }
    }

    public class GetOrganisationsForUserRequest
    {
        public string UserId { get; set; }
    }
}