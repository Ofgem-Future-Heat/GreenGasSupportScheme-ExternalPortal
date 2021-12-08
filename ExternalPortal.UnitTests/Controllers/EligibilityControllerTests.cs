using ExternalPortal.Controllers;
using ExternalPortal.Enums;
using ExternalPortal.ViewModels;
using ExternalPortal.ViewModels.Eligibility;
using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ofgem.API.GGSS.Domain.Enums;
using Xunit;

namespace ExternalPortal.UnitTests.Controllers
{
    public class EligibilityControllerTests
    {
        private readonly ILogger<EligibilityController> _logger;
        private readonly EligibilityController _controller;

        public EligibilityControllerTests()
        {
            _logger = A.Fake<ILogger<EligibilityController>>();

            _controller = new EligibilityController(_logger);
        }

        [Fact]
        public void EligibilityControllerReturnsIndexActionView()
        {
            var result = _controller.Index();

            Assert.IsType<ViewResult>(result);
            Assert.Equal("Index", ((ViewResult)result).ViewName);

            A.CallTo(_logger)
                .Where(x => x.Method.Name == "Log" && x.Arguments.Get<LogLevel>(0) == LogLevel.Debug)
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void ShouldReturnIndexViewWithErrorWhenGasInjectionIsNotAnswered()
        {
            var result = _controller.GasInjection(new EligibilityFlow());

            Assert.IsType<ViewResult>(result);
            Assert.Equal("Index", ((ViewResult)result).ViewName);
            Assert.IsType<EligibilityFlow>(((ViewResult)result).Model);

            A.CallTo(_logger)
                .Where(x => x.Method.Name == "Log" && x.Arguments.Get<LogLevel>(0) == LogLevel.Debug)
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void ShouldRedirectToIneligibleIfGasInjectionIsNo()
        {
            var model = new EligibilityFlow()
            {
                GasInjection = "No"
            };

            var result = _controller.GasInjection(model);

            Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Ineligible", ((RedirectToActionResult)result).ActionName);

            A.CallTo(_logger)
                .Where(x => x.Method.Name == "Log" && x.Arguments.Get<LogLevel>(0) == LogLevel.Debug)
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void ShouldRedirectToNewPlantIfGasInjectionIsYes()
        {
            var model = new EligibilityFlow()
            {
                GasInjection = "Yes"
            };

            var result = _controller.GasInjection(model);

            Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("NewPlant", ((RedirectToActionResult)result).ActionName);

            A.CallTo(_logger)
                .Where(x => x.Method.Name == "Log" && x.Arguments.Get<LogLevel>(0) == LogLevel.Debug)
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void EligibilityControllerReturnsNewPlantActionView()
        {
            var result = _controller.NewPlant();

            Assert.IsType<ViewResult>(result);
            Assert.Equal("NewPlant", ((ViewResult)result).ViewName);
            Assert.IsType<EligibilityFlow>(((ViewResult)result).Model);

            A.CallTo(_logger)
                .Where(x => x.Method.Name == "Log" && x.Arguments.Get<LogLevel>(0) == LogLevel.Debug)
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void ShouldReturnIndexViewWithErrorWhenNewPlantIsNotAnswered()
        {
            var result = _controller.NewPlant(new EligibilityFlow());

            Assert.IsType<ViewResult>(result);
            Assert.Equal("NewPlant", ((ViewResult)result).ViewName);
            Assert.IsType<EligibilityFlow>(((ViewResult)result).Model);

            A.CallTo(_logger)
                .Where(x => x.Method.Name == "Log" && x.Arguments.Get<LogLevel>(0) == LogLevel.Debug)
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void ShouldRedirectToIneligibleIfNewPlantIsNo()
        {
            var model = new EligibilityFlow()
            {
                NewPlant = "No"
            };

            var result = _controller.NewPlant(model);

            Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("FinancialSupport", ((RedirectToActionResult)result).ActionName);

            A.CallTo(_logger)
                .Where(x => x.Method.Name == "Log" && x.Arguments.Get<LogLevel>(0) == LogLevel.Debug)
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void ShouldRedirectToNewPlantIfNewPlantIsYes()
        {
            var model = new EligibilityFlow
            {
                NewPlant = "Yes"
            };

            var result = _controller.NewPlant(model);

            Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Ineligible", ((RedirectToActionResult)result).ActionName);

            A.CallTo(_logger)
                .Where(x => x.Method.Name == "Log" && x.Arguments.Get<LogLevel>(0) == LogLevel.Debug)
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void EligibilityControllerReturnsFinancialSupportActionView()
        {
            var result = _controller.FinancialSupport();

            Assert.IsType<ViewResult>(result);
            Assert.Equal("FinancialSupport", ((ViewResult)result).ViewName);
            Assert.IsType<EligibilityFlow>(((ViewResult)result).Model);

            A.CallTo(_logger)
                .Where(x => x.Method.Name == "Log" && x.Arguments.Get<LogLevel>(0) == LogLevel.Debug)
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void ShouldReturnIndexViewWithErrorWhenFinancialSupportIsNotAnswered()
        {
            var result = _controller.FinancialSupport(new EligibilityFlow());

            Assert.IsType<ViewResult>(result);
            Assert.Equal("FinancialSupport", ((ViewResult)result).ViewName);
            Assert.IsType<EligibilityFlow>(((ViewResult)result).Model);

            A.CallTo(_logger)
                .Where(x => x.Method.Name == "Log" && x.Arguments.Get<LogLevel>(0) == LogLevel.Debug)
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void ShouldRedirectToIneligibleIfFinancialSupportIsNo()
        {
            var model = new EligibilityFlow()
            {
                FinancialSupport = "No"
            };

            var result = _controller.FinancialSupport(model);

            Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Eligible", ((RedirectToActionResult)result).ActionName);

            A.CallTo(_logger)
                .Where(x => x.Method.Name == "Log" && x.Arguments.Get<LogLevel>(0) == LogLevel.Debug)
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void ShouldRedirectToIneligibleIfFinancialSupportIsYes()
        {
            var model = new EligibilityFlow()
            {
                FinancialSupport = "Yes"
            };

            var result = _controller.FinancialSupport(model);

            Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("IneligibleFunding", ((RedirectToActionResult)result).ActionName);

            A.CallTo(_logger)
                .Where(x => x.Method.Name == "Log" && x.Arguments.Get<LogLevel>(0) == LogLevel.Debug)
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void EligibilityControllerReturnsEligibleActionView()
        {
            var result = _controller.Eligible();

            Assert.IsType<ViewResult>(result);
            Assert.Equal("Eligible", ((ViewResult)result).ViewName);

            A.CallTo(_logger)
                .Where(x => x.Method.Name == "Log" && x.Arguments.Get<LogLevel>(0) == LogLevel.Debug)
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void EligibilityControllerReturnsIneligibleActionView()
        {
            var result = _controller.Ineligible();

            Assert.IsType<ViewResult>(result);
            Assert.Equal("Ineligible", ((ViewResult)result).ViewName);

            A.CallTo(_logger)
                .Where(x => x.Method.Name == "Log" && x.Arguments.Get<LogLevel>(0) == LogLevel.Debug)
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void EligibilityControllerReturnsIneligibleFundingActionView()
        {
            var result = _controller.IneligibleFunding();

            Assert.IsType<ViewResult>(result);
            Assert.Equal("IneligibleFunding", ((ViewResult)result).ViewName);

            A.CallTo(_logger)
                .Where(x => x.Method.Name == "Log" && x.Arguments.Get<LogLevel>(0) == LogLevel.Debug)
                .MustHaveHappenedOnceExactly();
        }
    }
}
