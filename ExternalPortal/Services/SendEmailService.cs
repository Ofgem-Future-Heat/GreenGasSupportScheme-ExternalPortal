using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ExternalPortal.Models;
using Microsoft.Extensions.Logging;

namespace ExternalPortal.Services
{
    public interface ISendEmailService
    {
        public Task Send(EmailParameter emailParameter, CancellationToken token, bool isTest = false);
    }

    public class SendEmailService : ISendEmailService
    {
        private readonly ILogger<SendEmailService> _logger;
        private readonly HttpClient _client;

        public SendEmailService(ILogger<SendEmailService> logger, HttpClient client)
        {
            _logger = logger;
            _client = client;
        }

        public async Task Send(EmailParameter emailParameter, CancellationToken token, bool isTest = false)
        {
            try
            {
                LogInfoMessage(emailParameter.TemplateId, emailParameter.EmailAddress, emailParameter.Reference);

                var request = new HttpRequestMessage(HttpMethod.Post, "/send")
                {
                    Content = emailParameter.Content
                };

                var response = await _client
                    .SendAsync(request, HttpCompletionOption.ResponseContentRead, token)
                    .ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException(await response.Content.ReadAsStringAsync());
                }
            }
            catch (Exception ex)
            {
                LogErrorMessage(emailParameter.Reference, ex.Message);

                if (isTest)
                {
                    throw;
                }
            }
        }

        private void LogInfoMessage(string templateId, string emailAddress, string reference)
        {
            var message = $"Email send for template {templateId} to {emailAddress} with reference {reference}";

            LogMessage(LogLevel.Information, message);
        }

        private void LogErrorMessage(string reference, string reason)
        {
            var message = $"Email send with reference {reference} failed - reason: {reason}";

            LogMessage(LogLevel.Error, message);
        }

        private void LogMessage(LogLevel logLevel, string message)
        {
            _logger.Log(logLevel, message);
        }
    }
}