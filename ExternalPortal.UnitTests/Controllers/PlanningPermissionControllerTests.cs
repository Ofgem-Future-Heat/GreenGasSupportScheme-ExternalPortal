using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using ExternalPortal.Constants;
using ExternalPortal.Controllers;
using ExternalPortal.Enums;
using ExternalPortal.Extensions;
using ExternalPortal.Services;
using ExternalPortal.ViewModels;
using ExternalPortal.ViewModels.PlanningPermission;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Moq;
using Ofgem.API.GGSS.Domain.ModelValues;
using Ofgem.API.GGSS.Domain.ModelValues.StageOne;
using Xunit;
using TaskStatus = ExternalPortal.Enums.TaskStatus;

namespace ExternalPortal.UnitTests.Controllers
{
    public class PlanningPermissionControllerTests : BaseFileUploadControllerTests, IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly PlanningPermissionController _controller;

        private readonly Mock<IUrlHelper> _urlHelper;
        private readonly Mock<HttpContext> _httpContext;
        private readonly Mock<ILogger<PlanningPermissionController>> _logger;
        private readonly Mock<ISaveDocumentService> _saveDocumentService;
        private readonly Mock<IDeleteDocumentService> _deleteDocumentService;
        private readonly Mock<IRedisCacheService> _mockRedisCacheService;
        private readonly Mock<ClaimsPrincipal> _userMock;

        private readonly Mock<IGetApplicationService> _mockGetApplicationService;
        private readonly Mock<IUpdateApplicationService> _mockUpdateApplicationService;

        private readonly InstallationModel _newInstallationModel;
        private readonly string _organisationId;
        private readonly string _newInstallationCookie;
        private readonly string _nameIdentifierValue;
        private readonly Guid _organisationGuid;

        public PlanningPermissionControllerTests(WebApplicationFactory<Startup> fixture) : base(fixture)
        {
            _urlHelper = new Mock<IUrlHelper>();
            _httpContext = new Mock<HttpContext>();
            _logger = new Mock<ILogger<PlanningPermissionController>>();
            _saveDocumentService = new Mock<ISaveDocumentService>();
            _deleteDocumentService = new Mock<IDeleteDocumentService>();
            _mockRedisCacheService = new Mock<IRedisCacheService>();

            _mockGetApplicationService = new Mock<IGetApplicationService>();
            _mockUpdateApplicationService = new Mock<IUpdateApplicationService>();

            _nameIdentifierValue = "326fa974-7c05-4b37-a8e4-6d5fe6deb63b";
            _userMock = new Mock<ClaimsPrincipal>();
            _userMock.SetupGet(u => u.Claims).Returns(new List<Claim> { new Claim(ClaimTypes.NameIdentifier, _nameIdentifierValue) }.AsEnumerable());

            _organisationId = "b141ac41-d6f7-47fd-b31e-847d77134fca";
            _organisationGuid = Guid.Parse(_organisationId);
            _newInstallationModel = new InstallationModel
            {
                Id = Guid.Parse("b0c2fe16-7a68-42fb-986d-13df95084625"),
                OrganisationId = _organisationGuid,
                UserId = Guid.Parse(_userMock.Object.GetUserId().ToString())
            };

            _newInstallationCookie = $"{CookieKeys.NewInstallation}={_newInstallationModel.Id}";

            _mockGetApplicationService
                .Setup(a => a.Get(It.IsAny<GetApplicationRequest>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new GetApplicationResponse() { Application = new ApplicationValue() }));

            _controller = new PlanningPermissionController(
                _logger.Object,
                _saveDocumentService.Object,
                _deleteDocumentService.Object,
                _mockGetApplicationService.Object,
                _mockUpdateApplicationService.Object,
                _mockRedisCacheService.Object)
            {
                Url = _urlHelper.Object,
                ControllerContext = new ControllerContext
                {
                    HttpContext = _httpContext.Object
                },
                TempData = A.Fake<TempDataDictionary>()
            };
        }

        [Fact]
        public void GetPlanningPermissionIndex_WhenCalled_ReturnsViewResultTypePlanningDetailsViewModel()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            _controller.ControllerContext.HttpContext.Request.Headers.Add(HeaderNames.Cookie, _newInstallationCookie);
            _controller.ControllerContext.HttpContext.User = _userMock.Object;

            // Act
            var result = _controller.Index();

            // Assert
            result.Should().BeOfType<ViewResult>();
            ((ViewResult)result).ViewName.Should().Be("Index");

