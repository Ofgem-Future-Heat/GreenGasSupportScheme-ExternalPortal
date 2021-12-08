using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ExternalPortal.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Moq.Protected;
using Xunit;

namespace ExternalPortal.UnitTests.Services
{
    public class SaveDocumentServiceTests
    {
        private readonly Mock<HttpMessageHandler> _handlerMock;
        private readonly FormFile _dummyFile;

        public SaveDocumentServiceTests()
        {
            _handlerMock = new Mock<HttpMessageHandler>();
            _dummyFile = new FormFile(GetStream(), 0, 1024, "dummy-stream", "dummy-stream.pdf");
        }

        [Fact]
        public async Task ShouldReturnNullFileErrorWhenFileIsNull()
        {
            var httpClient = new HttpClient(_handlerMock.Object) { BaseAddress = new Uri("http://localhost:1234/") };

            var result = await new SaveDocumentService(httpClient).Save(null, CancellationToken.None);

            var expected = new SaveDocumentError("NULL_FILE_VALIDATION_ERROR", "Upload a file");

            result.DocumentId.Should().BeNullOrEmpty();
            result.Errors.Should().ContainEquivalentOf(expected);
        }
        
        [Fact]
        public async Task ShouldReturnErrorIfDocumentIdIsEmpty()
        {
            SetHandler(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"documentId\":\"\"}")
            });

            var httpClient = new HttpClient(_handlerMock.Object) { BaseAddress = new Uri("http://localhost:1234/") };

            var result = await new SaveDocumentService(httpClient).Save(_dummyFile, CancellationToken.None);

            var expected = new SaveDocumentError("FILE_NOT_SAVED", "Document Id was not returned");

            result.DocumentId.Should().BeNullOrEmpty();
            result.Errors.Should().ContainEquivalentOf(expected);
        }

        [Fact]
        public async Task ShouldReturnModelWithoutErrorIfServiceReturnsOk()
        {
            SetHandler(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"documentId\":\"fake-file-document-id\"}")
            });

            var httpClient = new HttpClient(_handlerMock.Object) { BaseAddress = new Uri("http://localhost:1234/") };

            var result = await new SaveDocumentService(httpClient).Save(_dummyFile, CancellationToken.None);

            result.DocumentId.Should().Be("fake-file-document-id");
            result.Errors.Should().BeNullOrEmpty();
        }

        [Fact]
        public async Task ShouldReturnModelWithErrorIfServiceReturnsBadRequest()
        {
            const string errorMessage = "File could not be saved";

            SetHandler(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = new StringContent(errorMessage)
            });

            var httpClient = new HttpClient(_handlerMock.Object) { BaseAddress = new Uri("http://localhost:1234/") };

            var result = await new SaveDocumentService(httpClient).Save(_dummyFile, CancellationToken.None);

            var expected = new SaveDocumentError("FILE_NOT_SAVED", "File not saved successfully");

            result.DocumentId.Should().BeNullOrEmpty();
            result.Errors.Should().ContainEquivalentOf(expected);
        }

        [Fact]
        public async Task ShouldReturnModelWithErrorIfServiceReturnsInternalServerError()
        {
            const string errorMessage = "Oh no! Something has gone wrong!";

            SetHandler(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Content = new StringContent(errorMessage)
            });

            var httpClient = new HttpClient(_handlerMock.Object) { BaseAddress = new Uri("http://localhost:1234/") };

            var result = await new SaveDocumentService(httpClient).Save(_dummyFile, CancellationToken.None);

            var expected = new SaveDocumentError("FILE_NOT_SAVED", "File not saved successfully");

            result.DocumentId.Should().BeNullOrEmpty();
            result.Errors.Should().ContainEquivalentOf(expected);
        }

        [Fact]
        public async Task ShouldReturnModelWithErrorIfFileIsTooLarge()
        {
            var httpClient = new HttpClient(_handlerMock.Object) { BaseAddress = new Uri("http://localhost:1234/") };

            var dummyFile = new Mock<IFormFile>();

            dummyFile.Setup(f => f.Length).Returns(99999999);

            var result = await new SaveDocumentService(httpClient).Save(dummyFile.Object, CancellationToken.None);

            var expected = new SaveDocumentError("FILE_TOO_LARGE", "The file must be smaller than 5MB");

            result.DocumentId.Should().BeNullOrEmpty();
            result.Errors.Should().ContainEquivalentOf(expected);
        }

        [Fact]
        public async Task ShouldReturnModelWithErrorIfFileTypeIsNotAllowed()
        {
            var httpClient = new HttpClient(_handlerMock.Object) { BaseAddress = new Uri("http://localhost:1234/") };

            var dummyFile = new Mock<IFormFile>();

            dummyFile.Setup(f => f.FileName).Returns("dummy-file.xyz");

            var result = await new SaveDocumentService(httpClient).Save(dummyFile.Object, CancellationToken.None);

            var expected = new SaveDocumentError("FILE_TYPE_NOT_ALLOWED", "The file must be in one of these formats JPG, BMP, PNG, TIF, PDF, XLS or XLSX");

            result.DocumentId.Should().BeNullOrEmpty();
            result.Errors.Should().ContainEquivalentOf(expected);
        }

        [Theory]
        [InlineData("dummy-file.jPg")]
        [InlineData("dummy-file.bmP")]
        [InlineData("dummy-file.png")]
        [InlineData("dummy-file.tIf")]
        [InlineData("dummy-file.pdf")]
        [InlineData("dummy-file.XLS")]
        [InlineData("dummy-file.xlsx")]
        public async Task ShouldReturnModelWithoutErrorIfFileTypeIsAllowed(string fileName)
        {
            var httpClient = new HttpClient(_handlerMock.Object) { BaseAddress = new Uri("http://localhost:1234/") };

            var dummyFile = new Mock<IFormFile>();

            dummyFile.Setup(f => f.FileName).Returns(fileName);

            var result = await new SaveDocumentService(httpClient).Save(dummyFile.Object, CancellationToken.None);

            result.Errors.Should().NotContain("FILE_TYPE_NOT_ALLOWED");
        }

        [Fact]
        public async Task ShouldReturnModelWithHasErrorsIfErrorsInCollection()
        {
            var httpClient = new HttpClient(_handlerMock.Object) { BaseAddress = new Uri("http://localhost:1234/") };

            var result = await new SaveDocumentService(httpClient).Save(null, CancellationToken.None);

            result.HasErrors.Should().BeTrue();
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
