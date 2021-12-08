using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ExternalPortal.Constants;
using ExternalPortal.Extensions;
using ExternalPortal.Helpers;
using ExternalPortal.Models;
using ExternalPortal.Services;
using ExternalPortal.ViewModels;
using ExternalPortal.ViewModels.Shared;
using ExternalPortal.ViewModels.Shared.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Ofgem.API.GGSS.Domain.Contracts.Services;
using Ofgem.API.GGSS.Domain.ModelValues;
using Ofgem.API.GGSS.DomainModels;
using Ofgem.Azure.Redis.Data.Contracts;
using InstallationModel = ExternalPortal.ViewModels.InstallationModel;
using TaskStatus = ExternalPortal.Enums.TaskStatus;

namespace ExternalPortal.Controllers
{
    public class StageOneController : BaseController
    {
        private readonly ILogger<StageOneController> _logger;
        private readonly ISaveDocumentService _saveDocumentService;
        private readonly IGetApplicationService _getApplicationService;
        private readonly IUpdateApplicationService _updateApplicationService;
        private readonly ISendEmailService _sendEmailService;

        public StageOneController(
            IRedisCacheService redisCache,
            ILogger<StageOneController> logger,
            ISaveDocumentService saveDocumentService,
            IGetApplicationService getApplicationService,
            IUpdateApplicationService updateApplicationService,
            ISendEmailService sendEmailService) : base(redisCache)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _saveDocumentService = saveDocumentService;
            _getApplicationService = getApplicationService;
            _updateApplicationService = updateApplicationService;
            _sendEmailService = sendEmailService;
        }

        [HttpGet]
        [Route(UrlKeys.PlantLocationLink)]
        public async Task<IActionResult> PlantLocation(CancellationToken token)
        {
            _logger.LogDebug("PlantLocation action on StageOneController controller called");

            var persistedApplication = await RetrieveCurrentApplicationFromApi();

            var model = new PlantDetailsModel
            {
                BackAction = UrlKeys.PlantDetailsWhatYouWillNeedLink,
                ReferenceParams = new Dictionary<string, string>()
                {
                    { UrlKeys.ReturnToYourApplicationLink, "/task-list" }
                },
                ReturnUrl = HttpContext.Request.Query["returnUrl"],
                Location = (persistedApplication.StageOne.TellUsAboutYourSite.PlantLocation == null ? Ofgem.API.GGSS.DomainModels.PlantLocation.England : Enum.Parse<PlantLocation>(persistedApplication.StageOne.TellUsAboutYourSite.PlantLocation))
            };

            return View(nameof(PlantLocation), model);
        }

        [HttpPost]
        public async Task<IActionResult> PlantLocation([FromForm] PlantDetailsModel viewModel)
        {
            var model = new PlantDetailsModel();

            if (ModelState.GetFieldValidationState(nameof(viewModel.Location)) == ModelValidationState.Invalid)
            {
                return View(nameof(PlantLocation), model);
            }

            var persistedApplication = await RetrieveCurrentApplicationFromApi();

            var currentTask = persistedApplication.StageOne.TellUsAboutYourSite;
            if (currentTask.Status == TaskStatus.NotStarted.ToString())
            {
                currentTask.Status = TaskStatus.InProgress.ToString();
            }

            currentTask.PlantLocation = viewModel.Location.ToString();
            await PersistApplicationToApi(persistedApplication);

            if (!string.IsNullOrEmpty(viewModel.ReturnUrl))
            {
                return RedirectToAction(nameof(CheckAnswers));
            }
            
            return RedirectToAction(nameof(PlantAddress));
        }

        [HttpGet]
        [Route(UrlKeys.CapacityUploadLink)]
        public async Task<IActionResult> CapacityUpload(PlantDetailsModel viewModel = null)
        {
            var persistedApplication = await RetrieveCurrentApplicationFromApi();

            var model = new PlantDetailsModel
            {
                BackAction = UrlKeys.PlantLocationLink,
                ReferenceParams = new Dictionary<string, string>()
                {
                    { UrlKeys.ReturnToYourApplicationLink, "/task-list" }
                },
                CapacityCheckDocument = persistedApplication.StageOne.TellUsAboutYourSite.CapacityCheckDocument,
                ReturnUrl = viewModel.ReturnUrl
            };

            return View(nameof(CapacityUpload), model);
        }

