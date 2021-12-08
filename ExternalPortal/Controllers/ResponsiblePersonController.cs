using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ExternalPortal.Constants;
using ExternalPortal.Extensions;
using ExternalPortal.Helpers;
using ExternalPortal.Services;
using ExternalPortal.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Ofgem.API.GGSS.Domain.Models;
using Ofgem.API.GGSS.Domain.ModelValues;

namespace ExternalPortal.Controllers
{
    public class ResponsiblePersonController : BaseController
    {
        private readonly ILogger<ResponsiblePersonController> _logger;
        private readonly IResponsiblePersonService _responsiblePersonService;
        private readonly IGetUserByProviderIdService _getUserByProviderIdService;

        public ResponsiblePersonController(
            ILogger<ResponsiblePersonController> logger,
            IResponsiblePersonService responsiblePersonService,
            IGetUserByProviderIdService getUserByProviderIdService,
            IRedisCacheService redisCache) : base(redisCache)
        {
            _logger = logger;
            _responsiblePersonService = responsiblePersonService;
            _getUserByProviderIdService = getUserByProviderIdService;
        }

        [HttpGet]
        public IActionResult Index(CancellationToken token)
        {
            _logger.LogDebug("GET ResponsiblePersonIndex action on ResponsiblePerson controller called");

            return View(nameof(Index));
        }

        [HttpGet]
        public IActionResult Type()
        {
            _logger.LogDebug("GET ResponsiblePersonType action on ResponsiblePerson controller called");

            var vm = new ResponsiblePersonIndexViewModel { BackAction = UrlKeys.RegisterAnOrganisation };

            return View(nameof(Type), vm);
        }

        [HttpPost]
        public IActionResult Type(ResponsiblePersonIndexViewModel vm)
        {
            _logger.LogDebug("POST ResponsiblePersonType action on ResponsiblePerson controller called");

            if (!ModelState.IsValid)
            {
                vm.BackAction = UrlKeys.RegisterAnOrganisation;
                return View(nameof(Type), vm);
            }

            if (vm.ResponsiblePersonType == ResponsiblePersonType.You)
            {
                return RedirectToAction(nameof(Confirmation), new { responsiblePersonIsYou = true });
            }

            return RedirectToAction(nameof(NotResponsibleKickout));
        }

        [HttpGet]
        public IActionResult NotResponsibleKickout()
        {
            _logger.LogDebug("GET NotResponsibleKickout action on ResponsiblePerson controller called");

            return View(nameof(NotResponsibleKickout));
        }

        [HttpGet]
        public async Task<IActionResult> Confirmation(bool responsiblePersonIsYou, CancellationToken token)
        {
            _logger.LogDebug("GET ResponsiblePersonConfirmation action on ResponsiblePerson controller called");

            var rp = await _responsiblePersonService.GetAsync();

            if (responsiblePersonIsYou)
            {
                rp.User.IsResponsiblePerson = true;
            }

            return await InitiateNewCompanyRegistrationAsync(rp, token);
        }

        [HttpGet]
        public IActionResult LetterOfAuthorityIndex()
        {
            _logger.LogDebug("GET LetterOfAuthorityIndex action on ResponsiblePerson controller called");

            return View("Index");
        }

