using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ExternalPortal.Services
{
    public interface IGetDocumentService
    {
        Task<GetDocumentResponse> Get(string documentId, CancellationToken cancellationToken = default);
    }

    public class GetDocumentService : IGetDocumentService
    {
        private readonly HttpClient _client;

        public GetDocumentService(HttpClient client)
        {
            _client = client;
        }

        public async Task<GetDocumentResponse> Get(string documentId, CancellationToken cancellationToken = default)
        {
            CheckParameter(documentId);

            var response = new GetDocumentResponse();

            var serviceResponse = await _client.GetAsync($"/get/{documentId}", cancellationToken);

            if (!serviceResponse.IsSuccessStatusCode)
            {
                response.AddError("DOCUMENT_NOT_FOUND");

                return response;
            }

            response.Contents = await serviceResponse.Content.ReadAsByteArrayAsync();

            return response;
        }

        private void CheckParameter(string documentId)
        {
            if (string.IsNullOrWhiteSpace(documentId))
            {
                throw new System.ArgumentNullException(nameof(documentId));
            }
        }
    }

    public class GetDocumentResponse
    {
        private readonly List<string> _errors = new List<string>();

        public byte[] Contents { get; set; }

        public ReadOnlyCollection<string> Errors => _errors.AsReadOnly();

        public void AddError(string error)
        {
            _errors.Add(error);
        }
    }
}