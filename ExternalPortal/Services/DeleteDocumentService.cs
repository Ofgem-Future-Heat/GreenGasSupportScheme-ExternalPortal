using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ExternalPortal.Services
{
    public interface IDeleteDocumentService
    {
        Task<DeleteDocumentResponse> Delete(string documentId, CancellationToken cancellationToken = default);
    }

    public class DeleteDocumentService : IDeleteDocumentService
    {
        private readonly HttpClient _client;

        public DeleteDocumentService(HttpClient client)
        {
            _client = client;
        }
        
        public async Task<DeleteDocumentResponse> Delete(string documentId, CancellationToken cancellationToken = default)
        {
            CheckParameter(documentId);

            var deleteDocumentResponse = new DeleteDocumentResponse();
            var serviceResponse = await _client.DeleteAsync($"/delete/{documentId}", cancellationToken);

            if (!serviceResponse.IsSuccessStatusCode)
            {
                deleteDocumentResponse.AddError("DOCUMENT_NOT_DELETED");
            }

            return deleteDocumentResponse;
        }

        private void CheckParameter(string documentId)
        {
            if (string.IsNullOrWhiteSpace(documentId))
            {
                throw new System.ArgumentNullException(nameof(documentId));
            }
        }
    }

    public class DeleteDocumentResponse
    {
        private readonly List<string> _errors = new List<string>();

        public ReadOnlyCollection<string> Errors => _errors.AsReadOnly();

        public void AddError(string error)
        {
            _errors.Add(error);
        }
    }
}