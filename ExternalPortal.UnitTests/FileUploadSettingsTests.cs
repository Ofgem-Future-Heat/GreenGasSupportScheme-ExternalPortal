using ExternalPortal.Configuration;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace ExternalPortal.UnitTests
{
    public class ApplicationSettingsTests
    {
        private readonly ServiceConfig _config;

        public ApplicationSettingsTests()
        {
            _config = new ServiceConfig()
            {
                Api = new ApiConfig
                {
                    DocumentsApiBaseUri = "document-service-base-url-with-port-number"
                }
            };
        }

        [Fact]
        public void ShouldReturnStronglyTypedDocumentServiceBaseUrl()
        {
            _config.Api.DocumentsApiBaseUri.Should().BeOfType(typeof(string));
            _config.Api.DocumentsApiBaseUri.Should().Equals("document-service-base-url-with-port-number");
        }
    }
}
