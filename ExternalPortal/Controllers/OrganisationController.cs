using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ExternalPortal.Constants;
using ExternalPortal.Extensions;
using ExternalPortal.Helpers;
using ExternalPortal.Models;
using ExternalPortal.Services;
using ExternalPortal.ViewModels;
using ExternalPortal.ViewModels.Organisation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ofgem.API.GGSS.Domain.Commands.Organisations;
using Ofgem.API.GGSS.Domain.Enums;
using Ofgem.API.GGSS.Domain.Models;
using Ofgem.API.GGSS.Domain.ModelValues;
using Ofgem.GovUK.Notify.Client;

namespace ExternalPortal.Controllers
{
    public class OrganisationController : BaseController
    {
        private readonly ILogger<OrganisationController> _logger;
        private readonly IGetCompaniesHouseService _getCompaniesHouseService;
        private readonly IDeleteDocumentService _deleteDocumentService;
        private readonly ISaveDocumentService _saveDocumentService;
        private readonly IOrganisationService _organisationService;
        private readonly ISendEmailService _sendEmailService;
        private readonly IGetUserByProviderIdService _getUserByProviderIdService;
        private readonly IOptions<SendEmailConfig> _sendEmailConfig;

        public OrganisationController(ILogger<OrganisationController> logger,
            IGetCompaniesHouseService getCompaniesHouseService,
            IRedisCacheService redisCache,
            IDeleteDocumentService deleteDocumentService,
            ISaveDocumentService saveDocumentService,
            IOrganisationService organisationService,
            ISendEmailService sendEmailService,
            IGetUserByProviderIdService getUserByProviderIdService,
            IOptions<SendEmailConfig> sendEmailConfig) : base(redisCache)
        {
            _logger = logger;
            _getCompaniesHouseService = getCompaniesHouseService;
            _deleteDocumentService = deleteDocumentService;
            _saveDocumentService = saveDocumentService;
            _organisationService = organisationService;
            _sendEmailService = sendEmailService;
            _getUserByProviderIdService = getUserByProviderIdService;
            _sendEmailConfig = sendEmailConfig;
        }

        [Route(UrlKeys.RegisterAnOrganisation)]
        public IActionResult Index()
        {
            _logger.LogDebug("Index action on Organisation controller called");

            return View("Index");
        }

        [AllowAnonymous]
        [Route(UrlKeys.RegisterBeforeYouApply)]
        public IActionResult BeforeYouApply()
        {
            _logger.LogDebug("BeforeYouApply action on Organisation controller called");

            return View();
        }

        #region Organisation Type

        [HttpGet]
        public async Task<IActionResult> ChooseType(CancellationToken token)
        {
            var cacheModel = await RedisCache.GetOrgRegistrationAsync(UserId, token);
            cacheModel.ReturnUrl = HttpContext.Request.Query["change"];
            return View(nameof(ChooseType), cacheModel);
        }

        [HttpPost]
        public async Task<IActionResult> ChooseType(PortalViewModel<OrganisationModel> viewModel, CancellationToken token)
        {
            if (viewModel.Model.Value.Type == null)
            {
                return RedirectToAction(nameof(Start), viewModel);
            }

            _ = await RedisCache.SaveOrgRegistrationAsync(UserId, "Model.Value.Type", (OrganisationType)viewModel.Model.Value.Type, token);

            return viewModel.Model.Value.Type == OrganisationType.Private ?
                 RedirectToAction(nameof(Start), new PortalViewModel<OrganisationModel>{ReturnUrl = viewModel.ReturnUrl}) :
                 RedirectToAction("EnterOrgDetails", new PortalViewModel<OrganisationModel>{ReturnUrl = viewModel.ReturnUrl});
        }

        #endregion Organisation Type

        #region Companies House Lookup

        [HttpGet]
        public async Task<IActionResult> Start(PortalViewModel<OrganisationModel> viewModel)
        {
            var model = await RedisCache.GetOrgRegistrationAsync(UserId, CancellationToken.None);

            if (model == null)
            {
                return RedirectToAction(nameof(Index));
            }

            model.ReturnUrl = string.IsNullOrEmpty(viewModel.ReturnUrl) ? HttpContext.Request.Query["returnUrl"].ToString() : viewModel.ReturnUrl;

            return View("Start", model);
        }