        [HttpPost]
        public async Task<IActionResult> CapacityUpload([FromForm] IFormFile file, CancellationToken token, PlantDetailsModel viewModel = null)
        {
            var response = await _saveDocumentService.Save(file, token);

            if (response.HasErrors)
            {
                var model = new PlantDetailsModel
                {
                    Error = response.Errors.Select(e => e.Message).First()
                };

                return View(nameof(CapacityUpload), model);
            }

            var persistedApplication = await RetrieveCurrentApplicationFromApi();

            persistedApplication.StageOne.TellUsAboutYourSite.CapacityCheckDocument.FileName = file.FileName;
            persistedApplication.StageOne.TellUsAboutYourSite.CapacityCheckDocument.FileId = response.DocumentId;
            persistedApplication.StageOne.TellUsAboutYourSite.CapacityCheckDocument.FileSizeAsString = FileSizeHelper.GetFileSizeAsString(file.Length);
            persistedApplication.StageOne.TellUsAboutYourSite.CapacityCheckDocument.Tags = DocumentTags.CapacityCheck;

            await PersistApplicationToApi(persistedApplication);

            return RedirectToAction(nameof(CapacityUploadConfirm), viewModel);
        }

        [HttpGet]
        [Route(UrlKeys.CapacityUploadConfirmLink)]
        public async Task<IActionResult> CapacityUploadConfirm(PlantDetailsModel viewModel = null)
        {
            string queryString = string.IsNullOrEmpty(viewModel.ReturnUrl) ? HttpContext.Request.Query["returnUrl"].ToString() : viewModel.ReturnUrl;

            var persistedApplication = await RetrieveCurrentApplicationFromApi();

            var model = new PlantDetailsModel()
            {
                CapacityCheckDocument = new DocumentValue()
                {
                    FileId = persistedApplication.StageOne.TellUsAboutYourSite.CapacityCheckDocument.FileId,
                    FileName = persistedApplication.StageOne.TellUsAboutYourSite.CapacityCheckDocument.FileName,
                    FileSizeAsString = persistedApplication.StageOne.TellUsAboutYourSite.CapacityCheckDocument.FileSizeAsString
                },
                BackAction = UrlKeys.CapacityUploadLink,
                ReferenceParams = new Dictionary<string, string>()
                {
                    { UrlKeys.ReturnToYourApplicationLink, "/task-list" }
                },
                ReturnUrl = queryString
            };

            return View(nameof(CapacityUploadConfirm), model);
        }

        [HttpPost]
        [Route(UrlKeys.CapacityUploadConfirmLink)]
        public async Task<IActionResult> CapacityUploadConfirm(string documentAgree, [FromForm] PlantDetailsModel viewModel)
        {
            var persistedApplication = await RetrieveCurrentApplicationFromApi();

            if (string.IsNullOrWhiteSpace(documentAgree))
            {
                var model = new PlantDetailsModel()
                {
                    CapacityCheckDocument = new DocumentValue()
                    {
                        FileId = persistedApplication.StageOne.TellUsAboutYourSite.CapacityCheckDocument.FileId,
                        FileName = persistedApplication.StageOne.TellUsAboutYourSite.CapacityCheckDocument.FileName,
                        FileSizeAsString = persistedApplication.StageOne.TellUsAboutYourSite.CapacityCheckDocument.FileSizeAsString
                    },
                    BackAction = UrlKeys.CapacityUploadLink,
                    ReferenceParams = new Dictionary<string, string>()
                    {
                        { UrlKeys.ReturnToYourApplicationLink, "/task-list" }
                    },
                    ReturnUrl = viewModel.ReturnUrl
                };

                model.Error = "Select an option";

                return View(nameof(CapacityUploadConfirm), model);
            }

            if (documentAgree == "Yes")
            {
                if (!string.IsNullOrEmpty(viewModel.ReturnUrl))
                {
                    return RedirectToAction(nameof(CheckAnswers));
                }
                
                return RedirectToAction(nameof(EquipmentDescription));
            }

            return RedirectToAction(nameof(CapacityUpload), viewModel);
        }

        [HttpGet]
        [Route(UrlKeys.PlantAddressLink)]
        public async Task<IActionResult> PlantAddress()
        {
            var persistedApplication = await RetrieveCurrentApplicationFromApi();

            var model = new AddressViewModel()
            {
                LineOne = persistedApplication.StageOne.TellUsAboutYourSite.PlantAddress.LineOne, 
                LineTwo = persistedApplication.StageOne.TellUsAboutYourSite.PlantAddress.LineTwo, 
                Town = persistedApplication.StageOne.TellUsAboutYourSite.PlantAddress.Town, 
                County = persistedApplication.StageOne.TellUsAboutYourSite.PlantAddress.County, 
                Postcode = persistedApplication.StageOne.TellUsAboutYourSite.PlantAddress.Postcode,
                ReturnUrl = HttpContext.Request.Query["returnUrl"]
            };
            
            return View(nameof(PlantAddress), model);
        }

