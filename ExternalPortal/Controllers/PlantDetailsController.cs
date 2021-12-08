using System;
using System.Threading;
using System.Threading.Tasks;
using ExternalPortal.Enums;
using ExternalPortal.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ExternalPortal.Controllers
{
    public class PlantDetailsController : BaseController
    {
        private readonly ILogger<PlantDetailsController> _logger;

        public PlantDetailsController(IRedisCacheService redisCache, ILogger<PlantDetailsController> logger)
            : base(redisCache)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        public IActionResult WhatYouWillNeed()
        {
            _logger.LogInformation("Plant Details Index called.");

            return RedirectToAction("plant", "apply");
        }
        
        [HttpGet]
        public IActionResult CheckAnswers()
        {
            return RedirectToAction("check-answers", "tell-us-about-your-site");
        }
    }
}
