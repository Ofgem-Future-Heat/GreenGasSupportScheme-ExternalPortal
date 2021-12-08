using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ExternalPortal.Constants;
using ExternalPortal.Extensions;
using ExternalPortal.Services;
using ExternalPortal.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ofgem.API.GGSS.Domain.Contracts.Services;
using Ofgem.API.GGSS.Domain.Models;
using ExternalPortal.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.TagHelpers.Cache;
using Ofgem.API.GGSS.Domain.Enums;

namespace ExternalPortal.Controllers
{
    public class DashboardController : BaseController
    {
        private readonly ILogger<DashboardController> _logger;
        private readonly IGetOrganisationService _getOrganisationService;
        private readonly ICreateApplicationService _createApplicationService;
        private readonly IGetOrganisationsForUserService _getOrganisationsForUserService;
        private readonly IGetUserByProviderIdService _getUserByProviderIdService;
        private readonly IAddUserService _addUserService;
        private readonly IGetOrganisationDetailsService _getOrganisationDetailsService;

        public DashboardController(
            ILogger<DashboardController> logger,
            ICreateApplicationService createApplicationService,
            IRedisCacheService redisCache,
            IGetOrganisationService getOrganisationService,
            IGetOrganisationDetailsService getOrganisationDetailsService,
            IGetOrganisationsForUserService getOrganisationsForUserService,
            IGetUserByProviderIdService getUserByProviderIdService,
            IAddUserService addUserService) : base(redisCache)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _getOrganisationService = getOrganisationService;
            _getOrganisationDetailsService = getOrganisationDetailsService;
            _getOrganisationsForUserService = getOrganisationsForUserService;
            _getUserByProviderIdService = getUserByProviderIdService;
            _addUserService = addUserService;
            _createApplicationService = createApplicationService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken token)

        {
            _logger.LogDebug("Index action on Dashboard controller called");

            var user = await _getUserByProviderIdService.Get(new GetUserRequest()
            {
                ProviderId = UserId.ToString()
            });

            // This if statement (and it's contents) can be safely removed once all environments create account links
            // have been updated to request a form_post response from adb2c
            if (!user.Found)
            {
                var profile = new UserProfile(User);
                var addUserResponse = await _addUserService.Add(new AddUserRequest()
                {
                    Email = profile.Email,
                    Name = profile.FirstName,
                    Surname = profile.LastName,
                    ProviderId = profile.Id.ToString()
                }, token);
                user.UserId = addUserResponse.UserId;
            }
            
            var organisationList = await GetOrganisationDashboardAsync(user.UserId);

            var dashboardViewModel = new DashboardViewModel
            {
                UserDisplayName = UserDisplayName,
                Organisations = organisationList
            };

            return await Task.FromResult(View(nameof(Index), dashboardViewModel));
        }
        
        [HttpGet]
        public async Task<IActionResult> YourOrganisations()
        {
            var organisation = await RetrieveOrganisationFromApi();
            
            var model = new OrganisationApplicationsViewModel()
            {
                Name = organisation.Name,
                OrganisationId = Request.Query["OrganisationId"], 
                Applications = organisation.Applications.Select(a => new OrganisationApplicationsViewModel.Application()
                {
                    Id = a.Id,
                    Name = a.Name,
                    Status = Enum.Parse<ApplicationStatus>(a.Status).GetDisplayName()
                }).ToList()
            };

            return View(nameof(YourOrganisations), model);
        }
        
        [HttpPost]
        public async Task<IActionResult> StartNewApplication(CancellationToken token)
        {
            var createApplicationResponse = await _createApplicationService.Create(new CreateApplicationRequest()
            {
                OrganisationId = Request.Query["OrganisationId"], 
                UserId = UserId.ToString(),
            }, token);

            CurrentPersistedApplicationId = Guid.Parse(createApplicationResponse.ApplicationId);

            return Redirect("/task-list");
        }