        [HttpPost]
        public async Task<IActionResult> Start(PortalViewModel<OrganisationModel> viewModel, CancellationToken token)
        {
            var registrationNumber = viewModel?.Model?.Value?.RegistrationNumber;

            if (string.IsNullOrWhiteSpace(registrationNumber))
            {
                if (viewModel == null)
                {
                    viewModel = new PortalViewModel<OrganisationModel>();
                }

                viewModel.AddError("Enter your company registration number");

                return View("Start", viewModel);
            }

            var response = await _getCompaniesHouseService.GetCompanyDetailsAsync(registrationNumber);

            if (response.Errors.Any())
            {
                viewModel.AddError(response.Errors.First());

                return View("Start", viewModel);
            }

            var organisationModel = await RedisCache.GetOrgRegistrationAsync(UserId, token);

            organisationModel.Model.Value.RegistrationNumber = response.Model.Value.RegistrationNumber;
            organisationModel.Model.Value.Name = response.Model.Value.Name;
            organisationModel.Model.Value.RegisteredOfficeAddress = response.Model.Value.RegisteredOfficeAddress;

            _ = await RedisCache.SaveOrgRegistrationAsync(UserId, "Model", organisationModel.Model, token);

            return RedirectToAction(nameof(Confirm), new PortalViewModel<OrganisationModel>{ReturnUrl = viewModel.ReturnUrl});
        }
        
        [HttpGet]
        public async Task<IActionResult> Confirm(PortalViewModel<OrganisationModel> viewModel = null)
        {
            var model = await RedisCache.GetOrgRegistrationAsync(UserId, CancellationToken.None);

            model.ReturnUrl = viewModel.ReturnUrl;

            return View("Confirm", model);
        }

        [HttpPost]
        public async Task<IActionResult> Confirm(PortalViewModel<OrganisationModel> viewModel, string submit, CancellationToken token)
        {
            if (submit == null)
            {
                viewModel.AddError("Select an option");

                var organisationModel = await RedisCache.GetOrgRegistrationAsync(UserId, token);

                viewModel.Model = organisationModel.Model;

                return View(nameof(Confirm), viewModel);
            }

            if (submit == "cancel")
            {
                return RedirectToAction(nameof(Start), viewModel);
            }

            if (!string.IsNullOrEmpty(viewModel.ReturnUrl))
            {
                return RedirectToAction(nameof(CheckAnswers));
            }
            
            return RedirectToAction("Index", "ResponsiblePerson");
        }
        
        #endregion Companies House Lookup

        #region Organisation details

        [HttpGet]
        public async Task<IActionResult> EnterOrgDetails(CancellationToken token, PortalViewModel<OrganisationModel> viewModel = null)
        {
            var vm = await RedisCache.GetOrgRegistrationAsync(UserId, token);
            string queryString = string.IsNullOrEmpty(viewModel.ReturnUrl) ? HttpContext.Request.Query["returnUrl"].ToString() : viewModel.ReturnUrl;

            return View(nameof(EnterOrgDetails), new EnterOrgDetailsViewModel
            {
                Name = vm.Model.Value.Name,
                LineOne = vm.Model.Value.RegisteredOfficeAddress.LineOne,
                LineTwo = vm.Model.Value.RegisteredOfficeAddress.LineTwo,
                Town = vm.Model.Value.RegisteredOfficeAddress.Town,
                County = vm.Model.Value.RegisteredOfficeAddress.County,
                Postcode = vm.Model.Value.RegisteredOfficeAddress.Postcode,
                Type = vm.Model.Value.Type,
                ReturnUrl = queryString
            });
        }

        [HttpPost]
        public async Task<IActionResult> EnterOrgDetails(EnterOrgDetailsViewModel viewModel, CancellationToken token)
        {
            if (ModelState.IsValid)
            {
                return await SaveOrganisationAsync(viewModel, token);
            }
                
            return View(nameof(EnterOrgDetails), viewModel);
        }

        #endregion Organisation details

        [HttpGet]
        [Route(UrlKeys.LetterOfAuthorisation)]
        public async Task<IActionResult> LetterOfAuthorityUpload(CancellationToken token, PortalViewModel<OrganisationModel> viewModel = null)
        {
            var model = await RedisCache.GetOrgRegistrationAsync(UserId, token);

            string queryString = string.IsNullOrEmpty(viewModel.ReturnUrl) ? HttpContext.Request.Query["returnUrl"].ToString() : viewModel.ReturnUrl;
            
            if (model == null)
            {
                return View(nameof(Start));
            }

            model.ReturnUrl = queryString;

            return View(nameof(LetterOfAuthorityUpload), model);
        }

