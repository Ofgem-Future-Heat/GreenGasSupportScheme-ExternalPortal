using System.Net.Http;
using ExternalPortal.Configuration;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ExternalPortal.UnitTests.Controllers
{
    public class BaseFileUploadControllerTests
    {
        public readonly HttpClient Client;
        public readonly IOptions<ServiceConfig> ServiceOptions;

        public BaseFileUploadControllerTests(WebApplicationFactory<Startup> fixture)
        {
            Client = fixture.CreateClient();

            ServiceOptions = Options.Create<ServiceConfig>(new ServiceConfig()
            {
                KeyVaultUri = "some-fake-kv-uri",
                Api = new ApiConfig
                {
                    RetryCount = 1,
                    RetryIntervalSeconds = 2,
                    InternalApiBaseUri = "https://localhost:44313",
                    CompaniesHouseApiBaseUri = "https://localhost:44313",
                    DocumentsApiBaseUri = "document-service-base-url-with-port-number"
                }
            });
        }
    }
}
