using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using ExternalPortal.Constants;
using ExternalPortal.Controllers;
using ExternalPortal.Services;
using ExternalPortal.ViewModels;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Logging;
using Moq;
using Ofgem.API.GGSS.Domain.Contracts.Services;
using Ofgem.API.GGSS.Domain.Enums;
using Ofgem.API.GGSS.Domain.Models;
using Ofgem.API.GGSS.Domain.ModelValues;
using Xunit;

namespace ExternalPortal.UnitTests.Controllers
{
    public class DashboardControllerTests : BaseFileUploadControllerTests, IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly DashboardController _controller;

        private readonly Mock<ILogger<DashboardController>> _mockLogger;
        private readonly Mock<ClaimsPrincipal> _userMock;
        private readonly Mock<UserManager<IdentityUser>> _mockUserManager;
        private readonly Mock<IApplicationService> _mockApplicationService;
        private readonly Mock<IRedisCacheService> _mockRedisCacheService;
        private readonly Mock<IGetOrganisationService> _mockGetOrganisationService;
        private readonly Mock<IGetOrganisationDetailsService> _mockGetOrganisationDetailsService;
        private readonly Mock<ICreateApplicationService> _mockCreateApplicationService;
        private readonly Mock<IGetOrganisationsForUserService> _mockGetOrganisationsForUserService;
        private readonly Mock<IGetUserByProviderIdService> _mockGetUserByProviderIdService;
        private readonly Mock<IAddUserService> _mockAddUserService;
        
        private string _nameIdentifierValue;
        private string _nameValue;
        private string _displayNameValue;
        private string _organisationId;
        private IEnumerable<ApplicationModel> _installations;
        private ApplicationModel _installation1;
        private ApplicationModel _installation2;


