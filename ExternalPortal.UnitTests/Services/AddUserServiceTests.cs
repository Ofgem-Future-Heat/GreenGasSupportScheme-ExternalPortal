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
    public class AddUserServiceTests
    {
        private Mock<HttpMessageHandler> _handlerMock;
        
        [Fact]
        public async Task ReturnsCreatedUsersIdFromApi()
        {
            _handlerMock = new Mock<HttpMessageHandler>();
            
            var expectedResponse = new AddUserResponse()
            {
                UserId = Guid.NewGuid().ToString()
            };
            
            SetHandler(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(expectedResponse)),
            });

            var httpClient = new HttpClient(_handlerMock.Object) { BaseAddress = new Uri("http://localhost:1234/") };

            var service = new AddUserService(httpClient);

            var response = await service.Add(new AddUserRequest()
            {
                Email = "bob@bob.com",
                Name = "bob",
                Surname = "Bob",
                ProviderId = "someId"
            }, CancellationToken.None);

            response.UserId.Should().Be(expectedResponse.UserId);
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