using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace ExternalPortal.Services
{
    public interface ISaveDocumentService
    {
        Task<SaveDocumentResponse> Save(IFormFile file, CancellationToken token = default);
    }

    public class SaveDocumentService : ISaveDocumentService
    {
        const long MaximumFileSize = 5242880;
        static readonly string[] AllowedFileExtensions = { ".JPG", ".BMP", ".PNG", ".TIF", ".PDF", ".XLS", ".XLSX" };

        private readonly HttpClient _client;

        public SaveDocumentService(HttpClient client)
        {
            _client = client;
        }

        public async Task<SaveDocumentResponse> Save(IFormFile file, CancellationToken token = default)
        {
            var documentResponse = new SaveDocumentResponse();

            documentResponse.AddErrors(ValidateFile(file));

            if (documentResponse.HasErrors)
            {
                return documentResponse;
            }

            try
            {
                using var response = await MakeSaveFileRequest(file, token);

                if (response.IsSuccessStatusCode)
                {
                    documentResponse.DocumentId = ExtractField(await response.Content.ReadAsStringAsync(), "documentId");
                }
                else
                {
                    documentResponse.AddError(new SaveDocumentError("FILE_NOT_SAVED", "File not saved successfully"));
                }
            }
            catch (Exception ex)
            {
                documentResponse.AddError(new SaveDocumentError("FILE_NOT_SAVED", $"{ex.Message}"));
            }

            return documentResponse;
        }

        private async Task<HttpResponseMessage> MakeSaveFileRequest(IFormFile file, CancellationToken token)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, "/save") { Content = ToMultipartFormDataContent(file) };
            return await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, token)
                .ConfigureAwait(false);
        }

        private static MultipartFormDataContent ToMultipartFormDataContent(IFormFile file)
        {
            byte[] data;
            using (var reader = new BinaryReader(file.OpenReadStream()))
            {
                data = reader.ReadBytes((int)file.OpenReadStream().Length);
            }

            MultipartFormDataContent content = new MultipartFormDataContent
            {
                {new ByteArrayContent(data), "file", file.FileName}
            };
            return content;
        }

        private static IEnumerable<SaveDocumentError> ValidateFile(IFormFile file)
        {
            var errors = new List<SaveDocumentError>();

            if (file == null)
            {
                errors.Add(new SaveDocumentError("NULL_FILE_VALIDATION_ERROR", "Upload a file"));
            }

            if (file?.Length > MaximumFileSize)
            {
                errors.Add(new SaveDocumentError("FILE_TOO_LARGE", "The file must be smaller than 5MB"));
            }

            if (IsFileExtensionNotAllowed(file))
            {
                errors.Add(new SaveDocumentError("FILE_TYPE_NOT_ALLOWED", $"The file must be in one of these formats {GetListOfAllowedFilesExtensionsAsString()}"));
            }

            return errors;
        }

        private static bool IsFileExtensionNotAllowed(IFormFile file)
        {
            if (string.IsNullOrWhiteSpace(file?.FileName))
            {
                return false;
            }

            var fileExtension = new FileInfo(file.FileName).Extension;

            return !AllowedFileExtensions.Any(f => f.Equals(fileExtension, StringComparison.InvariantCultureIgnoreCase));
        }

        private static string ExtractField(string response, string field)
        {
            var documentId = JsonSerializer.Deserialize<IDictionary<string, string>>(response)[field];

            if (string.IsNullOrWhiteSpace(documentId))
            {
                throw new InvalidOperationException("Document Id was not returned");
            }

            return documentId;
        }

        private static string GetListOfAllowedFilesExtensionsAsString()
        {
            return $"{string.Join(", ", AllowedFileExtensions.Where(f => f != AllowedFileExtensions.Last()))} or {AllowedFileExtensions.Last()}".Replace(".", "");
        }
    }

    public class SaveDocumentResponse
    {
        private readonly List<SaveDocumentError> _errors = new List<SaveDocumentError>();

        public string DocumentId { get; set; }

        public bool HasErrors => Errors.Any();

        public ReadOnlyCollection<SaveDocumentError> Errors => _errors.AsReadOnly();

        public void AddErrors(IEnumerable<SaveDocumentError> errors)
        {
            foreach (var error in errors)
            {
                _errors.Add(error);
            }
        }

        public void AddError(SaveDocumentError error)
        {
            _errors.Add(error);
        }
    }

    public class SaveDocumentError
    {
        public string Id { get; }
        public string Message { get; }

        public SaveDocumentError(string id, string message)
        {
            Id = id;
            Message = message;
        }
    }
}