        [HttpPost]
        [Route(UrlKeys.PlantAddressLink)]
        public async Task<IActionResult> PlantAddress([FromForm] AddressViewModel model)
        {
            var application = new InstallationModel();

            if (!ModelState.IsValid)
            {
                return View(nameof(PlantAddress), model);
            }

            application.StageOne.PlantDetails.InstallationAddress = model;
            application.StageOne.PlantDetails.BackAction = UrlKeys.PlantAddressLink;

            var persistedApplication = await RetrieveCurrentApplicationFromApi();
            
            persistedApplication.StageOne.TellUsAboutYourSite.PlantAddress.LineOne = model.LineOne;
            persistedApplication.StageOne.TellUsAboutYourSite.PlantAddress.LineTwo = model.LineTwo;
            persistedApplication.StageOne.TellUsAboutYourSite.PlantAddress.Town = model.Town;
            persistedApplication.StageOne.TellUsAboutYourSite.PlantAddress.County = model.County;
            persistedApplication.StageOne.TellUsAboutYourSite.PlantAddress.Postcode = model.Postcode;

            await PersistApplicationToApi(persistedApplication);
            
            if (!string.IsNullOrEmpty(model.ReturnUrl))
            {
                return RedirectToAction(nameof(CheckAnswers));
            }
            
            return RedirectToAction(nameof(InjectionPointAddress));
        }
        
        [HttpGet]
        [Route(UrlKeys.InjectionPointAddressLink)]
        public async Task<IActionResult> InjectionPointAddress()
        {
            var persistedApplication = await RetrieveCurrentApplicationFromApi();

            var model = new AddressViewModel()
            {
                LineOne = persistedApplication.StageOne.TellUsAboutYourSite.InjectionPointAddress.LineOne,
                LineTwo = persistedApplication.StageOne.TellUsAboutYourSite.InjectionPointAddress.LineTwo,
                County = persistedApplication.StageOne.TellUsAboutYourSite.InjectionPointAddress.County,
                Town = persistedApplication.StageOne.TellUsAboutYourSite.InjectionPointAddress.Town,
                Postcode = persistedApplication.StageOne.TellUsAboutYourSite.InjectionPointAddress.Postcode,
                ReturnUrl = HttpContext.Request.Query["returnUrl"]
            };

            return View(nameof(InjectionPointAddress), model);
        }
        
        [HttpPost]
        [Route(UrlKeys.InjectionPointAddressLink)]
        public async Task<IActionResult> InjectionPointAddress(AddressViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(nameof(InjectionPointAddress), model);
            }

            var persistedApplication = await RetrieveCurrentApplicationFromApi();

            persistedApplication.StageOne.TellUsAboutYourSite.InjectionPointAddress.LineOne =
                model.LineOne;
            persistedApplication.StageOne.TellUsAboutYourSite.InjectionPointAddress.LineTwo =
                model.LineTwo;
            persistedApplication.StageOne.TellUsAboutYourSite.InjectionPointAddress.County =
                model.County;
            persistedApplication.StageOne.TellUsAboutYourSite.InjectionPointAddress.Town =
                model.Town;
            persistedApplication.StageOne.TellUsAboutYourSite.InjectionPointAddress.Postcode =
                model.Postcode;
    
            await PersistApplicationToApi(persistedApplication);
            
            if (!string.IsNullOrEmpty(model.ReturnUrl))
            {
                return RedirectToAction(nameof(CheckAnswers));
            }
    