        [HttpGet]
        public async Task<IActionResult> DateOfBirth(CancellationToken token)
        {
            _logger.LogDebug("GET DateOfBirth action on ResponsiblePerson controller called");

            var responsiblePersonDetailViewModel = new ResponsiblePersonDetailViewModel
            {
                BackAction = "Index",
                BackController = "ResponsiblePerson",
                ReturnUrl = HttpContext.Request.Query["returnUrl"]
            };

            var organisationModel = await RedisCache.GetOrgRegistrationAsync(UserId, token);

            if (organisationModel != null && organisationModel.Model.ResponsiblePeople?.Count > 0)
            {
                var responsiblePerson = organisationModel.Model.ResponsiblePeople.First();

                if (responsiblePerson.Value.DateOfBirth != null)
                {
                    responsiblePersonDetailViewModel.DateOfBirth = Convert.ToDateTime(responsiblePerson.Value.DateOfBirth);
                }
            }

            return View(nameof(DateOfBirth), responsiblePersonDetailViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> DateOfBirth([FromForm] ResponsiblePersonDetailViewModel viewModel, CancellationToken token)
        {
            _logger.LogDebug("POST DateOfBirth action on ResponsiblePerson controller called");

            if (ModelState.GetFieldValidationState(nameof(viewModel.DateOfBirth)) == ModelValidationState.Invalid)
            {
                return View(nameof(DateOfBirth), viewModel);
            }

            if (!DateTimeHelper.IsValidateDateOfBirth((DateTime)viewModel.DateOfBirth))
            {
                ModelState.AddModelError(nameof(viewModel.DateOfBirth), "Minimum age must 18 years");
                return View(nameof(DateOfBirth), viewModel);
            }

            var organisationModel = await RedisCache.GetOrgRegistrationAsync(UserId, token);

            if (organisationModel.Model.ResponsiblePeople?.Count > 0)
            {
                organisationModel.Model.ResponsiblePeople.First().Value.DateOfBirth = viewModel.DateOfBirth.ToString();
            }

            _ = await RedisCache.SaveOrgRegistrationAsync(UserId, "Model.ResponsiblePeople", organisationModel.Model.ResponsiblePeople, token);

            if (!string.IsNullOrEmpty(viewModel.ReturnUrl))
            {
                return RedirectToAction("CheckAnswers", "Organisation");
            }
            return RedirectToAction(nameof(PhoneNumber));
        }

        [HttpGet]
        public async Task<IActionResult> PhoneNumber(CancellationToken token)
        {
            _logger.LogDebug("GET PhoneNumber action on ResponsiblePerson controller called");

            var responsiblePersonDetailViewModel = new ResponsiblePersonDetailViewModel
            {
                BackAction = "DateOfBirth",
                BackController = "ResponsiblePerson",
                ReturnUrl = HttpContext.Request.Query["returnUrl"]
            };
            
            var organisationModel = await RedisCache.GetOrgRegistrationAsync(UserId, token);

            if (organisationModel != null && organisationModel.Model.ResponsiblePeople?.Count > 0)
            {
                var responsiblePerson = organisationModel.Model.ResponsiblePeople.First();

                if (responsiblePerson.Value.TelephoneNumber != null)
                {
                    responsiblePersonDetailViewModel.PhoneNumber = responsiblePerson.Value.TelephoneNumber;
                }
            }

            return View(nameof(PhoneNumber), responsiblePersonDetailViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> PhoneNumber([FromForm] ResponsiblePersonDetailViewModel viewModel, CancellationToken token)
        {
            _logger.LogDebug("POST PhoneNumber action on ResponsiblePerson controller called");

            if (ModelState.GetFieldValidationState(nameof(viewModel.PhoneNumber)) == ModelValidationState.Invalid)
            {
                return View(nameof(PhoneNumber), viewModel);
            }
            
            if (!PhoneNumberHelper.IsValidatePhoneNumber(viewModel.PhoneNumber))
            {
                ModelState.AddModelError(nameof(viewModel.PhoneNumber), "Please enter a valid phone number");
                return View(nameof(PhoneNumber), viewModel);
            }
            
            var organisationModel = await RedisCache.GetOrgRegistrationAsync(UserId, token);

            if (organisationModel.Model.ResponsiblePeople?.Count > 0)
            {
                organisationModel.Model.ResponsiblePeople.First().Value.TelephoneNumber = viewModel.PhoneNumber;
            }
            
            _ = await RedisCache.SaveOrgRegistrationAsync(UserId, "Model.ResponsiblePeople", organisationModel.Model.ResponsiblePeople, token);

            if (!string.IsNullOrEmpty(viewModel.ReturnUrl))
            {
                return RedirectToAction("CheckAnswers", "Organisation");
            }
            
            return RedirectToAction("LetterOfAuthorityUpload", "Organisation");
        }

        private async Task<IActionResult> InitiateNewCompanyRegistrationAsync(ResponsiblePersonModel responsiblePerson, CancellationToken token)
        {
            var initAsync = await RedisCache.GetOrgRegistrationAsync(UserId, token)
                .ContinueWith(async findAsync =>
                {
                    var found = await findAsync;
                    if (found != null) return RedirectToAction("ChooseType", "Organisation");

                    var currentUser = await _getUserByProviderIdService.Get(new GetUserRequest()
                    {
                        ProviderId = UserId.ToString()
                    });
                    
                    responsiblePerson.User.Id = currentUser.UserId;
                    
                    // Add a new registration to redis cache for this user
                    var newOrganisation = new OrganisationModel
                    {
                        ResponsiblePeople = new List<ResponsiblePersonModel>() { responsiblePerson },
                        Value = new OrganisationValue { RegisteredOfficeAddress = new AddressModel() }
                    };

                    var vm = new PortalViewModel<OrganisationModel>(newOrganisation) { Id = UserId };

                    if (!await RedisCache.SaveOrgRegistrationAsync(vm.Id, "", vm, token))
                    {
                        var errorMessage = $"Failed to save new organisation registration to cache for user {UserId}";
                        _logger.LogError(errorMessage);
                        vm.AddError(errorMessage);
                    }

                    return RedirectToAction("ChooseType", "Organisation");
                },
                token);

            return await initAsync;
        }
    }
}
