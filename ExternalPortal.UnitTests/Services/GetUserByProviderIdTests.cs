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
    public class GetUserByProviderIdTests
    {
        private Mock<HttpMessageHandler> _handlerMock;
        
        [Fact]
        public async Task SetsFoundToFalseWhenUserNotFoundInApi()
        {
            _handlerMock = new Mock<HttpMessageHandler>();
            var httpClient = new HttpClient(_handlerMock.Object) { BaseAddress = new Uri("http://localhost:1234/") };
            
            SetHandler(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound,
            });

            var service = new GetUserByProviderIdService(httpClient);

            var response = await service.Get(new GetUserRequest
            {
                ProviderId = "some id we haven't seen before"
            });

            response.Found.Should().BeFalse();
        }
        
        [Fact]
        public async Task SetsFoundToTrueWhenUserIsFoundInApi()
        {
            _handlerMock = new Mock<HttpMessageHandler>();
            
            var httpClient = new HttpClient(_handlerMock.Object) { BaseAddress = new Uri("http://localhost:1234/") };
            
            var expectedResponse = new
            {
                userId = Guid.NewGuid().ToString(),
            };
            
            SetHandler(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(expectedResponse)),
            });

            var service = new GetUserByProviderIdService(httpClient);

            var response = await service.Get(new GetUserRequest
            {
                ProviderId = "some id we know about"
            });

            response.Found.Should().BeTrue();
        }
        
        [Fact]
        public async Task ReturnsUserIdWhenUserIsFoundInApi()
        {
            _handlerMock = new Mock<HttpMessageHandler>();
            
            var httpClient = new HttpClient(_handlerMock.Object) { BaseAddress = new Uri("http://localhost:1234/") };

            var expectedResponse = new
            {
                userId = Guid.NewGuid().ToString()
            };
            
            SetHandler(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(expectedResponse)),
            });

            var service = new GetUserByProviderIdService(httpClient);

            var response = await service.Get(new GetUserRequest
            {
                ProviderId = "some id we know about",
            });

            response.UserId.Should().Be(expectedResponse.userId);
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