        [HttpPost]
        [Route(UrlKeys.LetterOfAuthorisation)]
        public async Task<IActionResult> LetterOfAuthorityUpload(IFormFile letterOfAuthorisation, CancellationToken token, [FromForm] PortalViewModel<OrganisationModel> viewModel = null)
        {
            var model = await RedisCache.GetOrgRegistrationAsync(UserId, token);
            
            var response = await _saveDocumentService.Save(letterOfAuthorisation, token);

            if (response.HasErrors)
            {
                model.Model.Value.Error = response.Errors.Select(e => e.Message).First();
                model.AddError(model.Model.Value.Error);
                return View(nameof(LetterOfAuthorityUpload), model);
            }

            model.Model.Value.LetterOfAuthorisation = new DocumentValue
            {
                FileName = letterOfAuthorisation.FileName,
                FileId = response.DocumentId,
                FileSizeAsString = FileSizeHelper.GetFileSizeAsString(letterOfAuthorisation.Length),
                Tags = DocumentTags.LetterOfAuthority
            };

            _ = await RedisCache.SaveOrgRegistrationAsync(UserId, "Model.Value", model.Model.Value, token);

            return RedirectToAction(nameof(LetterOfAuthorityUploadConfirm), viewModel);
        }

        [HttpGet]
        [Route(UrlKeys.LetterOfAuthorisationConfirm)]
        public async Task<IActionResult> LetterOfAuthorityUploadConfirm(CancellationToken token, PortalViewModel<OrganisationModel> viewModel = null)
        {
            var model = await RedisCache.GetOrgRegistrationAsync(UserId, token);
            model.ReturnUrl = viewModel.ReturnUrl;
            return View("LetterOfAuthorityUploadConfirm", model);
        }

        [HttpPost]
        [Route(UrlKeys.LetterOfAuthorisationConfirm)]
        public async Task<IActionResult> LetterOfAuthorityUploadConfirm(string documentAgree, CancellationToken token, [FromForm] PortalViewModel<OrganisationModel> viewModel = null)
        {
            var model = await RedisCache.GetOrgRegistrationAsync(UserId, token);
            if (string.IsNullOrEmpty(documentAgree))
            {
                model.Model.Value.Error = "Select an option";

                return View("LetterOfAuthorityUploadConfirm", model);
            }

            if (documentAgree == "No")
            {
                await _deleteDocumentService.Delete(model.Model.Value.LetterOfAuthorisation.FileId);

                return RedirectToAction("LetterOfAuthorityUpload", viewModel);
            }

            if (!string.IsNullOrEmpty(viewModel.ReturnUrl))
            {
                return RedirectToAction(nameof(CheckAnswers));
            }
            return RedirectToAction("PhotoIdUpload");
        }

        [HttpGet]
        public async Task<IActionResult> LegalDocUpload(CancellationToken token, PortalViewModel<OrganisationModel> viewModel = null)
        {
            var model = await RedisCache.GetOrgRegistrationAsync(UserId, token);
            
            var queryString = string.IsNullOrEmpty(viewModel?.ReturnUrl) ? HttpContext.Request.Query["returnUrl"].ToString() : viewModel.ReturnUrl;

            if (model == null)
            {
                return null;
            }
            
            model.ReturnUrl = queryString;

            return View("LegalDocUpload", model);
        }

