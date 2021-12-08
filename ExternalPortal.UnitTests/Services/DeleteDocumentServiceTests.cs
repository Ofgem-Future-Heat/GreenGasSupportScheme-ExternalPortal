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
using Xunit;

namespace ExternalPortal.UnitTests.Services
{
    public class DeleteDocumentServiceTests
    {
        private readonly Mock<HttpMessageHandler> _handlerMock;

        public DeleteDocumentServiceTests()
        {
            _handlerMock = new Mock<HttpMessageHandler>();
        }

        [Fact]
        public async Task ShouldReturnExceptionWhenDocumentIdIsMissing()
        {
            var httpClient = new HttpClient(_handlerMock.Object) { BaseAddress = new Uri("http://localhost:1234/") };

            Task result() => new DeleteDocumentService(httpClient).Delete(null, CancellationToken.None);

            await Assert.ThrowsAsync<ArgumentNullException>(result);
        }

        [Fact]
        public async Task ShouldReturnNoErrorsIfServiceReturnsOk()
        {
            SetHandler(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("")
            });

            var httpClient = new HttpClient(_handlerMock.Object) { BaseAddress = new Uri("http://localhost:1234/") };

            var result = await new DeleteDocumentService(httpClient).Delete("123", CancellationToken.None);
            
            result.Errors.Should().BeNullOrEmpty();
        }
        
        [Fact]
        public async Task ShouldReturnErrorIfServiceReturnsBadRequest()
        {
            SetHandler(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest
            });

            var httpClient = new HttpClient(_handlerMock.Object) { BaseAddress = new Uri("http://localhost:1234/") };

            var result = await new DeleteDocumentService(httpClient).Delete("123", CancellationToken.None);

            result.Errors.Should().Contain("DOCUMENT_NOT_DELETED");
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
