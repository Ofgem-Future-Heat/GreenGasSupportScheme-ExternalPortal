using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ExternalPortal.Services;
using FluentAssertions;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using Xunit;

namespace ExternalPortal.UnitTests.Services
{
    public class CreateApplicationServiceTests
    {
        private readonly Mock<HttpMessageHandler> _handlerMock;

        public CreateApplicationServiceTests()
        {
            _handlerMock = new Mock<HttpMessageHandler>();
        }

        [Fact]
        public async Task ShouldReturnErrorsIfResultCannotBeDeserialized()
        {
            SetHandler(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("")
            });

            var httpClient = new HttpClient(_handlerMock.Object) { BaseAddress = new Uri("http://localhost:1234/") };

            var result = await new CreateApplicationService(httpClient).Create(new CreateApplicationRequest(), CancellationToken.None);

            result.Errors.Should().Contain("FAILED_TO_DESERIALIZE_RESPONSE");
        }

        [Fact]
        public async Task ShouldReturnApplicationIdIfServiceReturnsOk()
        {

            var expectedResponse = new
            {
                NewApplicationId = "someId",
            };
            
            SetHandler(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(expectedResponse)),
            });

            var httpClient = new HttpClient(_handlerMock.Object) { BaseAddress = new Uri("http://localhost:1234/") };

            var result = await new CreateApplicationService(httpClient).Create(new CreateApplicationRequest(), CancellationToken.None);

            result.ApplicationId.Should().Be("someId");
        }
        
        [Fact]
        public async Task ShouldReturnNoErrorsIfServiceReturnsOk()
        {

            var expectedResponse = new
            {
                NewApplicationId = "someId",
            };
            
            SetHandler(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(expectedResponse)),
            });

            var httpClient = new HttpClient(_handlerMock.Object) { BaseAddress = new Uri("http://localhost:1234/") };

            var result = await new CreateApplicationService(httpClient).Create(new CreateApplicationRequest(), CancellationToken.None);

            result.Errors.Should().BeEmpty();
        }
        
        [Fact]
        public async Task ShouldReturnErrorIfServiceReturnsBadRequest()
        {
            SetHandler(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest
            });

            var httpClient = new HttpClient(_handlerMock.Object) { BaseAddress = new Uri("http://localhost:1234/") };

            var result = await new CreateApplicationService(httpClient).Create(new CreateApplicationRequest(), CancellationToken.None);

            result.Errors.Should().Contain("COULD_NOT_CREATE_NEW_APPLICATION");
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
