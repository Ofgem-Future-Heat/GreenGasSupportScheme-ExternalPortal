using Newtonsoft.Json;
using Ofgem.API.GGSS.Domain.Commands.Applications;
using Ofgem.API.GGSS.Domain.Contracts.Services;
using Ofgem.API.GGSS.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http.Headers;

namespace ExternalPortal.Services
{
    public class ApplicationService : IApplicationService
    {
        private readonly HttpClient _httpClient;

        public ApplicationService(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<IEnumerable<ApplicationModel>> GetAsync(CancellationToken token = default, bool includeDocuments = true)
        {
            var dataAsync = await _httpClient.GetAsync($"/applications?includeDocuments={includeDocuments}")
                .ContinueWith(async responseAsync =>
                {
                    var response = await responseAsync;
                    response.EnsureSuccessStatusCode();
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<IEnumerable<ApplicationModel>>(json);
                },
                TaskContinuationOptions.OnlyOnRanToCompletion);
            return await dataAsync;
        }

        public async Task<ApplicationModel> GetAsync(string aplicationId, CancellationToken cancellationToken = default)
        {
            var data = await this.GetAsync(cancellationToken);

            return await Task.FromResult(data.FirstOrDefault(a => a.Id == aplicationId));
        }

        public async Task<string> SaveStageOneAsync(StageOne stageOne, CancellationToken cancellationToken = default)
        {
            var saveAsync = await _httpClient.PostAsync("/applications/stageone", stageOne.ToHttpContent())
                .ContinueWith(async responseAsync =>
                {
                    var response = await responseAsync;

                    try
                    {
                        response.EnsureSuccessStatusCode();
                    }
                    catch(Exception ex)
                    {
                        throw new HttpRequestException(await response.Content.ReadAsStringAsync(), ex);
                    }
                    
                    var id = await response.Content.ReadAsStringAsync();

                    return JsonConvert.DeserializeObject<string>(id);
                },
                TaskContinuationOptions.OnlyOnRanToCompletion);
            return await saveAsync;
        }
    }
}
