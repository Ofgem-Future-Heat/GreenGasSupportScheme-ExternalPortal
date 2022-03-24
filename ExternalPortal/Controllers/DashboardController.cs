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
using Microsoft.Extensions.Configuration;
using Ofgem.API.GGSS.Domain.Enums;
using Ofgem.GovUK.Notify.Client;
using Microsoft.Extensions.Options;

namespace ExternalPortal.Controllers
{
    public class DashboardController : BaseController
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<DashboardController> _logger;
        private readonly IGetOrganisationService _getOrganisationService;
        private readonly ICreateApplicationService _createApplicationService;
        private readonly IGetOrganisationsForUserService _getOrganisationsForUserService;
        private readonly IGetUserByProviderIdService _getUserByProviderIdService;
        private readonly IAddUserService _addUserService;
        private readonly IGetOrganisationDetailsService _getOrganisationDetailsService;
        private readonly IInviteUserToOrganisationService _inviteUserToOrganisationService;
        private readonly ISendEmailService _sendEmailService;
        private readonly IOptions<SendEmailConfig> _sendEmailConfig;

        public DashboardController(
            IConfiguration configuration,
            ILogger<DashboardController> logger,
            ICreateApplicationService createApplicationService,
            IRedisCacheService redisCache,
            IGetOrganisationService getOrganisationService,
            IGetOrganisationDetailsService getOrganisationDetailsService,
            IGetOrganisationsForUserService getOrganisationsForUserService,
            IGetUserByProviderIdService getUserByProviderIdService,
            IAddUserService addUserService,
            IInviteUserToOrganisationService inviteUserToOrganisationService,
            ISendEmailService sendEmailService,
            IOptions<SendEmailConfig> sendEmailConfig) : base(redisCache)
        {
            _configuration = configuration;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _getOrganisationService = getOrganisationService;
            _getOrganisationDetailsService = getOrganisationDetailsService;
            _getOrganisationsForUserService = getOrganisationsForUserService;
            _getUserByProviderIdService = getUserByProviderIdService;
            _addUserService = addUserService;
            _createApplicationService = createApplicationService;
            _inviteUserToOrganisationService = inviteUserToOrganisationService;
            _sendEmailService = sendEmailService;
            _sendEmailConfig = sendEmailConfig;
        }

        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken token)
        {
            _logger.LogInformation("Index action on Dashboard controller called");

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
        
        [HttpGet]
        public async Task<IActionResult> OrganisationDetails()
        {
            var organisationId = HttpContext.Request.Query["organisationId"];
            
            var organisationDetails = await GetCurrentOrganisationDetails(organisationId);
        
            var model = new OrganisationDetailsViewModel()
            {
                OrganisationId = organisationId,
                OrganisationType = organisationDetails.OrganisationType,
                OrganisationName = organisationDetails.OrganisationName,
                OrganisationAddress = organisationDetails.OrganisationAddress,
                ResponsiblePersonName = organisationDetails.ResponsiblePersonName,
                ResponsiblePersonEmail = organisationDetails.ResponsiblePersonEmail,
                PhotoId = organisationDetails.PhotoId,
                ProofOfAddress = organisationDetails.ProofOfAddress,
                LetterOfAuthority = organisationDetails.LetterOfAuthority,
                IsAuthorisedSignatory = organisationDetails.IsAuthorisedSignatory
            };
            
            return View(nameof(OrganisationDetails), model);
        }
        
        [HttpGet]
        public async Task<IActionResult> UsersDetails()
        {
            var organisationId = HttpContext.Request.Query["organisationId"];
            
            var organisationDetails = await GetCurrentOrganisationDetails(organisationId);
        
            var model = new OrganisationDetailsViewModel()
            {
                OrganisationId = organisationId,
                OrganisationName = organisationDetails.OrganisationName,
                ResponsiblePersonName = organisationDetails.ResponsiblePersonName,
                ResponsiblePersonSurname = organisationDetails.ResponsiblePersonSurname,
                OrganisationUsers = organisationDetails.OrganisationUsers,
                IsAuthorisedSignatory = organisationDetails.IsAuthorisedSignatory
            };
            
            return View(nameof(UsersDetails), model);
        }

        [HttpGet]
        public async Task<IActionResult> InviteUser()
        {
            var organisationId = HttpContext.Request.Query["organisationId"];
            
            var organisationDetails = await GetCurrentOrganisationDetails(organisationId);

            var model = new InvitationsViewModel()
            {
                OrganisationId = organisationId,
                OrganisationName = organisationDetails.OrganisationName,
                IsAuthorisedSignatory = organisationDetails.IsAuthorisedSignatory,
                OrganisationUsers = organisationDetails.OrganisationUsers,
                BackAction = $"dashboard/users-details{organisationId}"
            };

            if (!model.IsAuthorisedSignatory)
            {
                return RedirectToAction(nameof(Index));
            }

            return View(nameof(InviteUser), model);
        }

        [HttpPost]
        public async Task<IActionResult> InviteUser([FromForm] InvitationsViewModel model)
        {

            if (ModelState.GetFieldValidationState(nameof(model.UserEmail)) == ModelValidationState.Invalid)
            {
                model.BackAction = $"dashboard/users-details{model.OrganisationId}";

                return View(nameof(InviteUser), model);
            }
            
            var response = await _inviteUserToOrganisationService.Invite(new InviteUserToOrganisationRequest()
            {
                OrganisationId = model.OrganisationId,
                UserEmail = model.UserEmail
            }, CancellationToken.None);
            
            switch (response.InvitationResult)
            {
                case "USER_ADDED":
                {
                    var emailParameter = new EmailParameterBuilder(EmailTemplateIds.ExistingAdminUser, model.UserEmail, _sendEmailConfig.Value.ReplyToId)
                        .AddCustom("organisationName", model.OrganisationName)
                        .Build();
                
                    await _sendEmailService.Send(emailParameter, CancellationToken.None);
                    break;
                }
                case "USER_NEEDS_TO_REGISTER":
                {
                    var signUpUrl = _configuration.GetValue<string>("AzureAdB2C:SignUpUrl");
                    var registrationLink =
                        $"{signUpUrl}&state={response.InvitationId}";
                
                    var emailParameter = new EmailParameterBuilder(EmailTemplateIds.NewAdminUser, model.UserEmail, _sendEmailConfig.Value.ReplyToId)
                        .AddCustom("organisationName", model.OrganisationName)
                        .AddCustom("registrationLink", registrationLink)
                        .Build();
                
                    await _sendEmailService.Send(emailParameter, CancellationToken.None);
                    break;
                }
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

        private async Task<GetOrganisationDetailsResponse> GetCurrentOrganisationDetails(string organisationId)
        {
            var currentUser = await _getUserByProviderIdService.Get(new GetUserRequest()
            {
                ProviderId = UserId.ToString()
            });
            
            return await _getOrganisationDetailsService.Get(new GetOrganisationDetailsRequest()
            {
                OrganisationId = organisationId,
                UserId = currentUser.UserId
            }, CancellationToken.None);
        }
    }
}
