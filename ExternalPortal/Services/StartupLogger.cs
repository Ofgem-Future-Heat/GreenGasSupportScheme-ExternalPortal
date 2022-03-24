using Microsoft.Extensions.Logging;

namespace ExternalPortal.Services
{
    public class StartupLogger
    {
        private readonly ILogger _logger;

        public StartupLogger(ILogger<StartupLogger> logger)
        {
            _logger = logger;
        }

        public void Log(string message)
        {
            _logger.LogInformation(message);
        }
    }
}