        [HttpPost]
        public async Task<IActionResult> LegalDocUpload([FromForm] IFormFile legalDocument, CancellationToken token, PortalViewModel<OrganisationModel> viewModel = null)
        {
            var model = await RedisCache.GetOrgRegistrationAsync(UserId, token);

            var response = await _saveDocumentService.Save(legalDocument, token);

            if (response.HasErrors)
            {
                model.Model.Value.Error = response.Errors.Select(e => e.Message).First();
                model.AddError(model.Model.Value.Error);
                return View(nameof(LegalDocUpload), model);
            }

            model.Model.Value.LegalDocument = new DocumentValue
            {
                FileName = legalDocument.FileName,
                FileId = response.DocumentId,
                FileSizeAsString = FileSizeHelper.GetFileSizeAsString(legalDocument.Length),
                Tags = DocumentTags.LegalDocument
            };

            _ = await RedisCache.SaveOrgRegistrationAsync(UserId, "Model.Value", model.Model.Value, token);

            return RedirectToAction(nameof(LegalDocUploadConfirm), viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> LegalDocUploadConfirm(CancellationToken token, PortalViewModel<OrganisationModel> viewModel = null)
        {
            var model = await RedisCache.GetOrgRegistrationAsync(UserId, token);
            model.ReturnUrl = viewModel.ReturnUrl;
            return View("legalDocUploadConfirm", model);
        }

        [HttpPost]
        public async Task<IActionResult> LegalDocUploadConfirm(string documentAgree, CancellationToken token, [FromForm] PortalViewModel<OrganisationModel> viewModel = null)
        {
            var model = await RedisCache.GetOrgRegistrationAsync(UserId, token);

            if (documentAgree == "Yes")
            {
                if (!string.IsNullOrEmpty(viewModel.ReturnUrl))
                {
                    return RedirectToAction(nameof(CheckAnswers));
                }
                return RedirectToAction("Index", "ResponsiblePerson");
            }

            if (documentAgree == "No")
            {
                return RedirectToAction("LegalDocUpload", viewModel);
            }

            model.Model.Value.Error = "Select an option";
            return View("LegalDocUploadConfirm", model);
        }

        [HttpGet]
        public async Task<IActionResult> PhotoIdUpload(CancellationToken token, PortalViewModel<OrganisationModel> viewModel = null)
        {
            var model = await RedisCache.GetOrgRegistrationAsync(UserId, token);
            
            string queryString = string.IsNullOrEmpty(viewModel.ReturnUrl) ? HttpContext.Request.Query["returnUrl"].ToString() : viewModel.ReturnUrl;
            
            if (model == null)
            {
                return null;
            }

            model.ReturnUrl = queryString;

            return View("PhotoIdUpload", model);
        }

        [HttpPost]
        public async Task<IActionResult> PhotoIdUpload([FromForm] IFormFile file, CancellationToken token, PortalViewModel<OrganisationModel> viewModel = null)
        {
            var model = await RedisCache.GetOrgRegistrationAsync(UserId, token);

            var response = await _saveDocumentService.Save(file, token);

            if (response.HasErrors)
            {
                model.Model.Value.Error = response.Errors.Select(e => e.Message).First();
                model.AddError(model.Model.Value.Error);
                return View(nameof(PhotoIdUpload), model);
            }

            model.Model.Value.PhotoId = new DocumentValue
            {
                FileName = file.FileName,
                FileId = response.DocumentId,
                FileSizeAsString = FileSizeHelper.GetFileSizeAsString(file.Length),
                Tags = DocumentTags.PhotoIdUpload
            };

            _ = await RedisCache.SaveOrgRegistrationAsync(UserId, "Model.Value", model.Model.Value, token);

            return RedirectToAction(nameof(PhotoIdUploadConfirm), viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> PhotoIdUploadConfirm(CancellationToken token, PortalViewModel<OrganisationModel> viewModel = null)
        {
            var model = await RedisCache.GetOrgRegistrationAsync(UserId, token);
            model.ReturnUrl = viewModel.ReturnUrl;
            return View("PhotoIdUploadConfirm", model);
        }

        [HttpPost]
        public async Task<IActionResult> PhotoIdUploadConfirm(string documentAgree, CancellationToken token, [FromForm] PortalViewModel<OrganisationModel> viewModel = null)
        {
            var model = await RedisCache.GetOrgRegistrationAsync(UserId, token);

            if (string.IsNullOrWhiteSpace(documentAgree))
            {
                model.Model.Value.Error = "Select an option";

                return View("PhotoIdUploadConfirm", model);
            }

            if (documentAgree == "No")
            {
                await _deleteDocumentService.Delete(model.Model.Value.PhotoId.FileId);

                return RedirectToAction("PhotoIdUpload", viewModel);
            }
            
            if (!string.IsNullOrEmpty(viewModel.ReturnUrl))
            {
                return RedirectToAction(nameof(CheckAnswers));
            }

            return RedirectToAction("ProofOfAddressUpload");
        }

        [HttpGet]
        public async Task<IActionResult> ProofOfAddressUpload(CancellationToken token)
        {
            var model = await RedisCache.GetOrgRegistrationAsync(UserId, token);

            if (model == null)
            {
                return null;
            }

            return View("ProofOfAddressUpload", model);
        }

        [HttpPost]
        public async Task<IActionResult> ProofOfAddressUpload([FromForm] IFormFile file, CancellationToken token)
        {
            var model = await RedisCache.GetOrgRegistrationAsync(UserId, token);

            var response = await _saveDocumentService.Save(file, token);

            if (response.HasErrors)
            {
                model.Model.Value.Error = response.Errors.Select(e => e.Message).First();
                model.AddError(model.Model.Value.Error);
                return View(nameof(ProofOfAddressUpload), model);
            }

            model.Model.Value.ProofOfAddress = new DocumentValue
            {
                FileName = file.FileName,
                FileId = response.DocumentId,
                FileSizeAsString = FileSizeHelper.GetFileSizeAsString(file.Length),
                Tags = DocumentTags.ProofOfAddress
            };

            _ = await RedisCache.SaveOrgRegistrationAsync(UserId, "Model.Value", model.Model.Value, token);

            return RedirectToAction(nameof(ProofOfAddressUploadConfirm));
        }

        [HttpGet]
        public async Task<IActionResult> ProofOfAddressUploadConfirm(CancellationToken token)
        {
            var model = await RedisCache.GetOrgRegistrationAsync(UserId, token);

            return View("ProofOfAddressUploadConfirm", model);
        }

        [HttpPost]
        public async Task<IActionResult> ProofOfAddressUploadConfirm(string documentAgree, CancellationToken token)
        {
            var model = await RedisCache.GetOrgRegistrationAsync(UserId, token);

            if (string.IsNullOrWhiteSpace(documentAgree))
            {
                model.Model.Value.Error = "Select an option";

                return View("ProofOfAddressUploadConfirm", model);
            }

            if (documentAgree == "No")
            {
                await _deleteDocumentService.Delete(model.Model.Value.ProofOfAddress.FileId);

                return RedirectToAction("ProofOfAddressUpload");
            }

            return RedirectToAction("CheckAnswers");
        }

        [HttpGet]
        public async Task<IActionResult> CheckAnswers()
        {
            var viewModel = await RedisCache.GetOrgRegistrationAsync(UserId, CancellationToken.None);
            if (viewModel != null) return View(nameof(CheckAnswers), viewModel);
            return RedirectToAction("Index", "Dashboard");
        }

        [HttpPost]
        public async Task<IActionResult> CheckAnswers(CancellationToken token)
        {
            var viewModel = await RedisCache.GetOrgRegistrationAsync(UserId, token);

            viewModel.Model.Value.ReferenceNumber = ReferenceNumber.GetNext();

            var findUserResult = await _getUserByProviderIdService.Get(new GetUserRequest()
            {
                ProviderId = UserId.ToString()
            });

            viewModel.Model.ResponsiblePeople.First().User.Id = findUserResult.UserId;

            var command = new OrganisationSave()
            {
                UserId = findUserResult.UserId,
                Model = viewModel.Model
            };

            viewModel.Model.Id = await _organisationService.AddOrganisationWithResponsiblePersonAsync(command, token);

            var externalEmailParameter = GetEmailParameter(User.GetEmailAddress(), viewModel.Model.Value.ReferenceNumber).Build();

            var internalEmailParameter = GetEmailParameter(_sendEmailConfig.Value.InternalEmail, viewModel.Model.Value.ReferenceNumber).Build();

            await _sendEmailService.Send(externalEmailParameter, token);

            await _sendEmailService.Send(internalEmailParameter, token);

            return View("Submitted", viewModel);
        }

        private async Task<IActionResult> SaveOrganisationAsync(EnterOrgDetailsViewModel organisation, CancellationToken token)
        {
            var vm = await RedisCache.GetOrgRegistrationAsync(UserId, token);

            vm.Model.Value.Name = organisation.Name;
            vm.Model.Value.RegisteredOfficeAddress.LineOne = organisation.LineOne;
            vm.Model.Value.RegisteredOfficeAddress.LineTwo = organisation.LineTwo;
            vm.Model.Value.RegisteredOfficeAddress.Town = organisation.Town;
            vm.Model.Value.RegisteredOfficeAddress.County = organisation.County;
            vm.Model.Value.RegisteredOfficeAddress.Postcode = organisation.Postcode;
            vm.Model.Value.Type = organisation.Type;

            if (await RedisCache.SaveOrgRegistrationAsync(UserId, "Model.Value", vm.Model.Value, token))
            {
                if (organisation.ReturnUrl == "check-answers")
                {
                    return RedirectToAction(nameof(CheckAnswers));
                }
                return RedirectToAction("LegalDocUpload", organisation);
            }
            return RedirectToAction(nameof(EnterOrgDetails));
        }

        private EmailParameterBuilder GetEmailParameter(string toEmailAddress, string referenceNumber)
        {
            return new EmailParameterBuilder(EmailTemplateIds.OrganisationSubmitted, toEmailAddress, _sendEmailConfig.Value.ReplyToId)
                .AddFullName(User.GetDisplayName())
                .AddDashboardLink(Request.Scheme, Request.Host, CurrentPersistedApplicationId.ToString())
                .AddCustom("ReferenceNumber", referenceNumber);
        }
    }
}
