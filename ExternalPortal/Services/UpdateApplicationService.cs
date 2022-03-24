using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Ofgem.API.GGSS.Domain.ModelValues;

namespace ExternalPortal.Services
{
    public interface IUpdateApplicationService
    {
        Task<UpdateApplicationResponse> Update(UpdateApplicationRequest request, CancellationToken token);
    }

    public class UpdateApplicationService : IUpdateApplicationService
    {
        private readonly HttpClient _client;

        public UpdateApplicationService(HttpClient client)
        {
            _client = client;
        }

        public async Task<UpdateApplicationResponse> Update(UpdateApplicationRequest request, CancellationToken token)
        {
            var response = new UpdateApplicationResponse();

            var data = new
            {
                Application = request.Application,

                UserId = request.UserId
            };
            
            var serviceResponse = await _client.PutAsJsonAsync($"/Application/{request.ApplicationId}", data, token);

            if (!serviceResponse.IsSuccessStatusCode)
            {
                response.Errors.Add("COULD_NOT_UPDATE_APPLICATION");
            }
            
            return response;
        }
    }
    
    public class UpdateApplicationRequest
    {
        public string ApplicationId { get; set; }
        public ApplicationValue Application { get; set; }
        
        public string UserId { get; set; }
    }
    
    public class UpdateApplicationResponse
    {
        public List<string> Errors { get; set; } = new List<string>();
    }
}