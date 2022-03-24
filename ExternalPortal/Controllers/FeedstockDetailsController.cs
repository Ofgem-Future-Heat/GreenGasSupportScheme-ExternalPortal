using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ExternalPortal.Enums;
using ExternalPortal.Services;
using ExternalPortal.ViewModels;
using ExternalPortal.ViewModels.Tasks;
using ExternalPortal.ViewModels.Value;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Ofgem.API.GGSS.Domain.ModelValues;
using TaskStatus = ExternalPortal.Enums.TaskStatus;

namespace ExternalPortal.Controllers
{
    public class FeedstockDetailsController : BaseController
    {
        private readonly string _backRoute = "/feedstock-details/feedstock-supply";

        private readonly IGetApplicationService _getApplicationService;
        private readonly IUpdateApplicationService _updateApplicationService;
        private readonly ISaveDocumentService _saveDocumentService;
        private readonly IDeleteDocumentService _deleteDocumentService;

        private readonly ILogger<TaskListController> _logger;

        public FeedstockDetailsController(IRedisCacheService redisCache, ILogger<TaskListController> logger, IGetApplicationService getApplicationService, IUpdateApplicationService updateApplicationService, ISaveDocumentService saveDocumentService, IDeleteDocumentService deleteDocumentService)
            : base(redisCache)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _getApplicationService = getApplicationService;
            _updateApplicationService = updateApplicationService;
            _saveDocumentService = saveDocumentService;
            _deleteDocumentService = deleteDocumentService;
        }

        [HttpGet]
        public async Task<IActionResult> WhatYouWillNeed(CancellationToken token)
        {
            _logger.LogInformation("Feedstock Details Index called...");
            
            var vm = new TaskRequirementsViewModel(TaskType.FeedstockDetails)
            {
                OrganisationId = CurrentOrganisationId,
                ApplicationId = CurrentApplicationId,
                Requirements = TaskRequirements[TaskType.FeedstockDetails],
                ProceedActionName = nameof(FeedstockSupply),
                BackLink = $"/task-list"
            };

            return await Task.FromResult(View("WhatYouWillNeed", vm));
        }

