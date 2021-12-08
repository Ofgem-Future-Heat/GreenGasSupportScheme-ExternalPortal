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
using Ofgem.API.GGSS.Application.Entities;
using Xunit;

namespace ExternalPortal.UnitTests.Services
{
    public class GetOrganisationsForUserServiceTests
    {
        private readonly Mock<HttpMessageHandler> _handlerMock;

        public GetOrganisationsForUserServiceTests()
        {
            _handlerMock = new Mock<HttpMessageHandler>();
        }
        
        [Fact]
        public async Task FetchesOrganisationsFromApi()
        {
            var expectedResponse = new
            {
                Organisations = new List<object>
                {
                    new
                    {
                        OrganisationId = Guid.NewGuid().ToString(),
                        OrganisationName = "organisation name"
                    }
                }
            };
            
            SetHandler(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(expectedResponse)),
            });
        
            var httpClient = new HttpClient(_handlerMock.Object) { BaseAddress = new Uri("http://localhost:1234/") };
            var service = new GetOrganisationsForUserService(httpClient);
            var result = await service.Get(new GetOrganisationsForUserRequest()
            {
                UserId = "userId"
            }, CancellationToken.None);
            
            result.Organisations.Should().Contain(o => o.OrganisationName == "organisation name");
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