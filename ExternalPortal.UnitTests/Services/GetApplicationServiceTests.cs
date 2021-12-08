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
    public class GetApplicationServiceTests
    {
        private readonly Mock<HttpMessageHandler> _handlerMock;

        public GetApplicationServiceTests()
        {
            _handlerMock = new Mock<HttpMessageHandler>();
        }

        [Fact]
        public async Task ShouldReturnApplicationDataIfServiceReturnsOk()
        {
            var expectedResponse = new
            {
                Application = new ApplicationValue
                {
                    StageOne = new StageOneValue
                    {
                        TellUsAboutYourSite = new TellUsAboutYourSiteValue
                        {
                            PlantName = "my plant"
                        }
                    }
                }
            };
            
            SetHandler(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(expectedResponse)),
            });
        
            var httpClient = new HttpClient(_handlerMock.Object) { BaseAddress = new Uri("http://localhost:1234/") };
        
            var result = await new GetApplicationService(httpClient).Get(new GetApplicationRequest(), CancellationToken.None);

            result.Application.StageOne.TellUsAboutYourSite.PlantName.Should()
                .Be(expectedResponse.Application.StageOne.TellUsAboutYourSite.PlantName);
        }

        [Fact]
        public async Task ShouldReturnNoErrorsIfServiceReturnsOk()
        {
            var expectedResponse = new
            {
                Application = new ApplicationValue(),
                Errors = new List<string>(),
            };
            
            SetHandler(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(expectedResponse)),
            });

            var httpClient = new HttpClient(_handlerMock.Object) { BaseAddress = new Uri("http://localhost:1234/") };

            var result = await new GetApplicationService(httpClient).Get(new GetApplicationRequest(), CancellationToken.None);

            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public async Task ShouldReturnErrorIfServiceReturnsBadRequest()
        {
            
            var expectedResponse = new
            {
                Application = new ApplicationValue(),
                Errors = new List<string>(),
            };
            
            SetHandler(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest,
            });
        
            var httpClient = new HttpClient(_handlerMock.Object) { BaseAddress = new Uri("http://localhost:1234/") };
        
            var result = await new GetApplicationService(httpClient).Get(new GetApplicationRequest(), CancellationToken.None);
        
            result.Errors.Should().Contain("COULD_NOT_RETRIEVE_APPLICATION");
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
