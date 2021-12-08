using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ExternalPortal.Services;
using Moq;
using Moq.Protected;
using Xunit;

namespace ExternalPortal.UnitTests.Services
{
    public class GetDocumentServiceTests
    {
        private readonly Mock<HttpMessageHandler> _handlerMock;
        private readonly HttpClient _httpClient;

        public GetDocumentServiceTests()
        {
            _handlerMock = new Mock<HttpMessageHandler>();

            _httpClient = new HttpClient(_handlerMock.Object) { BaseAddress = new Uri("http://localhost:1234/") };

            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new ByteArrayContent(new StreamContent(GetStream()).ReadAsByteArrayAsync().Result)
            };

            SetHandler(response);
        }

        [Fact]
        public async Task ShouldReturnExceptionWhenDocumentIdIsMissing()
        {
            Task result() => new GetDocumentService(_httpClient).Get(null, CancellationToken.None);

            await Assert.ThrowsAsync<ArgumentNullException>(result);
        }

        [Fact]
        public async Task ShouldReturnGetResponseWithErrors()
        {
            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest
            };

            SetHandler(response);

            var result = await new GetDocumentService(_httpClient).Get("123456", CancellationToken.None);

            Assert.NotEmpty(result.Errors);
            Assert.Equal("DOCUMENT_NOT_FOUND", result.Errors.First());
        }

        [Fact]
        public async Task ShouldReturnNoErrorsIfServiceReturnsOk()
        {
            var result = await new GetDocumentService(_httpClient).Get("123456", CancellationToken.None);

            Assert.Empty(result.Errors);
            Assert.NotEmpty(result.Contents);
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

        private static Stream GetStream()
        {
            var content = "Hello World from a Fake File";
            var ms = new MemoryStream();
            var writer = new StreamWriter(ms);
            writer.Write(content);
            writer.Flush();
            ms.Position = 0;

            return ms;
        }
    }
}
