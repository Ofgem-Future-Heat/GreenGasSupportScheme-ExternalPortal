using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ExternalPortal.Constants;
using ExternalPortal.Services;
using ExternalPortal.ViewModels.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ofgem.API.GGSS.Domain.ModelValues;
using Ofgem.API.GGSS.Domain.ModelValues.StageTwo;
using TaskStatus = ExternalPortal.Enums.TaskStatus;

namespace ExternalPortal.Controllers
{
    public class Isae3000AuditController : ApplicationController
    {
        private readonly ILogger<Isae3000AuditController> _logger;
        private readonly ISaveDocumentService _saveDocumentService;
        private readonly IDeleteDocumentService _deleteDocumentService;

        public Isae3000AuditController(
            ILogger<Isae3000AuditController> logger,
            ISaveDocumentService saveDocumentService,
            IDeleteDocumentService deleteDocumentService,
            IGetApplicationService getApplicationService,
            IUpdateApplicationService updateApplicationService) : base(getApplicationService, updateApplicationService)
        {
            _logger = logger;
            _saveDocumentService = saveDocumentService;
            _deleteDocumentService = deleteDocumentService;
        }

        [HttpGet]
        [Route(UrlKeys.Isae3000WhatYouWillNeed)]
        public IActionResult WhatYouWillNeed()
        {
            _logger.LogInformation("Isae index action called on StageTwo controller");

            return View(nameof(WhatYouWillNeed));
        }

        [HttpGet]
        [Route(UrlKeys.Isae3000Upload)]
        public async Task<IActionResult> Upload()
        {
            var application = await RetrieveCurrentApplicationFromApi();

            if (application.StageTwo.Isae3000.Status == TaskStatus.NotStarted.ToString())
            {
                application.StageTwo.Isae3000.Status = TaskStatus.InProgress.ToString();
            }

            await PersistApplicationToApi(application);

            var model = new Isae3000AuditModel()
            {
                DocumentId = application.StageTwo.Isae3000.Document.FileId,
                Filename = application.StageTwo.Isae3000.Document.FileName,
                FileSizeAsString = application.StageTwo.Isae3000.Document.FileSizeAsString
            };

            return View(nameof(Upload), model);
        }

        [HttpPost]
        [Route(UrlKeys.Isae3000Upload)]
        public async Task<IActionResult> Upload(IFormFile isae3000File, CancellationToken token)
        {
            var response = await _saveDocumentService.Save(isae3000File, token);

            if (response.HasErrors)
            {
                return View(nameof(Upload), new Isae3000AuditModel() { Error = response.Errors.Select(e => e.Message).First() });
            }

            var application = await RetrieveCurrentApplicationFromApi();

            application.StageTwo.Isae3000.Document = new DocumentValue
            {
                FileId = response.DocumentId,
                FileName = isae3000File.FileName,
                FileSizeAsString = Helpers.FileSizeHelper.GetFileSizeAsString(isae3000File.Length),
                Tags = DocumentTags.Isae3000Audit
            };

            await PersistApplicationToApi(application);

            return RedirectToAction(nameof(UploadConfirm));
        }

        [HttpGet]
        [Route(UrlKeys.Isae3000UploadConfirm)]
        public async Task<IActionResult> UploadConfirm()
        {
            var application = await RetrieveCurrentApplicationFromApi();

            var model = new Isae3000AuditModel()
            {
                DocumentId = application.StageTwo.Isae3000.Document.FileId,
                Filename = application.StageTwo.Isae3000.Document.FileName,
                FileSizeAsString = application.StageTwo.Isae3000.Document.FileSizeAsString
            };

            return View(nameof(UploadConfirm), model);
        }

        [HttpPost]
        [Route(UrlKeys.Isae3000UploadConfirm)]
        public async Task<IActionResult> UploadConfirm(string option, CancellationToken token)
        {
            var application = await RetrieveCurrentApplicationFromApi();

            if (IsMissing(option))
            {
                var model = new Isae3000AuditModel()
                {
                    DocumentId = application.StageTwo.Isae3000.Document.FileId,
                    Filename = application.StageTwo.Isae3000.Document.FileName,
                    FileSizeAsString = application.StageTwo.Isae3000.Document.FileSizeAsString,
                    Error = "Select an option"
                };

                return View(nameof(UploadConfirm), model);
            }

            if (IsOptionNo(option))
            {
                await _deleteDocumentService.Delete(application.StageTwo.Isae3000.Document.FileId, token);

                return RedirectToAction(nameof(Upload));
            }

            return RedirectToAction(nameof(CheckAnswers));
        }

        [HttpGet]
        [Route(UrlKeys.Isae3000CheckYourAnswers)]
        public async Task<IActionResult> CheckAnswers()
        {
            var application = await RetrieveCurrentApplicationFromApi();

            var model = new Isae3000AuditModel()
            {
                DocumentId = application.StageTwo.Isae3000.Document.FileId,
                Filename = application.StageTwo.Isae3000.Document.FileName,
                FileSizeAsString = application.StageTwo.Isae3000.Document.FileSizeAsString
            };

            return View(nameof(CheckAnswers), model);
        }

        [HttpPost]
        public async Task<IActionResult> SubmitYourAnswers()
        {
            var application = await RetrieveCurrentApplicationFromApi();

            application.StageTwo.Isae3000.Status = TaskStatus.Completed.ToString();

            await PersistApplicationToApi(application);

            return RedirectToAction("Index", "TaskList");
        }

        private static bool IsMissing(string option)
        {
            return string.IsNullOrEmpty(option);
        }

        private static bool IsOptionNo(string option)
        {
            return option.Equals("No", StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
