using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ExternalPortal.Constants;
using ExternalPortal.Enums;
using ExternalPortal.Extensions;
using ExternalPortal.Helpers;
using ExternalPortal.Services;
using ExternalPortal.ViewModels;
using ExternalPortal.ViewModels.ProductionDetails;
using ExternalPortal.ViewModels.Value;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ofgem.API.GGSS.Domain.ModelValues;
using TaskStatus = ExternalPortal.Enums.TaskStatus;

namespace ExternalPortal.Controllers
{
    public class ProductionDetailsController : BaseController
    {
        private readonly ILogger<ProductionDetailsController> _logger;
        private readonly IGetApplicationService _getApplicationService;
        private readonly IUpdateApplicationService _updateApplicationService;
        
        public ProductionDetailsController(IRedisCacheService redisCache, ILogger<ProductionDetailsController> logger, IGetApplicationService getApplicationService, IUpdateApplicationService updateApplicationService)
            : base(redisCache)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _getApplicationService = getApplicationService;
            _updateApplicationService = updateApplicationService;
        }

        [HttpGet]
        public async Task<IActionResult> WhatYouWillNeed(CancellationToken token)
        {
            _logger.LogInformation("Production details Index called...");

            return await WhatYouWillNeedAsync(TaskType.ProductionDetails, nameof(ProductionDetails), token);
        }

