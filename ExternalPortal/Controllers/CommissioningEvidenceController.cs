using System;
using System.Threading;
using System.Threading.Tasks;
using ExternalPortal.Enums;
using ExternalPortal.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ExternalPortal.Controllers
{
    public class CommissioningEvidenceController : BaseController
    {
        private readonly ILogger<CommissioningEvidenceController> _logger;

        public CommissioningEvidenceController(IRedisCacheService redisCache, ILogger<CommissioningEvidenceController> logger)
            : base(redisCache)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        public async Task<IActionResult> WhatYouWillNeed(CancellationToken token)
        {
            _logger.LogInformation("Production details Index called...");

            return await WhatYouWillNeedAsync(TaskType.CommissioningEvidence, nameof(WhatYouWillNeed), token);
        }
    }
}
