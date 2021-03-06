using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Ofgem.API.GGSS.Domain.Responses.Applications;

namespace ExternalPortal.Services
{
    public interface ICreateApplicationService
    {
         Task<CreateApplicationResponse> Create(CreateApplicationRequest request, CancellationToken cancellationToken);
    }

    public class CreateApplicationService : ICreateApplicationService
    {
        private readonly HttpClient _client;

        public CreateApplicationService(HttpClient client)
        {
            _client = client;
        }

        public async Task<CreateApplicationResponse> Create(CreateApplicationRequest request, CancellationToken cancellationToken)
        {
            var response = new CreateApplicationResponse();
            var serviceResponse = await _client.PostAsJsonAsync("/Application", request, cancellationToken);

            if (!serviceResponse.IsSuccessStatusCode)
            {
                response.Errors.Add("COULD_NOT_CREATE_NEW_APPLICATION");
                return response;
            }

            var result = JsonConvert.DeserializeObject<CreateNewApplicationResponse>(await serviceResponse.Content.ReadAsStringAsync());

            if (result == null)
            {
                response.Errors.Add("FAILED_TO_DESERIALIZE_RESPONSE");
                return response;
            }
            
            response.ApplicationId = result.NewApplicationId;
            return response;
        }
    }

    public class CreateApplicationRequest
    {
        public string OrganisationId { get; set; }
        
        public string UserId { get; set; }
    }
    
    public class CreateApplicationResponse
    {
        public List<string> Errors { get; set; } = new List<string>();
        public string ApplicationId { get; set; }
    }
}