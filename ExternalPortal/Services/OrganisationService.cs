using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using ExternalPortal.Extensions;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Ofgem.API.GGSS.Domain.Commands.Organisations;

namespace ExternalPortal.Services
{
    public interface IOrganisationService
    {
        Task<string> AddOrganisationWithResponsiblePersonAsync(OrganisationSave organisationSave, CancellationToken cancellationToken = default);
    }

    public class OrganisationService : IOrganisationService
    {
        private readonly HttpClient _httpClient;

        public OrganisationService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

            var bearerTokenString = httpContextAccessor.HttpContext.User.GetBearerTokenString();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerTokenString);
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<string> AddOrganisationWithResponsiblePersonAsync(OrganisationSave organisationSave, CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.PostAsJsonAsync("/organisations/AddWithResponsiblePerson", organisationSave, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var id = await response.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<string>(id);
            }

            return null;
        }

    }
}
