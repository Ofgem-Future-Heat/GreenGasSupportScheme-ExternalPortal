using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using Xunit;

namespace ExternalPortal.Services.UnitTests
{
    public class GetCompaniesHouseServiceTests
    {
        private Mock<HttpMessageHandler> _handlerMock;

        public GetCompaniesHouseServiceTests()
        {
            _handlerMock = new Mock<HttpMessageHandler>();
        }

        [Fact]
        public async Task ShouldThrowExceptionIfRegisteredNumberIsMissing()
        {
            var httpClient = new HttpClient(_handlerMock.Object) { BaseAddress = new Uri("http://localhost:1234/") };

            var service = new GetCompaniesHouseService(httpClient);

            await Assert.ThrowsAsync<ArgumentNullException>(() => service.GetCompanyDetailsAsync(null, CancellationToken.None));
        }

        [Fact]
        public async Task ShouldReturnErrorIfNotSuccess()
        {
            var response = new HttpResponseMessage { StatusCode = HttpStatusCode.BadRequest };

            _handlerMock
                 .Protected()
                 .Setup<Task<HttpResponseMessage>>(
                 "SendAsync",
                 ItExpr.IsAny<HttpRequestMessage>(),
                 ItExpr.IsAny<CancellationToken>()
                 )
                .ReturnsAsync(response);

            var httpClient = new HttpClient(_handlerMock.Object) { BaseAddress = new Uri("http://localhost:1234/") };

            var service = new GetCompaniesHouseService(httpClient);

            var result = await service.GetCompanyDetailsAsync("123456789", CancellationToken.None);

            Assert.NotEmpty(result.Errors);
        }

        [Fact]
        public async Task ShouldReturnPopulatedModelFromResponse()
        {
            const string json = @"{
                          ""company_name"": ""SWISHFUND LTD"",
                          ""company_number"": ""11180668"",
                          ""registered_office_address"": {
                                        ""address_line_1"": ""2 Hazlewell Court"",
                            ""address_line_2"": ""Bar Road, Lolworth"",
                            ""care_of"": null,
                            ""country"": ""United Kingdom"",
                            ""locality"": ""Cambridge"",
                            ""po_box"": null,
                            ""postal_code"": ""AA1 1AA"",
                            ""premises"": null,
                            ""region"": ""Cambridgeshire""
                          }
                        }";

            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(json)
            };

            _handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(response);

            var httpClient = new HttpClient(_handlerMock.Object) { BaseAddress = new Uri("http://localhost:1234/") };

            var service = new GetCompaniesHouseService(httpClient);

            var result = await service.GetCompanyDetailsAsync("fake-company-house-number", CancellationToken.None);

            Assert.NotNull(result.Model);
            Assert.Equal("SWISHFUND LTD", result.Model.Value.Name);
            Assert.Equal("11180668", result.Model.Value.RegistrationNumber);
            Assert.Equal("2 Hazlewell Court", result.Model.Value.RegisteredOfficeAddress.LineOne);
            Assert.Equal("Bar Road, Lolworth", result.Model.Value.RegisteredOfficeAddress.LineTwo);
            Assert.Equal("Cambridge", result.Model.Value.RegisteredOfficeAddress.Town);
            Assert.Equal("Cambridgeshire", result.Model.Value.RegisteredOfficeAddress.County);
            Assert.Equal("AA1 1AA", result.Model.Value.RegisteredOfficeAddress.Postcode);
        }
    }
}