        [HttpGet]
        public async Task<IActionResult> ProductionDetails(CancellationToken token)
        {
            _logger.LogInformation("Proceed with Production details...");
            
            var application = await RetrieveCurrentApplicationFromApi();
            
            var vms = new StringViewModel(TaskType.ProductionDetails, TaskPropertyName.MaxCapacity)
            {
                Value = application.StageOne.ProductionDetails.MaximumInitialCapacity,
                // Commas optional as long as they're consistent
                // Can't start with "."
                // Pass: (1,000,000), (1000000)
                // Fail: (10000,000), (1,00,00)
                Regex = @"^(\d+|\d{1,3}(,\d{3})*)(\.\d+)?$",
                BackAction = UrlKeys.ProductionDetailsWhatYouWillNeedLink,
                SaveActionName = nameof(ProductionDetails),
                ReturnUrl = HttpContext.Request.Query["returnUrl"]
            };

            return base.View("StringDetail", vms);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProductionDetails([FromForm] StringViewModel vm, CancellationToken token)
        {
            _logger.LogInformation($"Saving Max Capacity value: [{vm}]");

            if (ModelState.IsInvalidString(vm, "Enter the volume in cubic metres"))
            {
                vm.TaskType = TaskType.ProductionDetails;
                vm.PropertyName = TaskPropertyName.MaxCapacity;

                // Commas optional as long as they're consistent
                // Can't start with "."
                // Pass: (1,000,000), (1000000)
                // Fail: (10000,000), (1,00,00)
                vm.Regex = @"^(\d+|\d{1,3}(,\d{3})*)(\.\d+)?$";
                vm.BackAction = "/task-list";
                vm.SaveActionName = nameof(ProductionDetails);

                return View("StringDetail", vm);
            }
            

            var persistedApplication = await RetrieveCurrentApplicationFromApi();
            
            if (persistedApplication.StageOne.ProductionDetails.Status == TaskStatus.NotStarted.ToString())
            {
                persistedApplication.StageOne.ProductionDetails.Status = TaskStatus.InProgress.ToString();
            }
            
            persistedApplication.StageOne.ProductionDetails.MaximumInitialCapacity = vm.Value;
            
            await PersistApplicationToApi(persistedApplication);

            if (!string.IsNullOrEmpty(vm.ReturnUrl))
            {
                return RedirectToAction(nameof(CheckAnswers));
            }
            return RedirectToAction(nameof(EligibleBiomethane));
        }
        
        [HttpGet]
        public async Task<IActionResult> EligibleBiomethane(CancellationToken token)
        {
            _logger.LogInformation("Proceed with Production details...");
            
            var application = await RetrieveCurrentApplicationFromApi();
            
            var vms = new StringViewModel(TaskType.ProductionDetails, TaskPropertyName.EligibleBiomethane)
            {
                Value = application.StageOne.ProductionDetails.EligibleBiomethane,
                // Commas optional as long as they're consistent
                // Can't start with "."
                // Pass: (1,000,000), (1000000)
                // Fail: (10000,000), (1,00,00)
                Regex = @"^(\d+|\d{1,3}(,\d{3})*)(\.\d+)?$",
                BackAction = UrlKeys.ProductionDetailsMaximumInitialCapacity,
                SaveActionName = nameof(EligibleBiomethane)
            };

            return base.View("StringDetail", vms);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EligibleBiomethane([FromForm] StringViewModel vm, CancellationToken token)
        {
            _logger.LogInformation($"Saving eligible Maximum Capacity value: [{vm}]");
            
            if (ModelState.IsInvalidString(vm, "Enter the volume in cubic metres"))
            {
                vm.TaskType = TaskType.ProductionDetails;
                vm.PropertyName = TaskPropertyName.EligibleBiomethane;
            
                // Commas optional as long as they're consistent
                // Can't start with "."
                // Pass: (1,000,000), (1000000)
                // Fail: (10000,000), (1,00,00)
                vm.Regex = @"^(\d+|\d{1,3}(,\d{3})*)(\.\d+)?$";
                vm.BackAction = UrlKeys.ProductionDetailsMaximumInitialCapacity;
                vm.SaveActionName = nameof(EligibleBiomethane);
            
                return View("StringDetail", vm);
            }
            
            var persistedApplication = await RetrieveCurrentApplicationFromApi();
            
            if (persistedApplication.StageOne.ProductionDetails.Status == TaskStatus.NotStarted.ToString())
            {
                persistedApplication.StageOne.ProductionDetails.Status = TaskStatus.InProgress.ToString();
            }
            
            persistedApplication.StageOne.ProductionDetails.EligibleBiomethane = vm.Value;
            
            await PersistApplicationToApi(persistedApplication);
            
            return RedirectToAction(nameof(StartDate));
        }

        [HttpGet]
        public async Task<IActionResult> StartDate(CancellationToken token)
        {
            _logger.LogInformation("Editing Date Injection Start Date...");
            
            var persistedApplication = await RetrieveCurrentApplicationFromApi();
            
            var vm = new ProductionDetailsStartDateViewModel();

            if (persistedApplication.StageOne.ProductionDetails.InjectionStartDate != DateTime.MinValue)
            {
                vm.StartDate = persistedApplication.StageOne.ProductionDetails.InjectionStartDate;
            }

            return View("DateTimeDetail", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StartDate([FromForm] ProductionDetailsStartDateViewModel viewModel, CancellationToken token)
        {
            _logger.LogInformation("Saving Date Injection Start Date...");

            var vm = new ProductionDetailsStartDateViewModel();
            
            var persistedApplication = await RetrieveCurrentApplicationFromApi();

            if (viewModel.StartDate != null && !DateTimeHelper.IsValidYear((DateTime)viewModel.StartDate))
            {
                ModelState.AddModelError(nameof(viewModel.StartDate), "Year must be 4 digits");
            }

            if (ModelState.IsValid)
            {
                persistedApplication.StageOne.ProductionDetails.InjectionStartDate = (DateTime)viewModel.StartDate;

                await PersistApplicationToApi(persistedApplication);
            
                return RedirectToAction(nameof(CheckAnswers));
            }

            return View("DateTimeDetail", vm);

        }

        [HttpGet]
        public async Task<IActionResult> CheckAnswers()
        {
            var application = await RetrieveCurrentApplicationFromApi();
            var vm = new CheckAnswersViewModel(TaskType.ProductionDetails)
            {
                OrganisationId = CurrentOrganisationId,
                BackAction = "production-details/start-date"
            };
            string queryString = "?returnUrl=check-answers";
            
            vm.Answers.AddRange(new List<Answer>
            {
                new Answer
                {
                    PropertyName = TaskPropertyName.MaximumInitialCapacityOfBiomethane,
                    PropertyValue = $"{application.StageOne.ProductionDetails.MaximumInitialCapacity} m\u00b3",
                    ChangeLink = "/production-details/production-details" + queryString
                },
                new Answer
                {
                    PropertyName = TaskPropertyName.EligibleBiomethane,
                    PropertyValue = $"{application.StageOne.ProductionDetails.EligibleBiomethane} m\u00b3",
                    ChangeLink = "/production-details/eligible-biomethane" + queryString
                },
                new Answer
                {
                    PropertyName = TaskPropertyName.DateInjectionStart,
                    PropertyValue = application.StageOne.ProductionDetails.InjectionStartDate.ToString("dd.MM.yyyy"),
                    ChangeLink = "/production-details/start-date" + queryString

                }
            });

            return View("CheckAnswers", vm);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckAnswers(CancellationToken token)
        {
            var persistedApplication = await RetrieveCurrentApplicationFromApi();
            persistedApplication.StageOne.ProductionDetails.Status = TaskStatus.Completed.ToString();
            await PersistApplicationToApi(persistedApplication);

            return RedirectToAction("Index", "TaskList");
        }

        private async Task<ApplicationValue> RetrieveCurrentApplicationFromApi()
        {
            var response = await _getApplicationService.RetrieveApplication(new GetApplicationRequest()
            {
                ApplicationId = CurrentPersistedApplicationId.ToString(),
            }, CancellationToken.None);

            return response.Application;
        }

        private async Task PersistApplicationToApi(ApplicationValue application)
        {
            await _updateApplicationService.Update(new UpdateApplicationRequest()
            {
                Application = application,
                ApplicationId = CurrentPersistedApplicationId.ToString(),
                
                UserId = UserId.ToString()
            }, CancellationToken.None);
        }
    }
}