        // TODO: Reenable new user functionality
        // [HttpGet]
        // public async Task<IActionResult> OrganisationDetails()
        // {
        //     var organisationId = HttpContext.Request.Query["organisationId"];
        //     
        //     var organisationDetails = await _getOrganisationDetailsService.Get(new GetOrganisationDetailsRequest()
        //     {
        //         OrganisationId = organisationId
        //     }, CancellationToken.None);
        //
        //     var model = new OrganisationDetailsViewModel()
        //     {
        //         OrganisationId = organisationId,
        //         OrganisationType = organisationDetails.OrganisationType,
        //         OrganisationName = organisationDetails.OrganisationName,
        //         OrganisationAddress = organisationDetails.OrganisationAddress,
        //         ResponsiblePersonName = organisationDetails.ResponsiblePersonName,
        //         ResponsiblePersonEmail = organisationDetails.ResponsiblePersonEmail,
        //         PhotoId = organisationDetails.PhotoId,
        //         ProofOfAddress = organisationDetails.ProofOfAddress,
        //         LetterOfAuthority = organisationDetails.LetterOfAuthority
        //     };
        //     
        //     return View(nameof(OrganisationDetails), model);
        // }
        
        // TODO: Reenable new user functionality
        // [HttpGet]
        // public async Task<IActionResult> UsersDetails()
        // {
        //     var organisationId = HttpContext.Request.Query["organisationId"];
        //
        //     var organisationDetails = await _getOrganisationDetailsService.Get(new GetOrganisationDetailsRequest()
        //     {
        //         OrganisationId = organisationId
        //     }, CancellationToken.None);
        //
        //     var model = new OrganisationDetailsViewModel()
        //     {
        //         OrganisationId = organisationId,
        //         OrganisationName = organisationDetails.OrganisationName,
        //         ResponsiblePersonName = organisationDetails.ResponsiblePersonName,
        //         OrganisationUsers = new List<UserModel>()
        //     };
        //     
        //     return View(nameof(UsersDetails), model);
        // }

        [HttpGet]
        public async Task<IActionResult> InviteUser()
        {
            var organisationId = HttpContext.Request.Query["OrganisationId"];

            var organisationDetails = await _getOrganisationDetailsService.Get(new GetOrganisationDetailsRequest()
            {
                OrganisationId = organisationId
            }, CancellationToken.None);

            var model = new InvitationsViewModel()
            {
                OrganisationName = organisationDetails.OrganisationName,
                BackAction = $"dashboard/users-details{organisationId}"
            };

            return View(nameof(InviteUser), model);
        }

        [HttpPost]
        public IActionResult InviteUser([FromForm] InvitationsViewModel model)
        {
            var organisationId = HttpContext.Request.Query["OrganisationId"];

            if (ModelState.GetFieldValidationState(nameof(model.UserEmail)) == ModelValidationState.Invalid)
            {
                model.BackAction = $"dashboard/users-details{organisationId}";

                return View(nameof(InviteUser), model);
            }

            return RedirectToAction(nameof(EmailConfirmation), model);
        }

        [HttpGet]
        public IActionResult EmailConfirmation(InvitationsViewModel model)
        {
            return View(nameof(EmailConfirmation), model);
        }

        private async Task<GetOrganisationResponse> RetrieveOrganisationFromApi()
        {
            var response = await _getOrganisationService.Get(new GetOrganisationRequest()
            {
                OrganisationId = Request.Query["OrganisationId"]
            }, CancellationToken.None);

            return response;
        }

        private async Task<List<OrganisationItemViewModel>> GetOrganisationDashboardAsync(string userId)
        {
            var organisationList = new List<OrganisationItemViewModel>();
            
            var organisations = await _getOrganisationsForUserService.Get(new GetOrganisationsForUserRequest()
            {
                UserId = userId
            }, CancellationToken.None);
            
            foreach (var organisation in organisations.Organisations)
            {
                var organisationItem = new OrganisationItemViewModel
                    {
                        Id = Guid.Parse(organisation.OrganisationId),
                        Name = organisation.OrganisationName,
                        NumberOfInstallations = organisation.NumberOfApplications
                    };

                    organisationList.Add(organisationItem);
            }

            return organisationList;
        }
    }
}
