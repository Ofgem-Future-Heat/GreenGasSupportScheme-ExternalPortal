using ExternalPortal.Controllers;
using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Xunit;

namespace ExternalPortal.UnitTests.Controllers
{
    public class HomeControllerTests
    {
        ILogger<HomeController> _logger;
        HomeController _controller;

        public HomeControllerTests()
        {
            _logger = A.Fake<ILogger<HomeController>>();

            _controller = new HomeController(_logger);
        }
        [Fact]
        public void HomeControllerReturnsIndexActionView()
        {
            var result = _controller.Index();

            Assert.IsType<ViewResult>(result);

            A.CallTo(_logger)
                .Where(x => x.Method.Name == "Log" && x.Arguments.Get<LogLevel>(0) == LogLevel.Debug)
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void HomeControllerReturnsStepsInvolvedActionView()
        {
            var result = _controller.Steps();

            Assert.IsType<ViewResult>(result);

            A.CallTo(_logger)
                .Where(x => x.Method.Name == "Log" && x.Arguments.Get<LogLevel>(0) == LogLevel.Debug)
                .MustHaveHappenedOnceExactly();
        }
        
        [Fact]
        public void HomeControllerReturnsAccessibilityStatementView()
        {
            var result = _controller.Accessibility();

            Assert.IsType<ViewResult>(result);
            Assert.Equal("Accessibility", ((ViewResult)result).ViewName);
        }
    }
}
