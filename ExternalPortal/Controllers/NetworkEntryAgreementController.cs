using System;
using System.Threading;
using System.Threading.Tasks;
using ExternalPortal.Enums;
using ExternalPortal.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ExternalPortal.Controllers
{
    public class NetworkEntryAgreementController : BaseController
    {
        private readonly ILogger<NetworkEntryAgreementController> _logger;

        public NetworkEntryAgreementController(IRedisCacheService redisCache, ILogger<NetworkEntryAgreementController> logger)
            : base(redisCache)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        public async Task<IActionResult> WhatYouWillNeed(CancellationToken token)
        {
            _logger.LogInformation("Production details Index called...");

            return await WhatYouWillNeedAsync(TaskType.NetworkEntryAgreement, nameof(WhatYouWillNeed), token);
        }
    }
}
