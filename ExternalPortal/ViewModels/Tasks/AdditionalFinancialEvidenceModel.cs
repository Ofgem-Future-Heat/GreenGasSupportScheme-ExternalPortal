using System.Collections.Generic;
using System.Linq;
using ExternalPortal.Enums;
using ExternalPortal.Models;

namespace ExternalPortal.ViewModels.Tasks
{
    public class AdditionalFinancialEvidenceModel : IApplicationTaskItemModel
    {
        public TaskType Type => TaskType.SupportingEvidence;
        public TaskStatus State { get; set; }
        public List<Document> Documents { get; set; }
        public string UploadedHeading => GetUploadedHeading();
        public List<Option> Options => GetOptions();
        public List<Option> MoreOptions => GetMoreOptions();
        public string Error { get; set; }
        public string AddEvidence { get; set; }
        public bool HasDocuments => Documents.Any();

        public AdditionalFinancialEvidenceModel()
        {
            State = TaskStatus.CannotStartYet;
            Documents = new List<Document>();
        }

        public string GetUploadedHeading()
        {
            if (Documents.Any())
            {
                return $"You've uploaded {Documents.Count} {(Documents.Count == 1 ? "document" : "documents")}";
            }

            return string.Empty;
        }

        public bool HasError()
        {
            return !string.IsNullOrEmpty(Error);
        }

        public void AddDocument(Document document)
        {
            Documents.Add(document);
        }

        public void RemoveDocumentFor(string documentId)
        {
            Documents = Documents.Where(d => d.DocumentId != documentId).ToList();
        }

        public void AddDocumentReference(string documentId, string reference)
        {
            var document = Documents.FirstOrDefault(d => d.DocumentId == documentId);

            if (document != null)
            {
                document.Reference = reference;
            }
        }

        private List<Option> GetOptions()
        {
            return new List<Option>()
            {
                new Option(){ Id = "1", Text = "Yes, add this file", Value = "Yes"},
                new Option(){ Id = "2", Text = "No, I want to choose a different file", Value = "No"}
            };
        }

        private List<Option> GetMoreOptions()
        {
            return new List<Option>()
            {
                new Option(){ Id = "1", Text = "Yes", Value = "Yes"},
                new Option(){ Id = "2", Text = "No", Value = "No"}
            };
        }
    }
}
