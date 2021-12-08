using System;
using ExternalPortal.Constants;
using ExternalPortal.Enums;
using ExternalPortal.ViewModels.Eligibility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace ExternalPortal.Controllers
{
    [AllowAnonymous]
    public class EligibilityController : Controller
    {
        private readonly ILogger<EligibilityController> _logger;

        public EligibilityController(ILogger<EligibilityController> logger)
        {
            _logger = logger;
        }

        [Route(UrlKeys.EligibilityGasInjection)]
        public IActionResult Index()
        {
            _logger.LogDebug("Index action called on eligibility controller");

            var model = new EligibilityFlow();

            return View(nameof(Index), model);
        }

        [HttpPost]
        public IActionResult GasInjection(EligibilityFlow model)
        {
            _logger.LogDebug("GasInjection action called on eligibility controller");

            if (string.IsNullOrWhiteSpace( model.GasInjection))
            {
                var errorMessage = "Select yes if you plan to produce biomethane for injection into the gas grid";
                
                model.Error = errorMessage;

                ModelState.AddModelError("select-option", errorMessage);

                return View(nameof(Index), model);
            }

            if (model.GasInjection == "No")
            {
                return RedirectToAction("Ineligible");
            }

            return RedirectToAction("NewPlant");
        }

        [Route(UrlKeys.EligibilityNewInstallation)]
        public IActionResult NewPlant()
        {
            _logger.LogDebug("NewPlant action called on eligibility controller");

            return View(nameof(NewPlant), new EligibilityFlow());
        }

        [HttpPost]
        [Route(UrlKeys.EligibilityNewInstallation)]
        public IActionResult NewPlant(EligibilityFlow model)
        {
            _logger.LogDebug("NewPlant action called on eligibility controller");

            if (string.IsNullOrWhiteSpace(model.NewPlant))
            {
                var errorMessage = "Select yes if the equipment has already been used to produce biogas or biomethane";
                
                model.Error = errorMessage;
                
                ModelState.AddModelError("select-option", errorMessage);

                return View(nameof(NewPlant), model);
            }

            if (model.NewPlant == "Yes")
            {
                return RedirectToAction("Ineligible");
            }

            return RedirectToAction("FinancialSupport");
        }

        [Route(UrlKeys.EligibilityFinancialSupport)]
        public IActionResult FinancialSupport()
        {
            _logger.LogDebug("FinancialSupport action called on eligibility controller");

            return View(nameof(FinancialSupport), new EligibilityFlow());
        }

        [HttpPost]
        [Route(UrlKeys.EligibilityFinancialSupport)]
        public IActionResult FinancialSupport(EligibilityFlow model)
        {
            _logger.LogDebug("FinancialSupport action called on eligibility controller");

            if (string.IsNullOrWhiteSpace(model.FinancialSupport))
            {
                var errorMessage = "Select yes if the equipment has been used to secure an NDRHI tariff guarantee";

                model.Error = errorMessage;
                
                ModelState.AddModelError("select-option", errorMessage);

                return View(nameof(FinancialSupport), model);
            }

            if (model.FinancialSupport == "No")
            {
                return RedirectToAction("Eligible");
            }

            return RedirectToAction("IneligibleFunding");
        }

        [Route(UrlKeys.EligibilityCanApply)]
        public IActionResult Eligible()
        {
            _logger.LogDebug("Eligible action called on eligibility controller");

            return View(nameof(Eligible));
        }

        public IActionResult Ineligible()
        {
            _logger.LogDebug("Ineligible action called on eligibility controller");

            return View(nameof(Ineligible));
        }

        [Route(UrlKeys.EligibilityFunding)]
        public IActionResult IneligibleFunding()
        {
            _logger.LogDebug("IneligibleFunding action called on eligibility controller");

            return View(nameof(IneligibleFunding));
        }
    }
}
