using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ExternalPortal.Constants;
using ExternalPortal.Helpers;
using ExternalPortal.Services;
using ExternalPortal.ViewModels.PlanningPermission;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ofgem.API.GGSS.Domain.ModelValues;

namespace ExternalPortal.Controllers
{
    public class PlanningPermissionController : BaseController
    {
        private readonly ILogger<PlanningPermissionController> _logger;
        private readonly ISaveDocumentService _saveDocumentService;
        private readonly IDeleteDocumentService _deleteDocumentService;
        private readonly IGetApplicationService _getApplicationService;
        private readonly IUpdateApplicationService _updateApplicationService;

        public PlanningPermissionController(
            ILogger<PlanningPermissionController> logger,
            ISaveDocumentService saveDocumentService,
            IDeleteDocumentService deleteDocumentService,
            IGetApplicationService getApplicationService,
            IUpdateApplicationService updateApplicationService,
            IRedisCacheService redisCache) : base(redisCache)
        {
            _logger = logger;
            _saveDocumentService = saveDocumentService;
            _deleteDocumentService = deleteDocumentService;
            _getApplicationService = getApplicationService;
            _updateApplicationService = updateApplicationService;
        }

        [HttpGet]
        [Route(UrlKeys.PlanningPermissionWhatYouWillNeed)]
        public IActionResult WhatYouWillNeed()
        {
            return View(nameof(WhatYouWillNeed));
        }

        [HttpGet]
        [Route(UrlKeys.PlanningPermissionDoYouHave)]
        public IActionResult Index()
        {
            _logger.LogDebug("GET Index action on PlanningPermission controller called");

            return View(nameof(Index), new PlanningPermission());
        }

        [HttpPost]
        [Route(UrlKeys.PlanningPermissionDoYouHave)]
        public async Task<IActionResult> Index(string planningPermission)
        {
            if (IsMissing(planningPermission))
            {
                var model = new PlanningPermission() { Errors = new List<string>() };

                model.Errors.Add("Select an option");

                return View(nameof(Index), model);
            }

            var application = await RetrieveCurrentApplicationFromApi();
            
            if (application.StageOne.ProvidePlanningPermission.Status == Enums.TaskStatus.NotStarted.ToString())
            {
                application.StageOne.ProvidePlanningPermission.Status = Enums.TaskStatus.InProgress.ToString();
            }

            application.StageOne.ProvidePlanningPermission.PlanningPermissionOutcome = planningPermission;
            
            await PersistApplicationToApi(application);

            if (IsOptionNo(planningPermission))
            {
                return RedirectToAction(nameof(PlanningExemptUpload));
            }

            return RedirectToAction(nameof(PlanningUpload));
        }

        [HttpGet]
        [Route(UrlKeys.PlanningPermissionUploadLink)]
        public async Task<IActionResult> PlanningUpload()
        {
            var application = await RetrieveCurrentApplicationFromApi();

            var model = new PlanningPermission()
            {
                FileId = application.StageOne.ProvidePlanningPermission.PlanningPermissionDocument.FileId,
                Filename = application.StageOne.ProvidePlanningPermission.PlanningPermissionDocument.FileName,
                FileSizeAsString = application.StageOne.ProvidePlanningPermission.PlanningPermissionDocument.FileSizeAsString
            };

            return View(nameof(PlanningUpload), model);
        }

        [HttpPost]
        [Route(UrlKeys.PlanningPermissionUploadLink)]
        public async Task<IActionResult> PlanningUpload(IFormFile planningPermissionFile, CancellationToken token)
        {
            var model = new PlanningPermission() { Errors = new List<string>() };

            var response = await _saveDocumentService.Save(planningPermissionFile, token);

            if (response.HasErrors)
            {
                model.Errors.AddRange(response.Errors.Select(e => e.Message).ToList());

                return View(nameof(PlanningUpload), model);
            }

            var application = await RetrieveCurrentApplicationFromApi(token);
            
            application.StageOne.ProvidePlanningPermission.PlanningPermissionDocument = new DocumentValue
            {
                FileId = response.DocumentId,
                FileName = planningPermissionFile.FileName,
                FileSizeAsString = FileSizeHelper.GetFileSizeAsString(planningPermissionFile.Length),
                Tags = DocumentTags.PlanningPermission
            };
            
            await PersistApplicationToApi(application);

            return RedirectToAction(nameof(PlanningUploadConfirm));
        }

