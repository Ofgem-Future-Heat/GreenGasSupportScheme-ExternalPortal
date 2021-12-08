using System.Diagnostics;
using ExternalPortal.Constants;
using ExternalPortal.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ExternalPortal.Controllers
{
    [AllowAnonymous]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            _logger.LogDebug("Index action on Home controller called");

            return View();
        }

        [Route(UrlKeys.TariffGuaranteeProcess)]
        public IActionResult TariffGuaranteeProcess()
        {
            _logger.LogDebug("TariffGuaranteeProcess action on Home controller called");

            return View(nameof(TariffGuaranteeProcess));
        }

        [Route(UrlKeys.ApplyingStageOne)]
        public IActionResult ApplyingStageOne()
        {
            _logger.LogDebug("ApplyingStageOne action on Home controller called");

            return View(nameof(ApplyingStageOne));
        }

        [Route(UrlKeys.ApplyingStageTwo)]
        public IActionResult ApplyingStageTwo()
        {
            _logger.LogDebug("ApplyingStageTwo action on Home controller called");

            return View(nameof(ApplyingStageTwo));
        }

        [Route(UrlKeys.ApplyingStageThree)]
        public IActionResult ApplyingStageThree()
        {
            _logger.LogDebug("ApplyingStageThree action on Home controller called");

            return View(nameof(ApplyingStageThree));
        }

        public IActionResult Steps()
        {
            _logger.LogDebug("Steps action on Home controller called");

            return View();
        }
        
        public IActionResult Accessibility()
        {
            return View("Accessibility");
        }
        
        public IActionResult TermsAndConditions()
        {
            return View("TermsAndConditions");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new Error { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
