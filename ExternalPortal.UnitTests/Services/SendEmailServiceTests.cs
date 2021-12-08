using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ExternalPortal.Models;
using ExternalPortal.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Xunit;

namespace ExternalPortal.UnitTests.Services
{
    public class SendEmailServiceTests
    {
        private readonly Mock<HttpMessageHandler> _handlerMock;
        private readonly Mock<ILogger<SendEmailService>> _logger;
        private readonly HttpClient _httpClient;
        private readonly SendEmailService _sendEmailService;

        public SendEmailServiceTests()
        {
            _handlerMock = new Mock<HttpMessageHandler>();
            _logger = new Mock<ILogger<SendEmailService>>();
            _httpClient = new HttpClient(_handlerMock.Object) { BaseAddress = new Uri("http://localhost:1234/") };

            _sendEmailService = new SendEmailService(_logger.Object, _httpClient);
        }

        [Fact]
        public async Task ShouldReturnLogInformationOnlyWhenSendEmailOk()
        {
            SetHandler(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            });

            var emailParameter = new EmailParameterBuilder("template-id", "email-address").Build();

            await _sendEmailService.Send(emailParameter, CancellationToken.None);

            VerifyLogging<ILogger<SendEmailService>>(LogLevel.Information, "reference");
        }

        [Fact]
        public async Task ShouldReturnLogErrorWhenSendEmailReturns400()
        {
            SetHandler(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = new StringContent("bad request error message")
            });

            var emailParameter = new EmailParameterBuilder("template-id", "email-address").Build();

            await _sendEmailService.Send(emailParameter, CancellationToken.None);

            VerifyLogging<ILogger<SendEmailService>>(LogLevel.Error, "reference");
            VerifyLogging<ILogger<SendEmailService>>(LogLevel.Error, "bad request error message");
        }

        [Fact]
        public async Task ShouldReturnLogErrorWhenSendEmailReturns500()
        {
            SetHandler(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = new StringContent("internal server error message")
            });

            var emailParameter = new EmailParameterBuilder("template-id", "email-address").Build();

            await _sendEmailService.Send(emailParameter, CancellationToken.None);

            VerifyLogging<ILogger<SendEmailService>>(LogLevel.Error, "reference");
            VerifyLogging<ILogger<SendEmailService>>(LogLevel.Error, "internal server error message");
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

        private void VerifyLogging<T>(LogLevel expectedLogLevel, string expectedMessage)
        {
            Func<object, Type, bool> state = (v, t) => v.ToString().Contains(expectedMessage);

            _logger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == expectedLogLevel),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => state(v, t)),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
        }
    }
}
