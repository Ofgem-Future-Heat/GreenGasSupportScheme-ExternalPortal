using System.IO;
using ExternalPortal.Models;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace ExternalPortal.UnitTests.Models
{
    public class DocumentTests
    {
        [Fact]
        public void ShouldReturnEmptyStringForFileSizeWhenFileIsNull()
        {
            var document = new Document(null, "filename");

            Assert.Empty(document.FileSizeAsString);
        }

        [Fact]
        public void ShouldReturnFileSizeWhenFileIsNotNull()
        {
            var stream = GetStream("Hello World from a Fake File");

            var file = new FormFile(stream, 0, stream.Length, "fake-file", "fake-filename.ext");

            using var ms = new MemoryStream();
            file.CopyTo(ms);
            var fileBytes = ms.ToArray();

            var document = new Document(fileBytes, "filename");

            Assert.NotEmpty(document.FileSizeAsString);
        }

        private static Stream GetStream(string content)
        {
            var ms = new MemoryStream();
            var writer = new StreamWriter(ms);
            writer.Write(content);
            writer.Flush();
            ms.Position = 0;

            return ms;
        }
    }
}
