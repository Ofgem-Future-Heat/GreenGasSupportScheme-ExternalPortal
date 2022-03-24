using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Ofgem.API.GGSS.Domain.ModelValues;

namespace ExternalPortal.Services
{
    public interface IGetOrganisationService
    {
        Task<GetOrganisationResponse> Get(GetOrganisationRequest getOrganisationRequest, CancellationToken none);
    }
    
    public class GetOrganisationService : IGetOrganisationService
    {
        private readonly HttpClient _client;

        public GetOrganisationService(HttpClient client)
        {
            _client = client; 
        }
        
        public async Task<GetOrganisationResponse> Get(GetOrganisationRequest getOrganisationRequest, CancellationToken none)
        {
            var response = new GetOrganisationResponse();
            
            var serviceResponse = await _client.GetAsync($"/Organisation/{getOrganisationRequest.OrganisationId}");

            if (!serviceResponse.IsSuccessStatusCode)
            {
                response.Errors.Add("COULD_NOT_RETRIEVE_ORGANISATIONS");
                return response;
            }
            
            var content = await serviceResponse.Content.ReadAsStringAsync();
            
            return JsonConvert.DeserializeObject<GetOrganisationResponse>(content);
        }
        
    }
    
    public class GetOrganisationResponse
    {
        public string Name { get; set; }
        public List<RetrieveOrganisationApplicationResponse> Applications { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }
    
    public class RetrieveOrganisationApplicationResponse
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
    }

    public class GetOrganisationRequest
    {
        public string OrganisationId { get; set; }
    }
}