using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using ExternalPortal.Constants;
using ExternalPortal.Controllers;
using ExternalPortal.Enums;
using ExternalPortal.Extensions;
using ExternalPortal.Services;
using ExternalPortal.ViewModels;
using ExternalPortal.ViewModels.Shared;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Moq;
using Ofgem.API.GGSS.Domain.Commands.Applications;
using Ofgem.API.GGSS.Domain.Contracts.Services;
using Ofgem.API.GGSS.Domain.Models;
using Ofgem.API.GGSS.Domain.ModelValues;
using Ofgem.API.GGSS.Domain.ModelValues.StageOne;
using Ofgem.API.GGSS.DomainModels;
using Ofgem.Azure.Redis.Data.Contracts;
using Xunit;
using InstallationModel = ExternalPortal.ViewModels.InstallationModel;
using TaskStatus = ExternalPortal.Enums.TaskStatus;

namespace ExternalPortal.UnitTests.Controllers
{
    public class StageOneControllerTests : BaseFileUploadControllerTests, IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly StageOneController _controller;
        private readonly Mock<ILogger<StageOneController>> _logger;
        private readonly Mock<ISaveDocumentService> _saveDocumentService;
        private readonly Mock<ClaimsPrincipal> _userMock;
        private readonly Mock<IApplicationService> _applicationService;
        private readonly string _newInstallationCookie;
        private readonly string _nameIdentifierValue;
        private readonly string _organisationId;
        private Guid _organisationGuid;
        private readonly InstallationModel _newInstallationModel;
        private readonly string _returnToYourApplicationURL;
        private readonly Dictionary<string, string> _referenceParams;
        private readonly Mock<IRedisCacheService> _mockRedisCacheService;
        private readonly Mock<IGetApplicationService> _mockGetApplicationService;
        private readonly Mock<IUpdateApplicationService> _mockUpdateApplicationService;
        private const string _scheme = "https";
        private readonly HostString _host = new HostString("localhost");

        public StageOneControllerTests(WebApplicationFactory<Startup> fixture) : base(fixture)
        {
            _logger = new Mock<ILogger<StageOneController>>();
            _saveDocumentService = new Mock<ISaveDocumentService>();
            var store = new Mock<IUserStore<IdentityUser>>();
            _mockRedisCacheService = new Mock<IRedisCacheService>();
            _applicationService = new Mock<IApplicationService>();
            _mockGetApplicationService = new Mock<IGetApplicationService>();
            _mockUpdateApplicationService = new Mock<IUpdateApplicationService>();
            var sendEmailService = new Mock<ISendEmailService>();

            _mockGetApplicationService.Setup(s => s
                .Get(It.IsAny<GetApplicationRequest>(), It.IsAny<CancellationToken>()))
                .Returns(
                Task.FromResult(new GetApplicationResponse
                {
                    Application = new ApplicationValue()
                    {
                        StageOne = new Ofgem.API.GGSS.Domain.ModelValues.StageOne.StageOneValue()
                        {
                            TellUsAboutYourSite = new Ofgem.API.GGSS.Domain.ModelValues.StageOne.TellUsAboutYourSiteValue()
                            {
                                CapacityCheckDocument = new DocumentValue()
                                {
                                    FileId = "file-id",
                                    FileName = "file-name",
                                    FileSizeAsString = "file-size",
                                    Tags = "file-tag"
                                }
                            }
                        }
                    }
                }));

            _nameIdentifierValue = "326fa974-7c05-4b37-a8e4-6d5fe6deb63b";
            _userMock = new Mock<ClaimsPrincipal>();
            _userMock.Setup(u => u.Identity.IsAuthenticated).Returns(true);
            _userMock
                .SetupGet(u => u.Claims)
                .Returns(
                new List<Claim> {
                    new Claim(ClaimTypes.NameIdentifier, _nameIdentifierValue),
                    new Claim(ClaimTypes.Email, "email-address")
                }
                .AsEnumerable());

            _organisationId = "b141ac41-d6f7-47fd-b31e-847d77134fca";
            _organisationGuid = Guid.Parse(_organisationId);

            _newInstallationModel = new InstallationModel
            {
                Id = Guid.Parse("b0c2fe16-7a68-42fb-986d-13df95084625"),
                OrganisationId = _organisationGuid,
                UserId = Guid.Parse(_userMock.Object.GetUserId().ToString()),
            };

            _returnToYourApplicationURL = $"/task-list";
            _referenceParams = new Dictionary<string, string>
            {
                { UrlKeys.ReturnToYourApplicationLink, _returnToYourApplicationURL }
            };
            _newInstallationModel.ReferenceParams = _referenceParams;

            _newInstallationModel.StageOne.PlantDetails.ReferenceParams = _newInstallationModel.ReferenceParams;

            _newInstallationCookie = $"{CookieKeys.NewInstallation}={_newInstallationModel.Id}";

            _controller = new StageOneController(
                _mockRedisCacheService.Object,
                _logger.Object,
                _saveDocumentService.Object,
                _mockGetApplicationService.Object,
                _mockUpdateApplicationService.Object,
                sendEmailService.Object);
        }

