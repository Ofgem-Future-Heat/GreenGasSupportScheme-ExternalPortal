using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Ofgem.API.GGSS.Domain.Models;
using Ofgem.API.GGSS.Domain.ModelValues;

namespace ExternalPortal.Services
{
    public interface IGetOrganisationDetailsService
    {
        Task<GetOrganisationDetailsResponse> Get(GetOrganisationDetailsRequest getOrganisationDetailsRequest,
            CancellationToken cancellationToken);
    }
    
    public class GetOrganisationDetailsService : IGetOrganisationDetailsService
    {
        private readonly HttpClient _client;
        
        public GetOrganisationDetailsService(HttpClient client)
        {
            _client = client;
            _client.DefaultRequestHeaders.Authorization
                = new AuthenticationHeaderValue("Bearer", @"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpZCI6IjMyNmZhOTc0LTdjMDUtNGIzNy1hOGU0LTZkNWZlNmRlYjYzYiIsIm5iZiI6MTYyNzQ4MDk1MywiZXhwIjoxNjU5MDE2OTUzLCJpYXQiOjE2Mjc0ODA5NTN9.pYB3Bi65_NFCulqYdbJdhUraONb4lH__Gs9YoZbiLZM"); 
        }
        
        public async Task<GetOrganisationDetailsResponse> Get(GetOrganisationDetailsRequest getOrganisationDetailsRequest, CancellationToken cancellationToken)
        {
            var response = new GetOrganisationDetailsResponse();
            
            var serviceResponse = await _client.GetAsync($"Organisation/{getOrganisationDetailsRequest.OrganisationId}/details");

            if (!serviceResponse.IsSuccessStatusCode)
            {
                response.Errors.Add("ORGANISATION_DETAILS_NOT_FOUND");
                return response;
            }

            var content = await serviceResponse.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<GetOrganisationDetailsResponse>(content);
        }
    }

    public class GetOrganisationDetailsResponse
    {
        public List<string> Errors { get; set; } = new List<string>();
        public string OrganisationType { get; set; }
        public string OrganisationName { get; set; }
        public AddressModel OrganisationAddress { get; set; }
        public string ResponsiblePersonName { get; set; }
        public string ResponsiblePersonEmail { get; set; }
        public DocumentValue PhotoId { get; set; }
        public DocumentValue ProofOfAddress { get; set; }
        public DocumentValue LetterOfAuthority { get; set; }
        public List<UserModel> OrganisationUsers { get; set; }

    }

    public class GetOrganisationDetailsRequest
    {
        public string OrganisationId { get; set; }
    }
}