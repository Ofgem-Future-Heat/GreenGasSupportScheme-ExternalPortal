using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Ofgem.API.GGSS.Domain.ModelValues;
using Ofgem.API.GGSS.Domain.ModelValues.StageOne;

namespace ExternalPortal.Services
{
    public interface IGetApplicationService
    {
        Task<GetApplicationResponse> Get(GetApplicationRequest getApplicationRequest, CancellationToken none);
    }

    public class GetApplicationService : IGetApplicationService
    {
        private readonly HttpClient _client;

        public GetApplicationService(HttpClient client)
        {
            _client = client;
            _client.DefaultRequestHeaders.Authorization
                = new AuthenticationHeaderValue("Bearer", @"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpZCI6IjMyNmZhOTc0LTdjMDUtNGIzNy1hOGU0LTZkNWZlNmRlYjYzYiIsIm5iZiI6MTYyNzQ4MDk1MywiZXhwIjoxNjU5MDE2OTUzLCJpYXQiOjE2Mjc0ODA5NTN9.pYB3Bi65_NFCulqYdbJdhUraONb4lH__Gs9YoZbiLZM");
        }

        public async Task<GetApplicationResponse> Get(GetApplicationRequest getApplicationRequest, CancellationToken none)
        {
            var response = new GetApplicationResponse();
            
            var serviceResponse = await _client.GetAsync($"/Application/{getApplicationRequest.ApplicationId}");

            if (!serviceResponse.IsSuccessStatusCode)
            {
                response.Errors.Add("COULD_NOT_RETRIEVE_APPLICATION");
                return response;
            }
            
            var content = await serviceResponse.Content.ReadAsStringAsync();
            
            return JsonConvert.DeserializeObject<GetApplicationResponse>(content);
        }
    }

    public class GetApplicationResponse
    {
        public ApplicationValue Application { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }

    public class GetApplicationRequest
    {
        public string ApplicationId { get; set; }
    }
}