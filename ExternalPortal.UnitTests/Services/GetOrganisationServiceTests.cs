using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ExternalPortal.Services;
using FluentAssertions;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using Ofgem.API.GGSS.Domain.ModelValues;
using Ofgem.API.GGSS.Domain.ModelValues.StageOne;
using Xunit;

namespace ExternalPortal.UnitTests.Services
{
    public class GetOrganisationServiceTests
    {
        private readonly Mock<HttpMessageHandler> _handlerMock;

        public GetOrganisationServiceTests()
        {
            _handlerMock = new Mock<HttpMessageHandler>();
        }
        
        [Fact]
        public async Task ShouldReturnApplicationDataIfServiceReturnsOk()
        {
            var expectedResponse = new
            {
                Applications = new List<ApplicationValue>
                {
                    new ApplicationValue(), 
                    new ApplicationValue(), 
                    new ApplicationValue()
                }
            };
            
            SetHandler(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(expectedResponse)),
            });
        
            var httpClient = new HttpClient(_handlerMock.Object) { BaseAddress = new Uri("http://localhost:1234/") };
        
            var result = await new GetOrganisationService(httpClient).Get(new GetOrganisationRequest(), CancellationToken.None);

            result.Applications.Should().NotBeNullOrEmpty();
            
        }
        
        private void SetHandler(HttpResponseMessage response)
        {
            _handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(response);
        }
    }
}