        [HttpGet]
        public async Task<IActionResult> FeedstockSupply(CancellationToken token)
        {
            _logger.LogInformation("Proceed with Feedstock Details...");

            var application = await RetrieveCurrentApplicationFromApi();
            return OptionsView(application);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FeedstockSupply(SelectListItem item, CancellationToken token)
        {
            _logger.LogInformation($"Saving {item.Value} Feedstock supply selected option...");

            if (item.Value == null)
            {
                var application = await RetrieveCurrentApplicationFromApi();

                return OptionsView(application, "Select an option");
            }

            var persistedApplication = await RetrieveCurrentApplicationFromApi();

            if (persistedApplication.StageThree.FeedstockDetails.Status == TaskStatus.NotStarted.ToString())
            {
                persistedApplication.StageThree.FeedstockDetails.Status = TaskStatus.InProgress.ToString();
            }

            persistedApplication.StageThree.FeedstockDetails.FeedstockPlan = item.Value;

            await PersistApplicationToApi(persistedApplication);

            switch (item.Value)
            {
                case "agreement":
                    return RedirectToAction(nameof(FeedstockUpload));

                case "self-supply":
                    return RedirectToAction(nameof(SelfSupply));
                
                case "none":
                    return RedirectToAction(nameof(CheckAnswers));
            }

            return null;
        }

        [HttpGet]
        public async Task<IActionResult> SelfSupply(CancellationToken token)
        {
            var persistedApplication = await RetrieveCurrentApplicationFromApi();
            return SelfSupplyView(persistedApplication);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SelfSupply(StringViewModel vm ,string value, CancellationToken token)
        {
            var persistedApplication = await RetrieveCurrentApplicationFromApi();

            if (!ModelState.IsValid)
            {
                return SelfSupplyView(persistedApplication);
            }
            
            persistedApplication.StageThree.FeedstockDetails.FeedstockSupplierName = value;
            await PersistApplicationToApi(persistedApplication);
            
            return RedirectToAction(nameof(CheckAnswers));
        }

        [HttpGet]
        public async Task<IActionResult> FeedstockUpload(CancellationToken token)
        {
            return await Task.FromResult(View("FileUpload", new FileUploadViewModel(TaskType.FeedstockDetails, TaskPropertyName.FeedstockPlanDocument)
            {
                SaveActionName = nameof(FeedstockUpload),
                BackAction = _backRoute
            }));
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FeedstockUpload([FromForm] IFormFile file, CancellationToken token)
        {
            var persistedApplication = await RetrieveCurrentApplicationFromApi();

            if (!string.IsNullOrEmpty(persistedApplication.StageThree.FeedstockDetails.FeedStockSupplyDocument.FileId))
            {
                await _deleteDocumentService.Delete(persistedApplication.StageThree.FeedstockDetails.FeedStockSupplyDocument
                    .FileId, token);
            }

            var saveFileResponse = await _saveDocumentService.Save(file, token);

            persistedApplication.StageThree.FeedstockDetails.FeedStockSupplyDocument = new DocumentValue()
            {
                FileId = saveFileResponse.DocumentId,
                FileName = file.FileName
            };

            await PersistApplicationToApi(persistedApplication);

            var model = new StageOneModel()
            {
                FeedstockUploadFileName = saveFileResponse.DocumentId
            };
            
            return View("FeedstockUploadConfirm", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult FeedstockUploadConfirm(string documentAgree)
        {
            if (documentAgree == "No")
            {
                return RedirectToAction(nameof(FeedstockUpload));
            }

            return RedirectToAction(nameof(CheckAnswers));
        }

        [HttpGet]
        public async Task<IActionResult> CheckAnswers()
        {
            var application = await RetrieveCurrentApplicationFromApi();
            var vm = new CheckAnswersViewModel(TaskType.FeedstockDetails);

            var options = new FeedstockOptionsViewModel(nameof(FeedstockSupply));
            options.SetupOptions();

            var selectedOption = application.StageThree.FeedstockDetails.FeedstockPlan;
            vm.Answers.Add(new Answer
            {
                PropertyName = TaskPropertyName.FeedstockPlan,
                PropertyValue = options.Options.SingleOrDefault(o => o.Value == selectedOption)?.Text,
                ChangeLink = _backRoute,
            });

            if (selectedOption == "agreement")
            {
                vm.Answers.Add(
                    new Answer
                    {
                        PropertyName = TaskPropertyName.FeedstockPlanDocument,
                        PropertyValue = application.StageThree.FeedstockDetails.FeedStockSupplyDocument.FileName,
                        ChangeLink = _backRoute
                    }
                );
            }
            
            if (selectedOption == "self-supply")
            {
                vm.Answers.Add(
                    new Answer
                    {
                        PropertyName = TaskPropertyName.FeedstockSupplierName,
                        PropertyValue = application.StageThree.FeedstockDetails.FeedstockSupplierName,
                        ChangeLink = _backRoute
                    }
                );
            }
            
            
            return View("CheckAnswers", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckAnswers(CancellationToken token)
        {
            var persistedApplication = await RetrieveCurrentApplicationFromApi();
            persistedApplication.StageThree.FeedstockDetails.Status = TaskStatus.Completed.ToString();
            await PersistApplicationToApi(persistedApplication);

            return RedirectToAction("Index", "TaskList");
        }
        
        private IActionResult OptionsView(ApplicationValue application, string error = "")
        {
            var options = new FeedstockOptionsViewModel(nameof(FeedstockSupply));
            options.SetupOptions();
            options.SelectedValue = application.StageThree.FeedstockDetails.FeedstockPlan;
            options.Error = error;
            options.BackAction = "/feedstock-details/what-you-will-need";

            return View("OptionsDetail", options);
        }
        
        private IActionResult SelfSupplyView(ApplicationValue persistedApplication)
        {
            return View("StringDetail", new StringViewModel(TaskType.FeedstockDetails, TaskPropertyName.FeedstockPlanDocument)
            {
                Heading = "Name of supplier",
                Description = "Add decription here...",
                Value = persistedApplication.StageThree.FeedstockDetails.FeedstockSupplierName,
                SaveActionName = nameof(SelfSupply),
                BackAction = _backRoute
            });
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
