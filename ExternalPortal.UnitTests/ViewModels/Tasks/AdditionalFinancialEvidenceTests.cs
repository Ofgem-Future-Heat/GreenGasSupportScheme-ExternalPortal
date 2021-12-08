using System.Linq;
using ExternalPortal.Enums;
using ExternalPortal.Models;
using ExternalPortal.ViewModels.Tasks;
using Xunit;

namespace ExternalPortal.UnitTests.ViewModels.Tasks
{
    public class AdditionalFinancialEvidenceTests
    {
        [Fact]
        public void ShouldReturnEmptyModelFromConstructor()
        {
            var actual = new AdditionalFinancialEvidenceModel();

            Assert.Equal(TaskStatus.CannotStartYet, actual.State);
        }

        [Fact]
        public void ShouldAddDocumentToDocumentCollection()
        {
            var actual = new AdditionalFinancialEvidenceModel();

            actual.AddDocument(new Document());

            Assert.NotEmpty(actual.Documents);
            Assert.Single(actual.Documents);
        }

        [Fact]
        public void ShouldAddSecondDocumentToDocumentCollection()
        {
            var actual = new AdditionalFinancialEvidenceModel();

            actual.AddDocument(new Document());
            actual.AddDocument(new Document());

            Assert.NotEmpty(actual.Documents);
            Assert.Equal(2, actual.Documents.Count);
        }

        [Fact]
        public void ShouldRemoveHandleDocumentNotFoundInDocumentCollection()
        {
            const string DocumentId = "123456";

            var actual = new AdditionalFinancialEvidenceModel();

            actual.RemoveDocumentFor(DocumentId);

            Assert.Empty(actual.Documents);
        }

        [Fact]
        public void ShouldRemoveHandleDocumentNotFoundInDocumentCollectionNotEmpty()
        {
            const string DocumentId = "123456";

            var actual = new AdditionalFinancialEvidenceModel();

            actual.AddDocument(new Document());

            actual.RemoveDocumentFor(DocumentId);

            Assert.NotEmpty(actual.Documents);
            Assert.Single(actual.Documents);
        }

        [Fact]
        public void ShouldRemoveDocumentFromDocumentCollection()
        {
            const string DocumentId = "123456";

            var document = new Document() { DocumentId = DocumentId };

            var actual = new AdditionalFinancialEvidenceModel();

            actual.AddDocument(document);

            actual.RemoveDocumentFor(DocumentId);

            Assert.Empty(actual.Documents);
        }

        [Fact]
        public void ShouldRemoveOnlyOneDocumentFromDocumentCollection()
        {
            const string DocumentId = "123456";

            var document = new Document() { DocumentId = DocumentId };

            var actual = new AdditionalFinancialEvidenceModel();

            actual.AddDocument(document);
            actual.AddDocument(new Document());

            actual.RemoveDocumentFor(DocumentId);

            Assert.NotEmpty(actual.Documents);
            Assert.Single(actual.Documents);
        }

        [Fact]
        public void ShouldReturnEmptyStringForUploadCountWhenDocumentCollectionEmpty()
        {
            var actual = new AdditionalFinancialEvidenceModel();

            Assert.Empty(actual.UploadedHeading);
        }

        [Fact]
        public void ShouldReturnSingularHeadingForUploadCountWhenDocumentCollectionHasOneItem()
        {
            var actual = new AdditionalFinancialEvidenceModel();

            actual.AddDocument(new Document());

            Assert.Equal("You've uploaded 1 document", actual.UploadedHeading);
        }

        [Fact]
        public void ShouldReturnPluralHeadingForUploadCountWhenDocumentCollectionHasMoreThanOneItem()
        {
            var actual = new AdditionalFinancialEvidenceModel();

            actual.AddDocument(new Document());
            actual.AddDocument(new Document());

            Assert.Equal("You've uploaded 2 documents", actual.UploadedHeading);
        }

        [Fact]
        public void ShouldReturnPluralHeadingForUploadCountWhenDocumentCollectionHasThreeItems()
        {
            var actual = new AdditionalFinancialEvidenceModel();

            actual.AddDocument(new Document());
            actual.AddDocument(new Document());
            actual.AddDocument(new Document());

            Assert.Equal("You've uploaded 3 documents", actual.UploadedHeading);
        }

        [Fact]
        public void ShouldReturnAddReferenceToFile()
        {
            var documentId = "123456";

            var actual = new AdditionalFinancialEvidenceModel();

            actual.AddDocument(new Document() { DocumentId = documentId });

            actual.AddDocumentReference(documentId, "file-reference");

            Assert.Equal("file-reference", actual.Documents.First().Reference);
        }

        [Fact]
        public void ShouldReturnFalseForHasErrorWhenErrorMessageNotSet()
        {
            var actual = new AdditionalFinancialEvidenceModel();

            Assert.False(actual.HasError());
        }

        [Fact]
        public void ShouldReturnTrueForHasErrorWhenErrorMessageSet()
        {
            var actual = new AdditionalFinancialEvidenceModel
            {
                Error = "test error message"
            };

            Assert.True(actual.HasError());
        }
    }
}