            var model = ((ViewResult)result).Model;
            model.Should().BeOfType<PlanningPermission>();
        }

        [Fact]
        public async Task PostPlanningPermissionPersonIndex_WhenCalledAndModelStateNotValid_ReturnsViewResultType()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            _controller.ControllerContext.HttpContext.User = _userMock.Object;
            _controller.ControllerContext.HttpContext.Request.Headers.Add(HeaderNames.Cookie, _newInstallationCookie);

            // Act
            var result = await _controller.Index(null);

            // Assert
            result.Should().BeOfType<ViewResult>();
            ((ViewResult)result).ViewName.Should().Be("Index");

            var model = ((ViewResult)result).Model;
            model.Should().BeOfType<PlanningPermission>();
        }

        [Fact]
        public async Task PostPlanningPermissionPersonIndex_WhenCalledWithHavePlanningPermission_ReturnsViewResultType()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            _controller.ControllerContext.HttpContext.User = _userMock.Object;
            _controller.ControllerContext.HttpContext.Request.Headers.Add(HeaderNames.Cookie, _newInstallationCookie);

            _newInstallationModel.StageOne.PlanningDetails.PlanningPermissionOutcome = PlanningPermissionOutcome.HavePlanningPermission;

            // Act
            var result = await _controller.Index("Yes");

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();
            ((RedirectToActionResult)result).ActionName.Should().Be("PlanningUpload");
        }

        [Fact]
        public void GetWhatYouWillNeed_WhenCalled_ReturnsViewResultTypeInstallationModel()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            _controller.ControllerContext.HttpContext.Request.Headers.Add(HeaderNames.Cookie, _newInstallationCookie);
            _controller.ControllerContext.HttpContext.User = _userMock.Object;

            // Act
            var result = _controller.WhatYouWillNeed();

            // Assert
            result.Should().BeOfType<ViewResult>();
            ((ViewResult)result).ViewName.Should().Be("WhatYouWillNeed");
        }

        [Fact]
        public async Task GetCheckAnswers_WhenCalled_ReturnsViewResultTypeWithApplicationValue()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            _controller.ControllerContext.HttpContext.Request.Headers.Add(HeaderNames.Cookie, _newInstallationCookie);
            _controller.ControllerContext.HttpContext.User = _userMock.Object;

            _mockGetApplicationService
                .Setup(a => a.Get(It.IsAny<GetApplicationRequest>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new GetApplicationResponse()
                {
                    Application = new ApplicationValue()
                    {
                        StageOne = new StageOneValue()
                        {
                            ProvidePlanningPermission = new ProvidePlanningPermissionValue()
                            {
                                Status = TaskStatus.InProgress.GetLabelText()
                            }
                        }
                    }
                }));

            // Act
            var result = await _controller.CheckAnswers();

            // Assert
            result.Should().BeOfType<ViewResult>();
            ((ViewResult)result).ViewName.Should().Be("CheckAnswers");

            var viewResult = (ViewResult)result;
            viewResult.Should().BeOfType<ViewResult>();
            viewResult.ViewName.Should().Be("CheckAnswers");

            viewResult.Model.Should().BeOfType<ApplicationValue>();
            ((ApplicationValue)viewResult.Model).StageOne.ProvidePlanningPermission.Status.Should().Be(TaskStatus.InProgress.GetLabelText());
        }

        [Fact]
        public async Task LetterOfAuthorityUploadConfirmValidatesOptionsSelected()
        {
            // Arrange 
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

            // Act 
            var result = await _controller.PlanningUploadConfirm(null, default);
            var viewResult = result as ViewResult;

            // Assert 
            viewResult.Should().NotBeNull();
            viewResult?.ViewName.Should().Be("PlanningUploadConfirm");
        }

        [Fact]
        public async Task LetterOfAuthorityUploadConfirmOptionSelectedIsYes()
        {
            // Arrange 
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

            // Act 
            var result = await _controller.PlanningUploadConfirm("Yes", default);
            var viewResult = result as RedirectToActionResult;

            // Assert 
            viewResult.Should().NotBeNull();
            viewResult.ActionName.Should().Be("CheckAnswers");
        }

        [Fact]
        public async Task LetterOfAuthorityUploadConfirmOptionSelectedIsNo()
        {
            // Arrange 
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

            // Act 
            var result = await _controller.PlanningUploadConfirm("No", default);
            var viewResult = result as RedirectToActionResult;

            // Assert 
            viewResult.Should().NotBeNull();
            viewResult.ActionName.Should().Be("PlanningUpload");
            _deleteDocumentService.Verify(d => d.Delete(It.IsAny<string>(), It.IsAny<CancellationToken>()));
        }

        [Fact]
        public async Task PostCheckAnswers_WhenCalled_ReturnsRedirectResultTypeEditApplication()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            _controller.ControllerContext.HttpContext.Request.Headers.Add(HeaderNames.Cookie, _newInstallationCookie);
            _controller.ControllerContext.HttpContext.User = _userMock.Object;

            _mockGetApplicationService
                .Setup(s => s.Get(It.IsAny<GetApplicationRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetApplicationResponse
                {
                    Application = new ApplicationValue(),
                });

            // Act
            var result = await _controller.CheckAnswers(CancellationToken.None);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();
            ((RedirectToActionResult)result).ActionName.Should().Be("Index");
            ((RedirectToActionResult)result).ControllerName.Should().Be("TaskList");
        }

        [Fact]
        public async Task GetPlanningPermissionExemptUploadActionRedirectsToCheckAnswers()
        {
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            
            var model = new PlanningPermission()
            {
                ExemptionStatement = "Statement"
            };
            var result = await _controller.PlanningExemptUpload(model, default);
            
            ((RedirectToActionResult)result).ActionName.Should().Be("CheckAnswers");
        }
        
        [Fact]
        public async Task GetPlanningPermissionExemptUploadActionDoesNotRedirectWhenNoInputIsGiven()
        {
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

            var model = new PlanningPermission();

            _controller.ModelState.AddModelError("key", "model-error");

            var viewResult = (ViewResult) await _controller.PlanningExemptUpload(model, default);

            viewResult.Model.Should().BeOfType<PlanningPermission>();
            viewResult.ViewName.Should().Be("PlanningExemptUpload");
        }
    }
}