            return RedirectToAction(nameof(CapacityUpload));
        }
        
        [HttpGet]
        [Route(UrlKeys.EquipmentDescriptionLink)]
        public async Task<IActionResult> EquipmentDescription()
        {
            var persistedApplication = await RetrieveCurrentApplicationFromApi();

            var model = new PlantDetailsModel()
            {
                BackAction = UrlKeys.CapacityUploadLink,
                ReferenceParams = new Dictionary<string, string>()
                {
                    { UrlKeys.ReturnToYourApplicationLink, "/task-list" }
                },
                EquipmentDescription = persistedApplication.StageOne.TellUsAboutYourSite.EquipmentDescription
            };

            return View(nameof(EquipmentDescription), model);
        }
        
        [HttpPost]
        [Route(UrlKeys.EquipmentDescriptionLink)]
        public async Task<IActionResult> EquipmentDescription([FromForm] PlantDetailsModel model)
        {
            var application = new InstallationModel();

            if (ModelState.GetFieldValidationState(nameof(model.EquipmentDescription)) == ModelValidationState.Invalid)
            {
                model.BackAction = application.StageOne.PlantDetails.BackAction;
                model.ReferenceParams = new Dictionary<string, string>()
                {
                    {UrlKeys.ReturnToYourApplicationLink, "/task-list"}
                };
                
                return View(nameof(EquipmentDescription), model);
            }

            var persistedApplication = await RetrieveCurrentApplicationFromApi();
            
            persistedApplication.StageOne.TellUsAboutYourSite.EquipmentDescription = model.EquipmentDescription;
            
            await PersistApplicationToApi(persistedApplication);

            return RedirectToAction(nameof(CheckAnswers));
        }
        
        [HttpGet]
        public IActionResult ProductionDetails()
        {
            var model = JsonConvert.DeserializeObject<StageOneModel>(HttpContext.Session.GetString("StageOneModel"));

            return View("ProductionDetails", model);
        }

        [HttpPost]
        public IActionResult ProductionDetails(StageOneModel model)
        {
            var sessionModel = JsonConvert.DeserializeObject<StageOneModel>(HttpContext.Session.GetString("StageOneModel"));
            sessionModel.MaxCapacity = model.MaxCapacity;
            //save
            HttpContext.Session.SetString("StageOneModel", JsonConvert.SerializeObject(sessionModel));
            return RedirectToAction("start-date");
        }

        [HttpGet]
        public IActionResult StartDate()
        {
            var sessionModel = JsonConvert.DeserializeObject<StageOneModel>(HttpContext.Session.GetString("StageOneModel"));
            return View("StartDate", sessionModel);
        }

        [HttpPost]
        public IActionResult StartDate(StageOneModel model)
        {
            var sessionModel = JsonConvert.DeserializeObject<StageOneModel>(HttpContext.Session.GetString("StageOneModel"));
            sessionModel.InjectionStartDay = model.InjectionStartDay;
            sessionModel.InjectionStartMonth = model.InjectionStartMonth;
            sessionModel.InjectionStartYear = model.InjectionStartYear;
            //save
            HttpContext.Session.SetString("StageOneModel", JsonConvert.SerializeObject(sessionModel));
            return RedirectToAction("feedstock-supply");
        }

        [HttpGet]
        [Route(UrlKeys.PlantDetailsCheckAnswersLink)]
        public async Task<IActionResult> CheckAnswers()
        {
            var application = new InstallationModel();
            
            var persistedApplication = await RetrieveCurrentApplicationFromApi();

            application.StageOne.PlantDetails.Location = 
                Enum.Parse<PlantLocation>(persistedApplication.StageOne.TellUsAboutYourSite.PlantLocation);
                
            application.StageOne.PlantDetails.CapacityCheckDocument.FileName = 
                persistedApplication.StageOne.TellUsAboutYourSite.CapacityCheckDocument.FileName;
                
            application.StageOne.PlantDetails.InstallationName = 
                persistedApplication.StageOne.TellUsAboutYourSite.PlantName;
                
            application.StageOne.PlantDetails.InstallationAddress = new AddressViewModel()
                {
                    Name = persistedApplication.StageOne.TellUsAboutYourSite.PlantAddress.Name,
                    LineOne = persistedApplication.StageOne.TellUsAboutYourSite.PlantAddress.LineOne,
                    LineTwo = persistedApplication.StageOne.TellUsAboutYourSite.PlantAddress.LineTwo,
                    County = persistedApplication.StageOne.TellUsAboutYourSite.PlantAddress.County,
                    Town = persistedApplication.StageOne.TellUsAboutYourSite.PlantAddress.Town,
                    Postcode = persistedApplication.StageOne.TellUsAboutYourSite.PlantAddress.Postcode
                };
                
            application.StageOne.PlantDetails.InjectionPointAddress = new AddressViewModel()
                {
                    Name = persistedApplication.StageOne.TellUsAboutYourSite.InjectionPointAddress.Name,
                    LineOne = persistedApplication.StageOne.TellUsAboutYourSite.InjectionPointAddress.LineOne,
                    LineTwo = persistedApplication.StageOne.TellUsAboutYourSite.InjectionPointAddress.LineTwo,
                    County = persistedApplication.StageOne.TellUsAboutYourSite.InjectionPointAddress.County,
                    Town = persistedApplication.StageOne.TellUsAboutYourSite.InjectionPointAddress.Town,
                    Postcode = persistedApplication.StageOne.TellUsAboutYourSite.InjectionPointAddress.Postcode
                };

            application.StageOne.PlantDetails.EquipmentDescription =
                persistedApplication.StageOne.TellUsAboutYourSite.EquipmentDescription;
            
            return View("PlantDetails/CheckAnswers", application);
        }

        [HttpPost]
        public async Task<IActionResult> CheckAnswers(StageOneModel model)
        {
            var persistedApplication = await RetrieveCurrentApplicationFromApi();

            persistedApplication.StageOne.TellUsAboutYourSite.Status = TaskStatus.Completed.ToString();

            await PersistApplicationToApi(persistedApplication);

            return RedirectToAction("confirmation");
        }

        [HttpGet]
        public IActionResult Confirmation()
        {
            return View("Confirmation");
        }

        [HttpGet]
        [Route(UrlKeys.PlantDetailsWhatYouWillNeedLink)]
        public IActionResult PlantDetailsWhatYouWillNeed()
        {
            var model = new PlantDetailsModel
            {
                BackAction = "/task-list",
                ReferenceParams = new Dictionary<string, string>()
                {
                    { UrlKeys.ReturnToYourApplicationLink, "/task-list" }
                }
            };

            return View("PlantDetails/WhatYouWillNeed", model);
        }

        [HttpGet]
        [Route(UrlKeys.PlantNameLink)]
        public async Task<IActionResult> PlantName()
        {
            _logger.LogDebug("PlantName action on StageOneController controller called");
            
            
            var persistedApplication = await RetrieveCurrentApplicationFromApi();

            var model = new PlantDetailsModel
            {
                BackAction = UrlKeys.PlantDetailsWhatYouWillNeedLink,
                ReferenceParams = new Dictionary<string, string>()
                {
                    { UrlKeys.ReturnToYourApplicationLink, "/task-list" }
                },
                ReturnUrl = HttpContext.Request.Query["returnUrl"],
                InstallationName = persistedApplication.StageOne.TellUsAboutYourSite.PlantName
            };

            return View(nameof(PlantName), model);
        }

        [HttpPost]
        [Route(UrlKeys.PlantNameLink)]
        public async Task<IActionResult> PlantName([FromForm] PlantDetailsModel viewModel)
        {
            if (ModelState.GetFieldValidationState(nameof(viewModel.InstallationName)) == ModelValidationState.Invalid)
            {
                return View(nameof(PlantName), viewModel);
            }

            var persistedApplication = await RetrieveCurrentApplicationFromApi();
            persistedApplication.StageOne.TellUsAboutYourSite.PlantName = viewModel.InstallationName;
            await PersistApplicationToApi(persistedApplication);

            if (!string.IsNullOrEmpty(viewModel.ReturnUrl))
            {
                return RedirectToAction(nameof(CheckAnswers));
            }
            
            return RedirectToAction(nameof(PlantLocation));
        }

        [HttpPost]
        [Route(UrlKeys.PlantDetailsCheckAnswersLink)]
        public async Task<IActionResult> PlantDetailsCheckAnswers()
        {
            var persistedApplication = await RetrieveCurrentApplicationFromApi();

            persistedApplication.StageOne.TellUsAboutYourSite.Status = TaskStatus.Completed.ToString();

            await PersistApplicationToApi(persistedApplication);

            return RedirectToAction("Index", "TaskList");
        }

        [HttpGet]
        [Route(UrlKeys.Stage1confirmationLink)]
        public IActionResult Stage1Confirmation()
        {
            var confirmationViewModel = new ConfirmationViewModel
            {
                PageHeading = new PageHeadingViewModel
                {
                    Heading = "Stage 1 complete",
                    SubHeading = "What happens next",
                    SuperHeading = "We'll contact you to confirm your registration, or to ask for more information."
                },
                ConfirmationText = "We've received your application and sent you a confirmation email.",
                BackAction = "/task-list"
            };

            var emailParameter = new EmailParameterBuilder(EmailTemplateIds.StageOneSubmitted, User.GetEmailAddress())
                .AddFullName(User.GetDisplayName())
                .AddApplicationId(CurrentPersistedApplicationId)
                .AddDashboardLink(Request.Scheme, Request.Host, CurrentPersistedApplicationId)
                .Build();

            _sendEmailService.Send(emailParameter, CancellationToken.None);

            return View("~/Views/Shared/Confirmation.cshtml", confirmationViewModel);
        }

        private async Task<ApplicationValue> RetrieveCurrentApplicationFromApi()
        {
            var response = await _getApplicationService.Get(new GetApplicationRequest()
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
