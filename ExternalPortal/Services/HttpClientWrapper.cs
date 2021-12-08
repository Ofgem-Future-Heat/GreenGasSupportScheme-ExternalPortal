using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace ExternalPortal.Services
{
    public interface IHttpClientWrapper
    {
        Task<HttpResponseMessage> PostAsync(Uri uri, MultipartFormDataContent data);
        Task DeleteAsync(Uri uri);
    }

    public class HttpClientWrapper : IHttpClientWrapper
    {
        private readonly HttpClient _client;

        public HttpClientWrapper()
        {
            _client = new HttpClient();
        }

        public async Task<HttpResponseMessage> PostAsync(Uri uri, MultipartFormDataContent data)
        {
            return await _client.PostAsync(uri, data);
        }

        public async Task DeleteAsync(Uri uri)
        {
            _ = await _client.DeleteAsync(uri);
        }
    }
}