        #region "Plant Details What You Will Need Action Tests"
        [Fact]
        public void GetPlantDetailsWhatYouWillNeed_WhenCalled_ReturnsViewResultTypePlantDetailsViewModel()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            _controller.ControllerContext.HttpContext.Request.Headers.Add(HeaderNames.Cookie, _newInstallationCookie);
            _controller.ControllerContext.HttpContext.User = _userMock.Object;
            
            _mockRedisCacheService.Setup(r => r
                .UpdateApplicationTaskStatusAsync(It.IsAny<Guid>(), It.IsAny<ApplicationStage>(), It.IsAny<TaskType>(), It.IsAny<Enums.TaskStatus>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(true));

            _mockRedisCacheService.Setup(r => r
                .SaveAsync(It.IsAny<PortalViewModel<StageOne>>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(true));

            // Act
            var result = _controller.PlantDetailsWhatYouWillNeed();

            // Assert
            result.Should().BeOfType<ViewResult>();
            ((ViewResult)result).ViewName.Should().Be("PlantDetails/WhatYouWillNeed");

            var model = ((ViewResult)result).Model;
            model.Should().BeOfType<PlantDetailsModel>();

            ((PlantDetailsModel)model).BackAction.Should().Be(_returnToYourApplicationURL);
            ((PlantDetailsModel)model).ReferenceParams[UrlKeys.ReturnToYourApplicationLink].Should().Be(_returnToYourApplicationURL);
        }
        #endregion

        #region "Plant Location Action Tests"
        [Fact]
        public async Task GetPlantLocation_WhenCalled_ReturnsViewResultTypePlantDetailsViewModel()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            _controller.ControllerContext.HttpContext.Request.Headers.Add(HeaderNames.Cookie, _newInstallationCookie);
            _controller.ControllerContext.HttpContext.User = _userMock.Object;
           
            // Act
            var result = await _controller.PlantLocation(default(CancellationToken));

            // Assert
            result.Should().BeOfType<ViewResult>();
            ((ViewResult)result).ViewName.Should().Be("PlantLocation");

            var model = ((ViewResult)result).Model;
            model.Should().BeOfType<PlantDetailsModel>();

            ((PlantDetailsModel)model).State.Should().Be(TaskStatus.NotStarted);
            ((PlantDetailsModel)model).BackAction.Should().Be(UrlKeys.PlantDetailsWhatYouWillNeedLink);
            ((PlantDetailsModel)model).ReferenceParams[UrlKeys.ReturnToYourApplicationLink].Should().Be(_returnToYourApplicationURL);
            await Task.CompletedTask;
        }
        #endregion

        #region "Capacity Upload Action Tests"
        [Fact]
        public async void GetCapacityUpload_WhenCalled_ReturnsViewResultTypePlantDetailsViewModel()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            _controller.ControllerContext.HttpContext.Request.Headers.Add(HeaderNames.Cookie, _newInstallationCookie);
            _controller.ControllerContext.HttpContext.User = _userMock.Object;
            _newInstallationModel.StageOne.PlantDetails.State = TaskStatus.InProgress;

