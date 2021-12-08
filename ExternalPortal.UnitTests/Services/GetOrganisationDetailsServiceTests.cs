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
using Ofgem.API.GGSS.Domain.Models;
using Ofgem.API.GGSS.Domain.ModelValues;
using Xunit;

namespace ExternalPortal.UnitTests.Services
{
    public class GetOrganisationDetailsServiceTests
    {
        private readonly Mock<HttpMessageHandler> _handlerMock;

        public GetOrganisationDetailsServiceTests()
        {
            _handlerMock = new Mock<HttpMessageHandler>();
        }
        
        [Fact]
        public async Task GetOrganisationDetailsIfServiceReturnsOk()
        {
            var expectedResponse = new GetOrganisationDetailsResponse()
            {
                OrganisationType = "Private",
                OrganisationName = "Org Name",
                OrganisationAddress = new AddressModel()
                {
                    Name = "Name",
                    LineOne = "Line One",
                    LineTwo = "Line Two",
                    Town = "Town",
                    County = "County",
                    Postcode = "AB1 1BA"
                },
                ResponsiblePersonName = "Responsible person name",
                ResponsiblePersonEmail = "Responsible person email",
                PhotoId = new DocumentValue()
                {
                    FileName = "PhotoId",
                    FileSizeAsString = "20KB",
                    Reference = "Photo Id"
                },
                ProofOfAddress = new DocumentValue()
                {
                    FileName = "POA",
                    FileSizeAsString = "25KB",
                    Reference = "POA"
                },
                LetterOfAuthority = new DocumentValue()
                {
                    FileName = "LOA",
                    FileSizeAsString = "20KB",
                    Reference = "LOA"
                }
            };
            
            SetHandler(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(expectedResponse)),
            });
            
            var httpClient = new HttpClient(_handlerMock.Object) { BaseAddress = new Uri("http://localhost:1234/") };

            var result =
                await new GetOrganisationDetailsService(httpClient).Get(new GetOrganisationDetailsRequest(),
                    CancellationToken.None);
            
            result.OrganisationName.Should().Be(expectedResponse.OrganisationName);
            result.OrganisationType.Should().Be(expectedResponse.OrganisationType);
            result.OrganisationAddress.Should().Equals(expectedResponse.OrganisationAddress);
            result.ResponsiblePersonName.Should().Be(expectedResponse.ResponsiblePersonName);
            result.ResponsiblePersonEmail.Should().Be(expectedResponse.ResponsiblePersonEmail);
            result.PhotoId.Should().Equals(expectedResponse.PhotoId);
            result.ProofOfAddress.Should().Equals(expectedResponse.ProofOfAddress);
            result.LetterOfAuthority.Should().Equals(expectedResponse.LetterOfAuthority);
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