        public DashboardControllerTests(WebApplicationFactory<Startup> fixture) : base(fixture)
        {
            _nameIdentifierValue = "326fa974-7c05-4b37-a8e4-6d5fe6deb63b";
            _nameValue = "James.Anderson@ofgem.gov.uk";
            _displayNameValue = "James Anderson";
            _organisationId = "b141ac41-d6f7-47fd-b31e-847d77134fca";

            _mockLogger = new Mock<ILogger<DashboardController>>();
            _mockGetOrganisationService = new Mock<IGetOrganisationService>();

            var userStore = new Mock<IUserStore<IdentityUser>>();
            _mockUserManager = new Mock<UserManager<IdentityUser>>(userStore.Object, null, null, null, null, null, null, null, null);
            _mockUserManager.Object.UserValidators.Add(new UserValidator<IdentityUser>());
            _mockUserManager.Object.PasswordValidators.Add(new PasswordValidator<IdentityUser>());
            _mockUserManager.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>())).Returns(Task.FromResult(new IdentityUser(_nameValue) { }));

            _installation1 = new ApplicationModel
            {
                Id = Guid.NewGuid().ToString(),
                IsActive = true,
                Value = new ApplicationValue
                {
                    Location = Location.Scotland,
                    InstallationName = "CNG EUROCENTRAL GLASGOW",
                    InstallationAddress = new AddressModel { LineOne = "WOODSIDE", Town = "MOTHERWELL", Postcode = "ML1" },
                    MaxCapacity = 786000,
                    DateInjectionStart = DateTime.Now.AddDays(365),
                    Reference = "TEST-REF-001",
                    Status = ApplicationStatus.Draft
                },
                Organisation = new OrganisationModel
                {
                    Id = Guid.Parse(_organisationId).ToString(),
                    ResponsiblePeople = new List<ResponsiblePersonModel>
                        {
                            new ResponsiblePersonModel
                            {
                                Id = _nameIdentifierValue,
                                Value = new ResponsiblePersonValue { TelephoneNumber="0712345678" }
                            }
                        },
                    Value = new OrganisationValue
                    {
                        Name = "Clydebiomass",
                    }
                },
                Documents = new List<DocumentModel>
                    {
                        new DocumentModel
                        {
                            Id = Guid.NewGuid().ToString(),
                            Value = new DocumentValue
                            {
                                FileId = "1779fcc5-bdd7-4980-be9e-a852e5ed8904_CapacityCheck.pdf",
                                FileName= "CapacityCheck.pdf",
                                Tags = DocumentTags.CapacityCheck}
                        },
                        new DocumentModel
                        {
                            Id = Guid.NewGuid().ToString(),
                            Value = new DocumentValue
                            {
                                FileId = "5b4837f3-8faf-4e27-877f-d10a0bb455a9_MyPlanningPermission.pdf",
                                FileName= "MyPlanningPermission.pdf",
                                Tags = DocumentTags.PlanningPermission}
                        }
                    }
            };

            _installation2 = new ApplicationModel
            {
                Id = Guid.NewGuid().ToString(),
                IsActive = true,
                Value = new ApplicationValue
                {
                    Location = Location.Scotland,
                    InstallationName = "CNG EDINBURGH",
                    InstallationAddress = new AddressModel { LineOne = "EDINBURGH AIRPORT", Town = "EDINBURGH", Postcode = "ED1" },
                    Reference = "TEST-REF-002",
                    Status = ApplicationStatus.Draft
                },
                Organisation = new OrganisationModel
                {
                    Id = Guid.Parse(_organisationId).ToString(),
                    ResponsiblePeople = new List<ResponsiblePersonModel>
                        {
                            new ResponsiblePersonModel
                            {
                                Id = _nameIdentifierValue,
                                Value = new ResponsiblePersonValue { TelephoneNumber="0712345699" }
                            }
                        },
                    Value = new OrganisationValue
                    {
                        Name = "Clydebiomass",
                    }
                },
                Documents = new List<DocumentModel>
                    {
                        new DocumentModel
                        {
                            Id = Guid.NewGuid().ToString(),
                            Value = new DocumentValue
                            {
                                FileId = "1779fcc5-bdd7-4980-be9e-a852e5ed8904_CapacityCheck.pdf",
                                FileName= "CapacityCheck.pdf",
                                Tags = DocumentTags.CapacityCheck}
                        },
                        new DocumentModel
                        {
                            Id = Guid.NewGuid().ToString(),
                            Value = new DocumentValue
                            {
                                FileId = "5b4837f3-8faf-4e27-877f-d10a0bb455a9_MyPlanningPermission.pdf",
                                FileName= "MyPlanningPermission.pdf",
                                Tags = DocumentTags.PlanningPermission}
                        },
                        new DocumentModel
                        {
                            Id = Guid.NewGuid().ToString(),
                            Value = new DocumentValue
                            {
                                FileId = "6ac81fd8-9317-4b36-8565-cd4b3e2354f6_Feedstock supply.pdf",
                                FileName= "Feedstock supply.pdf",
                                Tags = DocumentTags.FeedstockAgreement}
                        }
                    }
            };

            _mockApplicationService = new Mock<IApplicationService>();
            _mockRedisCacheService = new Mock<IRedisCacheService>();
            _mockCreateApplicationService = new Mock<ICreateApplicationService>();
            _mockGetOrganisationsForUserService = new Mock<IGetOrganisationsForUserService>();
            _mockGetUserByProviderIdService = new Mock<IGetUserByProviderIdService>();
            _mockAddUserService = new Mock<IAddUserService>();
            _mockGetOrganisationDetailsService = new Mock<IGetOrganisationDetailsService>();

            _controller = new DashboardController(_mockLogger.Object, _mockCreateApplicationService.Object, _mockRedisCacheService.Object, _mockGetOrganisationService.Object,_mockGetOrganisationDetailsService.Object ,_mockGetOrganisationsForUserService.Object, _mockGetUserByProviderIdService.Object, _mockAddUserService.Object);

            _userMock = new Mock<ClaimsPrincipal>();
        }

        [Fact]
        public async Task GetDashboardIndex_WhenCalled_ReturnViewResultTypeDashboardViewModel()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() { } };

            _userMock.Setup(u => u.Identity).Returns(new ClaimsIdentity());

            _userMock.Setup(u => u.Identity.IsAuthenticated).Returns(true);

            _userMock.SetupGet(u => u.Claims).Returns(new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, _nameIdentifierValue),
                new Claim(ClaimTypes.Name, _nameValue),
                new Claim(ClaimTypes.GivenName, "James"),
                new Claim(ClaimTypes.Surname, "Anderson"),
                new Claim("org", _organisationId)
            }.AsEnumerable());

            _controller.ControllerContext.HttpContext.User = _userMock.Object;

            _installations = new List<ApplicationModel> { _installation1, _installation2 };
            _mockApplicationService.Setup(a => a.GetAsync(default, false)).Returns(Task.FromResult(_installations));

            _mockGetOrganisationsForUserService.Setup(a =>
                a.Get(It.IsAny<GetOrganisationsForUserRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(new GetOrganisationsForUserResponse()
            {
                Organisations = new List<GetOrganisationForUserResponse>()
                {
                    new GetOrganisationForUserResponse()
                    {
                        OrganisationId = Guid.NewGuid().ToString(),
                        OrganisationName = "Clydebiomass",
                        NumberOfApplications = 2
                    }
                }
            });

            _mockGetUserByProviderIdService.Setup(s => s.Get(It.IsAny<GetUserRequest>())).ReturnsAsync(
                new GetUserResponse()
                {
                    Found = true,
                    UserId = Guid.NewGuid().ToString()
                });
            
            // Act
            var result = await _controller.Index(CancellationToken.None);

            // Assert
            result.Should().BeOfType<ViewResult>();
            ((ViewResult)result).ViewName.Should().Be("Index");

            var model = ((ViewResult)result).Model;
            model.Should().BeOfType<DashboardViewModel>();

            ((DashboardViewModel)model).UserDisplayName.Should().Be(_displayNameValue);

            ((DashboardViewModel)model).Organisations.Should().NotBeEmpty();

            ((DashboardViewModel)model).Organisations[0].NumberOfInstallations.Should().Be(2);
            ((DashboardViewModel)model).Organisations[0].Name.Should().Be("Clydebiomass");
        }

        [Fact]
        public async Task GetDashboardIndex_WhenCalled_ReturnViewResultTypeDashboardViewModel_NoOrganisations()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() { } };

            _userMock.Setup(u => u.Identity).Returns(new ClaimsIdentity());

            _userMock.Setup(u => u.Identity.IsAuthenticated).Returns(true);

            _userMock.SetupGet(u => u.Claims).Returns(new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, _nameIdentifierValue),
                new Claim(ClaimTypes.Name, _nameValue),
                new Claim(ClaimTypes.GivenName, "James"),
                new Claim(ClaimTypes.Surname, "Anderson")
            }.AsEnumerable());

            _controller.ControllerContext.HttpContext.User = _userMock.Object;

            _installations = new List<ApplicationModel>();
            _mockApplicationService.Setup(a => a.GetAsync(default, false)).Returns(Task.FromResult(_installations));
            
            _mockGetOrganisationsForUserService.Setup(a =>
                a.Get(It.IsAny<GetOrganisationsForUserRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(new GetOrganisationsForUserResponse()
            {
                Organisations = new List<GetOrganisationForUserResponse>()
            });

            _mockGetUserByProviderIdService.Setup(s => s.Get(It.IsAny<GetUserRequest>())).ReturnsAsync(
                new GetUserResponse()
                {
                    Found = true,
                    UserId = Guid.NewGuid().ToString()
                });
            
            // Act
            var result = await _controller.Index(CancellationToken.None);

            // Assert
            result.Should().BeOfType<ViewResult>();
            ((ViewResult)result).ViewName.Should().Be("Index");

            var model = ((ViewResult)result).Model;
            model.Should().BeOfType<DashboardViewModel>();

            ((DashboardViewModel)model).UserDisplayName.Should().Be(_displayNameValue);

            ((DashboardViewModel)model).Organisations.Should().BeEmpty();
        }
        
        [Fact]
        public async Task InviteUserActionReturnsViewResultType()
        {
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() { } };
            
            _mockGetOrganisationDetailsService.Setup(a =>
                    a.Get(It.IsAny<GetOrganisationDetailsRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetOrganisationDetailsResponse()
                {
                    OrganisationName = "Org Name"
                });

            var result = _controller.InviteUser();
            var viewResult = await result as ViewResult;

            viewResult.Should().NotBeNull();
            viewResult?.ViewName.Should().Be("InviteUser");
            
            var model = viewResult?.Model as InvitationsViewModel;
            model.Should().NotBeNull();
            model.Should().BeOfType<InvitationsViewModel>();
        }

        [Fact]
        public void InviteUserRedirectsToActionIfValidEmailProvided()
        {
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() { User = _userMock.Object } };

            var userEmail = new InvitationsViewModel() {UserEmail = "test@test.com"};
            
            var result = _controller.InviteUser(userEmail);
            var viewResult = result as RedirectToActionResult;

            viewResult.Should().NotBeNull();
            viewResult?.ActionName.Should().Be("EmailConfirmation");
        }
    }
}