            // Act
            var result = await _controller.CapacityUpload(new PlantDetailsModel());

            // Assert
            result.Should().BeOfType<ViewResult>();
            ((ViewResult)result).ViewName.Should().Be("CapacityUpload");

            var model = ((ViewResult)result).Model;
            model.Should().BeOfType<PlantDetailsModel>();

            ((PlantDetailsModel)model).BackAction.Should().Be(UrlKeys.PlantLocationLink);
            ((PlantDetailsModel)model).ReferenceParams[UrlKeys.ReturnToYourApplicationLink].Should().Be(_returnToYourApplicationURL);
        }
        #endregion

        #region "Capacity Upload Confirm Action Tests"
        [Fact]
        public async Task GetCapacityUploadConfirm_WhenCalled_ReturnsViewResultTypePlantDetailsViewModel()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            _controller.ControllerContext.HttpContext.Request.Headers.Add(HeaderNames.Cookie, _newInstallationCookie);
            _controller.ControllerContext.HttpContext.User = _userMock.Object;
            _newInstallationModel.StageOne.PlantDetails.State = TaskStatus.InProgress;
            _newInstallationModel.StageOne.PlantDetails.BackAction = UrlKeys.CapacityUploadLink;

            // Act
            var result = await _controller.CapacityUploadConfirm("", new PlantDetailsModel());

            // Assert
            result.Should().BeOfType<ViewResult>();
            ((ViewResult)result).ViewName.Should().Be("CapacityUploadConfirm");

            var model = ((ViewResult)result).Model;
            model.Should().BeOfType<PlantDetailsModel>();

            ((PlantDetailsModel)model).BackAction.Should().Be(UrlKeys.CapacityUploadLink);
            ((PlantDetailsModel)model).ReferenceParams[UrlKeys.ReturnToYourApplicationLink].Should().Be(_returnToYourApplicationURL);
        }
        #endregion

        [Fact]
        public async Task UploadConfirmValidatesFileProvided()
        {
            SetUpTheController();

            var response = new SaveDocumentResponse();

            response.AddError(new SaveDocumentError("error-id", "error-message"));

            _saveDocumentService
               .Setup(s => s.Save(It.IsAny<IFormFile>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(response);

            var result = await _controller.CapacityUpload(null, default);
            var viewResult = result as ViewResult;

            viewResult.Should().NotBeNull();
            viewResult?.ViewName.Should().Be("CapacityUpload");
            var model = viewResult?.Model as PlantDetailsModel;

            model.Should().NotBeNull();
            model?.HasError().Should().BeTrue();
        }

        [Fact]
        public async Task UploadConfirmValidatesOptionSelected()
        {
            SetUpTheController();
            await EnsureUploadConfirmOptionIsValidated();
        }

        [Fact]
        public async Task UploadConfirmSelectingNoTakesYouBackToFileUpload()
        {
            SetUpTheController();
            await EnsureChoosingNoOnCapacityUploadConfirmationTakesYouBackToCapacityUpload();
        }

        #region "Plant Name Confirm Action Tests"
        [Fact]
        public async Task RedirectToPlantLocationFromPlantName()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            _controller.ControllerContext.HttpContext.Request.Headers.Add(HeaderNames.Cookie, _newInstallationCookie);
            _controller.ControllerContext.HttpContext.User = _userMock.Object;
            _newInstallationModel.StageOne.PlantDetails.State = TaskStatus.InProgress;
            var mockPlantDetailsModel = new PlantDetailsModel()
            {
                Location = PlantLocation.Scotland,
                InstallationName = "TEST INSTALLATION!",
                InstallationAddress = new AddressViewModel
                {
                    Town = "TEST",
                    Postcode = "G144GW"
                }
            };
            
            // Act
            var result = await _controller.PlantName(mockPlantDetailsModel);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();
            ((RedirectToActionResult)result).ActionName.Should().Be("PlantLocation");
        }
        #endregion

        #region "Plant Address Confirm Action Tests"
        [Fact]
        public async Task GetPlantAddress_WhenCalled_ReturnsViewResultTypePlantDetailsViewModel()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            _controller.ControllerContext.HttpContext.User = _userMock.Object;

            _mockGetApplicationService
                .Setup(s => s.Get(It.IsAny<GetApplicationRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(
                    new GetApplicationResponse()
                    {
                        Application = new ApplicationValue()
                        {
                            StageOne = new StageOneValue 
                            { 
                                TellUsAboutYourSite = new TellUsAboutYourSiteValue {
                                    PlantLocation = "Scotland",
                                    PlantName = "Test Installation",
                                    PlantAddress = new AddressModel()
                                    {
                                        LineOne = "test-line-one",
                                        Postcode = "test-postcode"
                                    }
                                } 
                            }
                        }
                    });

            // Act
            var result = await _controller.PlantAddress();

            // Assert
            result.Should().BeOfType<ViewResult>();
            ((ViewResult)result).ViewName.Should().Be("PlantAddress");

            var model = ((ViewResult)result).Model;
            model.Should().BeOfType<AddressViewModel>();

            ((AddressViewModel)model).LineOne.Should().Be("test-line-one");
            ((AddressViewModel)model).Postcode.Should().Be("test-postcode");
        }
        #endregion

        [Fact]
        public async Task InjectionPointAddressShouldReturnViewResultTypeAndPlantDetailsViewModel()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            _controller.ControllerContext.HttpContext.User = _userMock.Object;
            _mockGetApplicationService
                .Setup(s => s.Get(It.IsAny<GetApplicationRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetApplicationResponse()
                {
                    Application = new ApplicationValue()
                    {
                        StageOne = new StageOneValue()
                        {
                            TellUsAboutYourSite = new TellUsAboutYourSiteValue()
                            {
                                InjectionPointAddress = new AddressModel()
                                {
                                    LineOne = "test-line-one",
                                    Postcode = "test-postcode"
                                }
                            }
                        }
                    }
                });

            // Act
            var result = await _controller.InjectionPointAddress();
            var viewResult = result as ViewResult;
            
            // Assert
            viewResult.Should().NotBeNull();
            viewResult?.ViewName.Should().Be("InjectionPointAddress");

            var model = viewResult?.Model as AddressViewModel;
            model.Should().NotBeNull();

            ((AddressViewModel)model).LineOne.Should().Be("test-line-one");
            ((AddressViewModel)model).Postcode.Should().Be("test-postcode");
        }

        #region "Check Answers Action Tests"
        [Fact]
        public async Task GetPlantDetailsCheckAnswers_WhenCalled_ReturnsViewResultTypePlantDetailsViewModel()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            _controller.ControllerContext.HttpContext.Request.Headers.Add(HeaderNames.Cookie, _newInstallationCookie);
            _controller.ControllerContext.HttpContext.User = _userMock.Object;
            _newInstallationModel.StageOne.PlantDetails.State = TaskStatus.InProgress;

            _mockRedisCacheService.Setup(r => r
                .UpdateApplicationTaskStatusAsync(It.IsAny<Guid>(), It.IsAny<ApplicationStage>(), It.IsAny<TaskType>(), It.IsAny<Enums.TaskStatus>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(true));

            _mockGetApplicationService
                .Setup(s => s.Get(It.IsAny<GetApplicationRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(
                    new GetApplicationResponse()
                    {
                        Application = new ApplicationValue()
                    });

            // Act
            var result = await _controller.PlantDetailsCheckAnswers();

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();
            ((RedirectToActionResult)result).ActionName.Should().Be("Index");
            ((RedirectToActionResult)result).ControllerName.Should().Be("TaskList");
        }
        #endregion

        private async Task EnsureUploadConfirmOptionIsValidated()
        {
            var result = await _controller.CapacityUploadConfirm("", new PlantDetailsModel());
            var viewResult = result as ViewResult;

            viewResult.Should().NotBeNull();
            viewResult?.ViewName.Should().Be("CapacityUploadConfirm");
            var model = viewResult?.Model as PlantDetailsModel;

            model.Should().NotBeNull();
            model?.HasError().Should().BeTrue();
        }

        private async Task EnsureChoosingNoOnCapacityUploadConfirmationTakesYouBackToCapacityUpload()
        {
            var result = await _controller.CapacityUploadConfirm("No", new PlantDetailsModel());
            var redirectActionResult = result as RedirectToActionResult;

            redirectActionResult.Should().NotBeNull();
            redirectActionResult?.ActionName.Should().Be("CapacityUpload");
        }

        private void SetUpTheController()
        {
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            _controller.ControllerContext.HttpContext.Request.Headers.Add(HeaderNames.Cookie, _newInstallationCookie);
            _controller.ControllerContext.HttpContext.User = _userMock.Object;
        }

        #region "Stag1eConfirmation Action Tests"
        [Fact]
        public void PostStage1Confirmation_WhenCalled_ReturnsViewResultType_ConfirmationViewModel()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            _controller.ControllerContext.HttpContext.Request.Headers.Add(HeaderNames.Cookie, _newInstallationCookie);
            _controller.ControllerContext.HttpContext.User = _userMock.Object;
            _controller.ControllerContext.HttpContext.Request.Scheme = _scheme;
            _controller.ControllerContext.HttpContext.Request.Host = _host;

            _newInstallationModel.StageOne.PlantDetails.Location = PlantLocation.Scotland;
            _newInstallationModel.StageOne.PlantDetails.InstallationName = "TEST INSTALLATION NAME";
            _newInstallationModel.StageOne.PlantDetails.InstallationAddress = new AddressViewModel
            {
                LineOne = "TEST LINE ONE",
                LineTwo = "TEST LINE TWO",
                County = "TEST COUNTY",
                Town = "TEST TOWN",
                Postcode = "TEST POSTCODE"
            };
            _newInstallationModel.StageOne.PlantDetails.CapacityCheckDocument = new DocumentValue() { FileName = "TEST CAPACITY CHECK DOC" };
            _newInstallationModel.StageOne.PlanningDetails.PlanningPermissionDocument = new DocumentValue() { FileName = "TEST PLANNING PERMISION DOC" };

            _newInstallationModel.StageOne.ProductionDetails.MaxCapacity = "78,600";
            _newInstallationModel.StageOne.ProductionDetails.DateInjectionStart = DateTime.UtcNow.AddYears(1);

            _applicationService.Setup(a => a.SaveStageOneAsync(It.IsAny<StageOne>(), default)).Returns(Task.FromResult(HttpStatusCode.OK.GetHashCode().ToString()));

            // Act
            var result = _controller.Stage1Confirmation();

            // Assert
            result.Should().BeOfType<ViewResult>();
            ((ViewResult)result).ViewName.Should().Be("~/Views/Shared/Confirmation.cshtml");

            var model = ((ViewResult)result).Model;
            model.Should().BeOfType<ConfirmationViewModel>();

            ((ConfirmationViewModel)model).BackAction.Should().Be("/task-list");
        }
        #endregion

        [Fact]
        public async Task PostPlantAddress_WhenCalledAndModelStateNotValid_ReturnsViewResultType()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() { User = _userMock.Object } };

            _controller.ViewData.ModelState.Clear();
            _controller.ViewData.ModelState.AddModelError("InstallationAddress", "It's a puppet!");

            var mockPlantDetailsModel = new AddressViewModel();

            // Act
            var result = await _controller.PlantAddress(mockPlantDetailsModel);

            // Assert
            result.Should().BeOfType<ViewResult>();
            ((ViewResult)result).ViewName.Should().Be("PlantAddress");

            var model = ((ViewResult)result).Model;
            model.Should().BeOfType<AddressViewModel>();

            var resultModelState = ((ViewResult)result).ViewData.ModelState;
            resultModelState.Count.Should().Be(1);
        }
    }
}

