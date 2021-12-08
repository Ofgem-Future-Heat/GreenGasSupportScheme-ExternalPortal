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
using Xunit;

namespace ExternalPortal.UnitTests.Services
{
    public class UpdateApplicationServiceTests
    {
        private readonly Mock<HttpMessageHandler> _handlerMock;

        public UpdateApplicationServiceTests()
        {
            _handlerMock = new Mock<HttpMessageHandler>();
        }


        [Fact]
        public async Task ShouldReturnNoErrorsIfServiceReturnsOk()
        {

            var expectedResponse = new
            {
                Errors = new List<string>()
            };
            
            SetHandler(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(expectedResponse)),
            });

            var httpClient = new HttpClient(_handlerMock.Object) { BaseAddress = new Uri("http://localhost:1234/") };

            var result = await new UpdateApplicationService(httpClient).Update(new UpdateApplicationRequest(), CancellationToken.None);

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

            var result = await new UpdateApplicationService(httpClient).Update(new UpdateApplicationRequest(), CancellationToken.None);

            result.Errors.Should().Contain("COULD_NOT_UPDATE_APPLICATION");
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
