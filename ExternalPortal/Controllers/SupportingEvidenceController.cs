using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ExternalPortal.Constants;
using ExternalPortal.Helpers;
using ExternalPortal.Services;
using ExternalPortal.ViewModels.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ofgem.API.GGSS.Domain.ModelValues;
using TaskStatus = ExternalPortal.Enums.TaskStatus;

namespace ExternalPortal.Controllers
{
    public class SupportingEvidenceController : ApplicationController
    {
        private readonly ILogger<SupportingEvidenceController> _logger;
        private readonly ISaveDocumentService _saveDocumentService;
        private readonly IDeleteDocumentService _deleteDocumentService;

        public SupportingEvidenceController(
            ILogger<SupportingEvidenceController> logger,
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
        [Route(UrlKeys.SupportingEvidenceWhatYouWillNeed)]
        public IActionResult WhatYouWillNeed()
        {
            _logger.LogInformation("WhatYouWillNeed action called on SupportingEvidence controller");

            return View(nameof(WhatYouWillNeed));
        }

        [HttpGet]
        [Route(UrlKeys.SupportingEvidenceAdd)]
        public async Task<IActionResult> AddSupportingEvidence()
        {
            _logger.LogInformation("AddSupportingEvidence action called on SupportingEvidence controller");

            var application = await RetrieveCurrentApplicationFromApi();

            if (application.StageTwo.AdditionalSupportingEvidence.Status == TaskStatus.NotStarted.ToString())
            {
                application.StageTwo.AdditionalSupportingEvidence.Status = TaskStatus.InProgress.ToString();
            }

            await PersistApplicationToApi(application);

            var model = new AdditionalFinancialEvidenceModel()
            {
                AddEvidence = application.StageTwo.AdditionalSupportingEvidence.AddEvidence
            };

            return View(nameof(AddSupportingEvidence), model);
        }

        [HttpPost]
        [Route(UrlKeys.SupportingEvidenceAdd)]
        public async Task<IActionResult> AddSupportingEvidence(string supportingEvidence, CancellationToken token)
        {
            _logger.LogInformation("AddSupportingEvidence action called on SupportingEvidence controller");

            var application = await RetrieveCurrentApplicationFromApi();

            if (string.IsNullOrWhiteSpace(supportingEvidence))
            {
                var model = new AdditionalFinancialEvidenceModel
                {
                    Error = "Select an option"
                };

                return View(nameof(AddSupportingEvidence), model);
            }

            application.StageTwo.AdditionalSupportingEvidence.AddEvidence = supportingEvidence;

            await PersistApplicationToApi(application);

            if (IsOptionNo(supportingEvidence))
            {
                var documents = application.StageTwo.AdditionalSupportingEvidence.AdditionalSupportingEvidenceDocuments;

                documents?.ForEach(async document => _ = await _deleteDocumentService.Delete(document.FileId, token));

                application.StageTwo.AdditionalSupportingEvidence.AdditionalSupportingEvidenceDocuments = new List<DocumentValue>();

                await PersistApplicationToApi(application);
                
                return RedirectToAction(nameof(CheckYourAnswers));
            }

            return RedirectToAction(nameof(Upload));
        }

        [HttpGet]
        [Route(UrlKeys.SupportingEvidenceUpload)]
        public async Task<IActionResult> Upload()
        {
            _logger.LogInformation("Upload action called on SupportingEvidence controller");

            var application = await RetrieveCurrentApplicationFromApi();

            if (application.StageTwo.AdditionalSupportingEvidence.Status == TaskStatus.NotStarted.ToString())
            {
                application.StageTwo.AdditionalSupportingEvidence.Status = TaskStatus.InProgress.ToString();

                await PersistApplicationToApi(application);
            }

            return View(nameof(Upload), new AdditionalFinancialEvidenceModel());
        }

        [HttpPost]
        [Route(UrlKeys.SupportingEvidenceUpload)]
        public async Task<IActionResult> Upload(IFormFile supportingEvidenceFile, CancellationToken token)
        {
            _logger.LogInformation("Upload action called on SupportingEvidence controller");

            var application = await RetrieveCurrentApplicationFromApi();

            var response = await _saveDocumentService.Save(supportingEvidenceFile, token);

            if (response.HasErrors)
            {
                var model = new AdditionalFinancialEvidenceModel()
                {
                    Error = response.Errors.Select(e => e.Message).First()
                };

                return View(nameof(Upload), model);
            }

            application.StageTwo.AdditionalSupportingEvidence.AdditionalSupportingEvidenceDocuments.Add(new Ofgem.API.GGSS.Domain.ModelValues.DocumentValue()
            {
                FileId = response.DocumentId,
                FileName = supportingEvidenceFile.FileName,
                FileSizeAsString =  FileSizeHelper.GetFileSizeAsString(supportingEvidenceFile.Length),
                Tags = DocumentTags.SupportingEvidence
            });

            await PersistApplicationToApi(application);

            return RedirectToAction(nameof(UploadConfirm));
        }

        [HttpGet]
        [Route(UrlKeys.SupportingEvidenceUploadConfirm)]
        public async Task<IActionResult> UploadConfirm()
        {
            _logger.LogInformation("Upload action called on SupportingEvidence controller");

            var application = await RetrieveCurrentApplicationFromApi();

            var model = GetAdditionalFinancialEvidenceModel(application);

            return View(nameof(UploadConfirm), model);
        }

        [HttpPost]
        [Route(UrlKeys.SupportingEvidenceUploadConfirm)]
        public async Task<IActionResult> UploadConfirm(string option, string documentId, CancellationToken token)
        {
            _logger.LogInformation("UploadConfirm action called on SupportingEvidence controller");

            var application = await RetrieveCurrentApplicationFromApi();

            if (string.IsNullOrWhiteSpace(option))
            {
                var model = GetAdditionalFinancialEvidenceModel(application);

                model.Error = "Select an option";

                return View(nameof(UploadConfirm), model);
            }

            if (IsOptionNo(option))
            {
                await _deleteDocumentService.Delete(documentId, token);

                application.StageTwo.AdditionalSupportingEvidence.AdditionalSupportingEvidenceDocuments
                    .RemoveAll(d => d.FileId == documentId);

                await PersistApplicationToApi(application);

                return RedirectToAction(nameof(Upload));
            }

            return RedirectToAction(nameof(AddReference));
        }

        [HttpGet]
        [Route(UrlKeys.SupportingEvidenceAddReference)]
        public async Task<IActionResult> AddReference()
        {
            _logger.LogInformation("AddReference action called on SupportingEvidence controller");

            var application = await RetrieveCurrentApplicationFromApi();

            var model = GetAdditionalFinancialEvidenceModel(application);

            return View(nameof(AddReference), model);
        }

        [HttpPost]
        [Route(UrlKeys.SupportingEvidenceAddReference)]
        public async Task<IActionResult> AddReference(string reference, string documentId, CancellationToken token)
        {
            _logger.LogInformation("UploadConfirm action called on SupportingEvidence controller");

            var application = await RetrieveCurrentApplicationFromApi();

            if (string.IsNullOrWhiteSpace(reference))
            {
                var model = GetAdditionalFinancialEvidenceModel(application);

                model.Error = "Enter a reference";

                return View(nameof(AddReference), model);
            }

            application.StageTwo.AdditionalSupportingEvidence.AdditionalSupportingEvidenceDocuments
                .First(d => d.FileId == documentId).Reference = reference;

            await PersistApplicationToApi(application);

            return RedirectToAction(nameof(UploadMore));
        }

        [HttpGet]
        [Route(UrlKeys.SupportingEvidenceAddMore)]
        public async Task<IActionResult> UploadMore()
        {
            _logger.LogInformation("UploadMore action called on SupportingEvidence controller");

            var application = await RetrieveCurrentApplicationFromApi();

            var model = GetAdditionalFinancialEvidenceModel(application);

            return View(nameof(UploadMore), model);
        }

        [HttpPost]
        [Route(UrlKeys.SupportingEvidenceAddMore)]
        public async Task<IActionResult> UploadMore(string option, CancellationToken token)
        {
            _logger.LogInformation("UploadMore action called on SupportingEvidence controller");

            if (string.IsNullOrWhiteSpace(option))
            {
                var application = await RetrieveCurrentApplicationFromApi();

                var model = GetAdditionalFinancialEvidenceModel(application);

                model.Error = "Select an option";

                return View(nameof(UploadMore), model);
            }

            if (IsOptionNo(option))
            {
                return RedirectToAction(nameof(CheckYourAnswers));
            }

            return RedirectToAction(nameof(Upload));
        }

        [HttpGet]
        [Route(UrlKeys.SupportingEvidenceCheckYourAnswers)]
        public async Task<IActionResult> CheckYourAnswers()
        {
            var application = await RetrieveCurrentApplicationFromApi();

            var model = GetAdditionalFinancialEvidenceModel(application);

            return View(nameof(CheckYourAnswers), model);
        }

        [HttpGet]
        [HttpPost]
        public async Task<IActionResult> SubmitYourAnswers()
        {
            var application = await RetrieveCurrentApplicationFromApi();

            application.StageTwo.AdditionalSupportingEvidence.Status = TaskStatus.Completed.ToString();

            await PersistApplicationToApi(application);

            return RedirectToAction("Index", "TaskList");
        }

        [HttpGet]
        [Route(UrlKeys.SupportingEvidenceDeleteFullPath)]
        public async Task<IActionResult> DeleteDocument([FromRoute] string containerName, [FromRoute] string blobName)
        {
            _logger.LogInformation("DeleteDocument action called on SupportingEvidence controller");

            var application = await RetrieveCurrentApplicationFromApi();

            var documentId = $"{containerName}/{blobName}";

            var document = application.StageTwo.AdditionalSupportingEvidence.AdditionalSupportingEvidenceDocuments.First(d => d.FileId == documentId);

            TempData["FileName"] = document.FileName;
            TempData["DocumentId"] = document.FileId;

            return View(nameof(DeleteDocument), new AdditionalFinancialEvidenceModel());
        }

        [HttpPost]
        [Route(UrlKeys.SupportingEvidenceDeleteConfirm)]
        public async Task<IActionResult> DeleteConfirm(string option, string documentId)
        {
            _logger.LogInformation("DeleteDocument action called on SupportingEvidence controller");

            if (IsOptionNo(option))
            {
                return RedirectToAction(nameof(UploadMore));
            }

            var application = await RetrieveCurrentApplicationFromApi();

            var model = GetAdditionalFinancialEvidenceModel(application);

            if (string.IsNullOrWhiteSpace(option))
            {
                model.Error = "Select an option";

                return View(nameof(DeleteDocument), model);
            }

            await _deleteDocumentService.Delete(documentId);

            model.RemoveDocumentFor(documentId);

            application.StageTwo.AdditionalSupportingEvidence.AdditionalSupportingEvidenceDocuments
                .RemoveAll(
                d => d.FileId == documentId);

            await PersistApplicationToApi(application);

            if (model.HasDocuments)
            {
                return RedirectToAction(nameof(UploadMore));
            }

            return RedirectToAction(nameof(Upload));
        }

        private static AdditionalFinancialEvidenceModel GetAdditionalFinancialEvidenceModel(ApplicationValue application)
        {
            return new AdditionalFinancialEvidenceModel()
            {
                Documents = application.StageTwo.AdditionalSupportingEvidence.AdditionalSupportingEvidenceDocuments
                            .Select(d => new Models.Document()
                            {
                                Filename = d.FileName,
                                DocumentId = d.FileId,
                                Reference = d.Reference,
                                FileSizeAsString = d.FileSizeAsString
                            })
                            .ToList(),
                AddEvidence = application.StageTwo.AdditionalSupportingEvidence.AddEvidence
            };
        }

        private static bool IsOptionNo(string option)
        {
            return option.Equals("No", StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
