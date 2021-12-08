using ExternalPortal.Constants;
using ExternalPortal.Services;
using ExternalPortal.ViewModels.Shared;
using ExternalPortal.ViewModels.Shared.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;

namespace ExternalPortal.Controllers
{
    public class StageTwoController : BaseController
    {
        private readonly ILogger<StageTwoController> _logger;

        public StageTwoController(ILogger<StageTwoController> logger, IRedisCacheService redisCache) : base(redisCache)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        [Route(UrlKeys.Stage2confirmationLink)]
        public IActionResult Stage2Confirmation()
        {
            _logger.LogDebug("Stage2Confirmation action on StageTwoController controller called");

            var confirmationViewModel = new ConfirmationViewModel
            {
                PageHeading = new PageHeadingViewModel
                {
                    Heading = "Stage 2 complete",
                    SubHeading = "What happens next",
                    SuperHeading = "We'll contact you to confirm your registration, or to ask for more information."
                },
                ConfirmationText = "We've received your application and sent you a confirmation email.",
                BackAction = "/task-list"
            };

            return View("~/Views/Shared/Confirmation.cshtml", confirmationViewModel);
        }
    }
}