        [HttpGet]
        [Route(UrlKeys.PlanningPermissionUploadConfirmLink)]
        public async Task<IActionResult> PlanningUploadConfirm()
        {
            var application = await RetrieveCurrentApplicationFromApi();

            var model = new PlanningPermission()
            {
                FileId = application.StageOne.ProvidePlanningPermission.PlanningPermissionDocument.FileId,
                Filename = application.StageOne.ProvidePlanningPermission.PlanningPermissionDocument.FileName,
                FileSizeAsString = application.StageOne.ProvidePlanningPermission.PlanningPermissionDocument.FileSizeAsString
            };

            return View(nameof(PlanningUploadConfirm), model);
        }

        [HttpPost]
        [Route(UrlKeys.PlanningPermissionUploadConfirmLink)]
        public async Task<IActionResult> PlanningUploadConfirm(string documentAgree, string option)
        {
            var application = await RetrieveCurrentApplicationFromApi();

            var model = new PlanningPermission()
            {
                FileId = application.StageOne.ProvidePlanningPermission.PlanningPermissionDocument.FileId,
                Filename = application.StageOne.ProvidePlanningPermission.PlanningPermissionDocument.FileName,
                FileSizeAsString = application.StageOne.ProvidePlanningPermission.PlanningPermissionDocument.FileSizeAsString,
            Errors = new List<string>()
            };

            if (IsMissing(documentAgree))
            {
                model.Errors.Add("Select an option");

                return View(nameof(PlanningUploadConfirm), model);
            }

            if (IsOptionNo(documentAgree))
            {
                await _deleteDocumentService.Delete(model.FileId, CancellationToken.None);

                return RedirectToAction(nameof(PlanningUpload));
            }

            return RedirectToAction(nameof(CheckAnswers));
        }
        
        [HttpGet]
        [Route(UrlKeys.PlanningExemptUploadLink)]
        public async Task<IActionResult> PlanningExemptUpload()
        {
            var application = await RetrieveCurrentApplicationFromApi();

            var model = new PlanningPermission()
            {
                ExemptionStatement = application.StageOne.ProvidePlanningPermission.PlanningPermissionStatement,
            };

            return View(nameof(PlanningExemptUpload), model);
        }
        
        [HttpPost]
        [Route(UrlKeys.PlanningExemptUploadLink)]
        public async Task<IActionResult> PlanningExemptUpload(PlanningPermission planningPermissionModel, CancellationToken token)
        {
            var model = new PlanningPermission()
            {
                Errors = new List<string>(),
                ExemptionStatement = planningPermissionModel.ExemptionStatement
            };

            if (!ModelState.IsValid)
            {
                return View(nameof(PlanningExemptUpload), model);
            }
            
            var persistedApplication = await RetrieveCurrentApplicationFromApi(token);
            persistedApplication.StageOne.ProvidePlanningPermission.PlanningPermissionStatement = model.ExemptionStatement;

            await PersistApplicationToApi(persistedApplication, token);

            return RedirectToAction(nameof(CheckAnswers));
        }

        [HttpGet]
        [Route(UrlKeys.PlanningPermissionCheckYourAnswers)]
        public async Task<IActionResult> CheckAnswers()
        {
            var application = await RetrieveCurrentApplicationFromApi();

            return View(nameof(CheckAnswers), application);
        }

        [HttpPost]
        [Route(UrlKeys.PlanningPermissionCheckYourAnswers)]
        public async Task<IActionResult> CheckAnswers(CancellationToken token)
        {
            var application = await RetrieveCurrentApplicationFromApi(token);

            application.StageOne.ProvidePlanningPermission.Status = Enums.TaskStatus.Completed.ToString();

            await PersistApplicationToApi(application, token);

            return RedirectToAction("Index", "TaskList");
        }

        private static bool IsMissing(string option)
        {
            return string.IsNullOrWhiteSpace(option);
        }

        private static bool IsOptionNo(string option)
        {
            return option.Equals("No", StringComparison.InvariantCultureIgnoreCase);
        }
        
        private async Task<ApplicationValue> RetrieveCurrentApplicationFromApi(CancellationToken token = default)
        {
            var response = await _getApplicationService.Get(new GetApplicationRequest()
            {
                ApplicationId = CurrentPersistedApplicationId.ToString(),
            }, token);

            return response.Application;
        }

        private async Task PersistApplicationToApi(ApplicationValue application, CancellationToken token = default)
        {
            await _updateApplicationService.Update(new UpdateApplicationRequest()
            {
                Application = application,
                ApplicationId = CurrentPersistedApplicationId.ToString(),
                UserId = UserId.ToString()
            }, token);
        }
    }
}
