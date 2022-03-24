using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using ExternalPortal.Extensions;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace ExternalPortal.Services
{
    public interface IInviteUserToOrganisationService
    {
        Task<InviteUserToOrganisationResponse> Invite(InviteUserToOrganisationRequest request,
            CancellationToken cancellationToken);
    }
    
    public class InviteUserToOrganisationService : IInviteUserToOrganisationService
    {
        private readonly HttpClient _client;

        public InviteUserToOrganisationService(HttpClient client)
        {
            _client = client;
        }
        public async Task<InviteUserToOrganisationResponse> Invite(InviteUserToOrganisationRequest request, CancellationToken cancellationToken)
        {
            var response = new InviteUserToOrganisationResponse();
            
            var serviceResponse = await _client.PostAsJsonAsync("/users/invite", request);

            if (!serviceResponse.IsSuccessStatusCode)
            {
                response.Errors.Add("INVITATION_RESULT_NOT_FOUND");
                return response;
            }

            var content = await serviceResponse.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<InviteUserToOrganisationResponse>(content);
        }
    }
    
    public class InviteUserToOrganisationRequest
    {
        public string UserEmail { get; set; }
        public string OrganisationId { get; set; }
    }
    public class InviteUserToOrganisationResponse
    {
        public string InvitationResult { get; set; }
        public string InvitationId { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }
}
