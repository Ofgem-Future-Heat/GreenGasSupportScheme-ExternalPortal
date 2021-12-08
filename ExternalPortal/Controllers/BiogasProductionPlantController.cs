using System;
using System.Threading;
using System.Threading.Tasks;
using ExternalPortal.Enums;
using ExternalPortal.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ExternalPortal.Controllers
{
    public class BiogasProductionPlantController : BaseController
    {
        private readonly ILogger<BiogasProductionPlantController> _logger;

        public BiogasProductionPlantController(IRedisCacheService redisCache, ILogger<BiogasProductionPlantController> logger)
            : base(redisCache)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        public async Task<IActionResult> WhatYouWillNeed(CancellationToken token)
        {
            _logger.LogInformation("Production details Index called...");

            return await WhatYouWillNeedAsync(TaskType.BiogasProductionPlant, nameof(WhatYouWillNeed), token);
        }
    }
}
