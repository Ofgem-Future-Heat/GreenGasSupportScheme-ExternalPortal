using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using ExternalPortal.Configuration;
using ExternalPortal.Controllers;
using ExternalPortal.Services;
using ExternalPortal.ViewModels;
using ExternalPortal.ViewModels.Organisation;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Logging;
using Moq;
using Ofgem.API.GGSS.Domain.Enums;
using Ofgem.API.GGSS.Domain.Models;
using Ofgem.API.GGSS.Domain.ModelValues;
using Xunit;
using Times = Moq.Times;

namespace ExternalPortal.UnitTests.Controllers
{
    public class OrganisationControllerTests : IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly HttpClient _client;
        private readonly ILogger<OrganisationController> _logger;
        private readonly OrganisationController _controller;

        private readonly Mock<IRedisCacheService> _mockRedisStore;
        private readonly Mock<ClaimsPrincipal> _userMock;
        private readonly Mock<IDeleteDocumentService> _deleteDocumentService;
        private readonly Mock<ISaveDocumentService> _saveDocumentService;
        private readonly Mock<IGetCompaniesHouseService> _chService;
        private readonly Mock<IOrganisationService> _organisationService;

        private readonly PortalViewModel<OrganisationModel> _registerOrganisationModel;

        private readonly PortalViewModel<OrganisationModel> _mockOrganisationModel = new PortalViewModel<OrganisationModel>()
        {
            Model = new OrganisationModel
            {
                Id = "organisation-id",
                Value = new OrganisationValue
                {
                    RegistrationNumber = "registration-number",
                    Name = "organisation-name",
                    ReferenceNumber = "reference-number",
                    Type = OrganisationType.Private,
                    RegisteredOfficeAddress = new AddressModel
                    {
                        Name = "organisation-name",
                        LineOne = "line-one",
                        LineTwo = "line-two",
                        Town = "town",
                        County = "county",
                        Postcode = "XX1 1XX"
                    }
                }
            }
        };

        private Mock<IGetUserByProviderIdService> _mockGetUserByProviderId;

        private const string _scheme = "https";
        private readonly HostString _host = new HostString("localhost");

        public OrganisationControllerTests(WebApplicationFactory<Startup> fixture)
        {
            var services = new ServiceConfig()
            {
                KeyVaultUri = "some-fake-kv-uri",
                Api = new ApiConfig
                {
                    RetryCount = 1,
                    RetryIntervalSeconds = 2,
                    InternalApiBaseUri = "https://localhost:44313",
                    CompaniesHouseApiBaseUri = "https://localhost:44313",
                    DocumentsApiBaseUri = "document-service-base-url-with-port-number"
                }
            };

            _logger = A.Fake<ILogger<OrganisationController>>();
            _client = fixture.CreateClient();

            _chService = new Mock<IGetCompaniesHouseService>();

            _chService.Setup(s => s.GetCompanyDetailsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new GetCompaniesHouseResponse() 
                { 
                    Model = _mockOrganisationModel.Model 
                }));

            var store = new Mock<IUserStore<IdentityUser>>();
            var mgr = new Mock<UserManager<IdentityUser>>(store.Object, null, null, null, null, null, null, null, null);
            mgr.Object.UserValidators.Add(new UserValidator<IdentityUser>());
            mgr.Object.PasswordValidators.Add(new PasswordValidator<IdentityUser>());
            mgr.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .Returns(Task.FromResult(new IdentityUser("test.user@ofgem.gov.uk") { }));

            _mockRedisStore = new Mock<IRedisCacheService>();

            _saveDocumentService = new Mock<ISaveDocumentService>();

            _deleteDocumentService = new Mock<IDeleteDocumentService>();
            _organisationService = new Mock<IOrganisationService>();

            var sendEmailService = new Mock<ISendEmailService>();

            _registerOrganisationModel = new PortalViewModel<OrganisationModel>
            {
                Model = new OrganisationModel
                {
                    Value = new OrganisationValue
                    {
                        RegistrationNumber = "123456",
                        RegisteredOfficeAddress = new AddressModel(),
                        PhotoId = new DocumentValue()
                        {
                            FileId = "fake-document-id"
                        },
                        ProofOfAddress = new DocumentValue()
                        {
                            FileId = "fake-document-id"
                        },
                        LegalDocument = new DocumentValue()
                        {
                            FileId = "fake-document-id"
                        },
                        LetterOfAuthorisation = new DocumentValue()
                        {
                            FileId = "fake-document-id"
                        }
                    }
                }
            };

            _userMock = new Mock<ClaimsPrincipal>();
            _userMock.Setup(u => u.Identity).Returns(new ClaimsIdentity());
            _userMock.Setup(u => u.Identity.IsAuthenticated).Returns(true);
            _userMock.SetupGet(u => u.Claims)
                .Returns(new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, "DD370D2F-7911-474E-857F-8958145932F1"),
                    new Claim(ClaimTypes.Email, "email-address")
                }
                .AsEnumerable());

            _mockGetUserByProviderId = new Mock<IGetUserByProviderIdService>();

            _controller = new OrganisationController(
                _logger,
                _chService.Object,
                _mockRedisStore.Object,
                _deleteDocumentService.Object,
                _saveDocumentService.Object,
                _organisationService.Object,
                sendEmailService.Object,
                _mockGetUserByProviderId.Object
                );
        }

        [Fact]
        public void OrganisationControllerReturnsIndexActionView()
        {
            // Arrange 
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() { User = _userMock.Object } };

            // Act
            var result = _controller.Index();

            // Assert
            Assert.IsType<ViewResult>(result);
            Assert.Equal("Index", ((ViewResult)result).ViewName);

            A.CallTo(_logger)
                .Where(x => x.Method.Name == "Log" && x.Arguments.Get<LogLevel>(0) == LogLevel.Debug)
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task OrganisationContollerReturnsStartActionViewWhenNumberMissing()
        {
            _registerOrganisationModel.Model.Value.RegistrationNumber = "";
            var result = await _controller.Start(_registerOrganisationModel ,CancellationToken.None);

            Assert.IsType<ViewResult>(result);
            Assert.Equal("Start", ((ViewResult)result).ViewName);
        }

        [Fact]
        public async Task OrganisationContollerReturnsConfirmActionViewWhenNumberSupplied()
        {
            _mockRedisStore.Setup(r => r.GetOrgRegistrationAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(_mockOrganisationModel));
            var result = await _controller.Confirm(new PortalViewModel<OrganisationModel>());

            Assert.IsType<ViewResult>(result);
            Assert.Equal("Confirm", ((ViewResult)result).ViewName);
        }

        [Fact]
        public async Task OrganisationControllerReturnLookupViewWithCancel()
        {
            var result = await _controller.Confirm(null, "cancel", CancellationToken.None);

            result.Should().BeOfType<RedirectToActionResult>();
            ((RedirectToActionResult)result).ActionName.Should().Be("Start");
        }

        [Fact]
        public async Task OrganisationControllerReturnConfirmView()
        {
            var result = await _controller.Confirm(new PortalViewModel<OrganisationModel>(), "confirm", CancellationToken.None);

            result.Should().BeOfType<RedirectToActionResult>();
            ((RedirectToActionResult)result).ControllerName.Should().Be("ResponsiblePerson");
            ((RedirectToActionResult)result).ActionName.Should().Be("Index");
        }

        [Fact]
        public async Task OrganisationControllerReturnConfirmViewWithNullSubmit()
        {
            // Arrange
            var organisationModel = _mockOrganisationModel;

            _mockRedisStore
                .Setup(r => r.GetOrgRegistrationAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(organisationModel));

            var result = await _controller.Confirm(organisationModel, null, CancellationToken.None);

            result.Should().BeOfType<ViewResult>();
            ((ViewResult)result).ViewName.Should().Be("Confirm");

            var model = ((ViewResult)result).Model;
            model.Should().BeOfType<PortalViewModel<OrganisationModel>>();
        }

        #region "ChooseType action tests"
        [Fact]
        public async Task GetChooseType_WhenCalled_ReturnsViewResultTypeWithOrganisationModelFromCache()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

            _controller.ControllerContext.HttpContext.User = _userMock.Object;

            _mockRedisStore
                .Setup(s => s.GetOrgRegistrationAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(_registerOrganisationModel));

            // Act
            var result = await _controller.ChooseType(default);

            // Assert
            Assert.IsType<ViewResult>(result);
            Assert.Equal("ChooseType", ((ViewResult)result).ViewName);

            var model = ((ViewResult)result).Model;
            Assert.IsType<PortalViewModel<OrganisationModel>>(model);
        }

        [Fact]
        public async Task GetChooseType_WhenCalled_ReturnsViewResultTypeWithOrganisationModelSavedToCache()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() { User = _userMock.Object } };

            _mockRedisStore.Setup(r => r.GetOrgRegistrationAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(_mockOrganisationModel));

            // Act
            var result = await _controller.ChooseType(default);

            // Assert
            Assert.IsType<ViewResult>(result);
            Assert.Equal("ChooseType", ((ViewResult)result).ViewName);

            var model = ((ViewResult)result).Model;
            Assert.IsType<PortalViewModel<OrganisationModel>>(model);
        }

        [Fact]
        public async Task PostChooseType_WhenCalledAndOrganisationModelStateNotValid_ReturnsViewResultType()
        {
            // Arrange
            _controller.ModelState.AddModelError("Type", "Select an organisation type");

            var viewModel = new PortalViewModel<OrganisationModel>
            {
                Model = new OrganisationModel
                {
                    Value = new OrganisationValue()
                }
            };

            // Act
            var result = await _controller.ChooseType(viewModel, default);

            // Assert
            Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Start", ((RedirectToActionResult)result).ActionName);
        }

        [Fact]
        public async Task PostChooseType_WhenCalledWithPrivate_ReturnsViewResultType()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() { User = _userMock.Object } };

            _registerOrganisationModel.Model.Value.Type = OrganisationType.Private;

            _mockRedisStore.Setup(s => s.GetOrgRegistrationAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(_registerOrganisationModel));

            _mockRedisStore.Setup(s => s.SaveOrgRegistrationAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(true));

            // Act
            var result = await _controller.ChooseType(_registerOrganisationModel, default);

            // Assert
            Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Start", ((RedirectToActionResult)result).ActionName);
        }

        [Fact]
        public async Task PostChooseType_WhenCalledWithOther_ReturnsViewResultType()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() { User = _userMock.Object } };

            _registerOrganisationModel.Model.Value.Type = OrganisationType.Other;

            _mockRedisStore.Setup(s => s.GetOrgRegistrationAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(_registerOrganisationModel));

            _mockRedisStore.Setup(s => s.SaveOrgRegistrationAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(true));

            // Act
            var result = await _controller.ChooseType(_registerOrganisationModel, default);

            // Assert
            Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("EnterOrgDetails", ((RedirectToActionResult)result).ActionName);
        }
        #endregion

        #region "EnterOrgDetails action tests"
        [Fact]

        public async Task GetEnterOrgDetails_WhenCalled_ReturnsViewResultTypeWithOrganisationModel()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() { User = _userMock.Object } };

            _registerOrganisationModel.Model.Value.Type = OrganisationType.Other;
            _mockRedisStore.Setup(s => s.GetOrgRegistrationAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(_registerOrganisationModel));

            // Act
            var result = await _controller.EnterOrgDetails(default, new PortalViewModel<OrganisationModel>());

            // Assert
            Assert.IsType<ViewResult>(result);
            Assert.Equal("EnterOrgDetails", ((ViewResult)result).ViewName);

            var model = ((ViewResult)result).Model;
            Assert.IsType<EnterOrgDetailsViewModel>(model);

            Assert.NotNull((EnterOrgDetailsViewModel)model);
        }

        [Fact]
        public async Task PostEnterOrgDetails_WhenCalledAndModelStateNotValid_ReturnsViewResultType()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() { User = _userMock.Object } };

            _controller.ModelState.AddModelError("RegisteredOfficeAddress", "test");

            // Act
            var result = await _controller.EnterOrgDetails(new EnterOrgDetailsViewModel(), default);

            // Assert
            result.Should().BeOfType<ViewResult>();
            ((ViewResult)result).ViewName.Should().Be("EnterOrgDetails");

            var model = ((ViewResult)result).Model;
            model.Should().BeOfType<EnterOrgDetailsViewModel>();
        }

        #endregion

        #region "CheckAnswers action tests"
        [Fact]
        public async Task GetCheckAnswers_WhenCalled_ReturnsViewResultTypeWithOrganisationModel()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

            _controller.ControllerContext.HttpContext.User = _userMock.Object;

            _registerOrganisationModel.Model.Value.Type = OrganisationType.Other;

            _mockRedisStore.Setup(s => s.GetOrgRegistrationAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(_registerOrganisationModel));

            // Act
            var result = await _controller.CheckAnswers();

            // Assert
            result.Should().BeOfType<ViewResult>();
            ((ViewResult)result).ViewName.Should().Be("CheckAnswers");

            var model = ((ViewResult)result).Model;
            model.Should().BeOfType<PortalViewModel<OrganisationModel>>();

            ((PortalViewModel<OrganisationModel>)model).Model.Value.Type.Should().Be(OrganisationType.Other);

            ((PortalViewModel<OrganisationModel>)model).Model.Should().NotBeNull();

            ((PortalViewModel<OrganisationModel>)model).Model.Value.RegisteredOfficeAddress.Should().NotBeNull();
        }

        [Fact]
        public async Task PostCheckAnswers_WhenCalled_ReturnsViewResultTypeWithRegisterOrganisationModel()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() { User = _userMock.Object } };
            _controller.ControllerContext.HttpContext.Request.Scheme = _scheme;
            _controller.ControllerContext.HttpContext.Request.Host = _host;

            _registerOrganisationModel.Model.Value.Type = OrganisationType.Other;
            _registerOrganisationModel.Model.Id = Guid.NewGuid().ToString();
            _registerOrganisationModel.Model.Value.Name = "Organisation Name";
            _registerOrganisationModel.Model.Value.RegistrationNumber = "3F81724B-0D0F-4307-9B33-63E6E6E2E75B";
            _registerOrganisationModel.Model.ResponsiblePeople = new List<ResponsiblePersonModel>()
            {
                new ResponsiblePersonModel()
                {
                    User = new UserModel()
                }
            };

            _mockRedisStore.Setup(s => s.GetOrgRegistrationAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(_registerOrganisationModel));

            _mockGetUserByProviderId.Setup(s => s.Get(It.IsAny<GetUserRequest>())).ReturnsAsync(new GetUserResponse()
            {
                Found = true,
                UserId = Guid.NewGuid().ToString()
            });

            // Act
            var result = await _controller.CheckAnswers(default);

            // Assert
            result.Should().BeOfType<ViewResult>();
            ((ViewResult)result).ViewName.Should().Be("Submitted");

            var model = ((ViewResult)result).Model;
            model.Should().BeOfType<PortalViewModel<OrganisationModel>>();

            ((PortalViewModel<OrganisationModel>)model).Model.Value.RegistrationNumber.Should().NotBeNull();
        }

        #endregion

        [Fact]
        public async Task UploadLegalDocumentSavesFileName()
        {
            // Arrange
            var filename = "Filename.png";
            SetUpRedis();
            SetUpSaveDocumentService();

            // Act
            var formFile = new FormFile(default, 0, 0, filename, filename);
            await _controller.LegalDocUpload(formFile, default);

            // Assert
            _mockRedisStore.Verify(
                r => r.SaveOrgRegistrationAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.Is<OrganisationValue>(model => model.LegalDocument.FileName == filename), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UploadLegalDocumentSavesFileId()
        {
            // Arrange
            var fileId = "1234";
            SetUpRedis();
            SetUpSaveDocumentService(fileId);

            // Act
            var formFile = new FormFile(default, 0, 0, "", "");
            await _controller.LegalDocUpload(formFile, default);

            // Assert
            _mockRedisStore.Verify(
                r => r.SaveOrgRegistrationAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.Is<OrganisationValue>(model => model.LegalDocument.FileId == fileId), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UploadConfirmValidatesLegalDocumentProvided()
        {
            // Arrange
            SetUpRedis();

            var response = new SaveDocumentResponse();

            response.AddError(new SaveDocumentError("error-id", "error-message"));

            _saveDocumentService
                .Setup(s => s.Save(It.IsAny<IFormFile>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            // Act 
            var result = await _controller.LegalDocUpload(null, default);
            var viewResult = result as ViewResult;

            // Assert 
            viewResult?.ViewName.Should().Be("LegalDocUpload");
            var model = viewResult?.Model as PortalViewModel<OrganisationModel>;
            model?.Model.Value.Error.Should().Be("error-message");
        }

        [Fact]
        public async Task LegalDocUploadConfirmValidatesOptionsSelected()
        {
            // Arrange 
            SetUpRedis();
            SetUpSaveDocumentService();

            // Act 
            var result = await _controller.LegalDocUploadConfirm(null, default);
            var viewResult = result as ViewResult;

            // Assert 
            viewResult.Should().NotBeNull();
            viewResult?.ViewName.Should().Be("LegalDocUploadConfirm");
            var model = viewResult?.Model as PortalViewModel<OrganisationModel>;
            model?.Model.Value.Error.Should().Be("Select an option");
        }

        [Fact]
        public async Task UploadPhotoIdSavesFileName()
        {
            // Arrange 
            var photoIdName = "PhotoId.png";
            SetUpRedis();
            SetUpSaveDocumentService();

            // Act
            var formFile = new FormFile(default, 0, 0, photoIdName, photoIdName);
            await _controller.PhotoIdUpload(formFile, default);

            // Assert
            _mockRedisStore.Verify(
                r => r.SaveOrgRegistrationAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.Is<OrganisationValue>(model => model.PhotoId.FileName == photoIdName), It.IsAny<CancellationToken>()), Times.Once);

        }

        [Fact]
        public async Task UploadPhotoIdSavesFileId()
        {
            // Arrange 
            var photoId = "1234";
            SetUpRedis();
            SetUpSaveDocumentService(photoId);

            // Act
            var formFile = new FormFile(default, 0, 0, "", "");
            await _controller.PhotoIdUpload(formFile, default);

            // Assert
            _mockRedisStore.Verify(
                r => r.SaveOrgRegistrationAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.Is<OrganisationValue>(model => model.PhotoId.FileId == photoId), It.IsAny<CancellationToken>()), Times.Once);

        }

        [Fact]
        public async Task UploadConfirmValidatesPhotoIdProvided()
        {
            // Arrange
            SetUpRedis();

            var response = new SaveDocumentResponse();

            response.AddError(new SaveDocumentError("error-id", "error-message"));

            _saveDocumentService
                .Setup(s => s.Save(It.IsAny<IFormFile>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            // Act 
            var result = await _controller.PhotoIdUpload(null, default);
            var viewResult = result as ViewResult;

            // Assert 
            viewResult?.ViewName.Should().Be("PhotoIdUpload");
            var model = viewResult?.Model as PortalViewModel<OrganisationModel>;
            model?.Model.Value.Error.Should().Be("Select a file");
        }

        [Fact]
        public async Task PhotoIdUploadConfirmValidatesOptionsSelected()
        {
            // Arrange 
            SetUpRedis();
            SetUpSaveDocumentService();

            // Act 
            var result = await _controller.PhotoIdUploadConfirm(null, default);
            var viewResult = result as ViewResult;

            // Assert 
            viewResult.Should().NotBeNull();
            viewResult?.ViewName.Should().Be("PhotoIdUploadConfirm");
            var model = viewResult?.Model as PortalViewModel<OrganisationModel>;
            model?.Model.Value.Error.Should().Be("Select an option");
        }

        [Fact]
        public async Task PhotoIdUploadConfirmOptionSelectedIsYes()
        {
            // Arrange 
            SetUpRedis();
            SetUpSaveDocumentService();

            // Act 
            var result = await _controller.PhotoIdUploadConfirm("Yes", default, new PortalViewModel<OrganisationModel>());
            var viewResult = result as RedirectToActionResult;

            // Assert 
            viewResult.Should().NotBeNull();
            viewResult.ActionName.Should().Be("ProofOfAddressUpload");
        }

        [Fact]
        public async Task PhotoIdUploadConfirmOptionSelectedIsNo()
        {
            // Arrange 
            SetUpRedis();
            SetUpSaveDocumentService();

            // Act 
            var result = await _controller.PhotoIdUploadConfirm("No", default);
            var viewResult = result as RedirectToActionResult;

            // Assert 
            viewResult.Should().NotBeNull();
            viewResult.ActionName.Should().Be("PhotoIdUpload");
            _deleteDocumentService.Verify(d => d.Delete(It.IsAny<string>(), It.IsAny<CancellationToken>()));
        }

        [Fact]
        public async Task UploadProofAddressSavesFileName()
        {
            // Arrange 
            var poaName = "POA.png";
            SetUpRedis();
            SetUpSaveDocumentService();

            // Act
            var formFile = new FormFile(default, 0, 0, poaName, poaName);
            await _controller.ProofOfAddressUpload(formFile, default);

            // Assert
            _mockRedisStore.Verify(
                r => r.SaveOrgRegistrationAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.Is<OrganisationValue>(model => model.ProofOfAddress.FileName == poaName), It.IsAny<CancellationToken>()), Times.Once);

        }

        [Fact]
        public async Task UploadProofAddressSavesFileId()
        {
            // Arrange 
            var poaId = "1234";
            SetUpRedis();
            SetUpSaveDocumentService(poaId);

            // Act
            var formFile = new FormFile(default, 0, 0, "", "");
            await _controller.ProofOfAddressUpload(formFile, default);

            // Assert
            _mockRedisStore.Verify(
                r => r.SaveOrgRegistrationAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.Is<OrganisationValue>(model => model.ProofOfAddress.FileId == poaId), It.IsAny<CancellationToken>()), Times.Once);

        }

        [Fact]
        public async Task UploadProofOfAddressConfirmValidatesProofAddressProvided()
        {
            // Arrange
            SetUpRedis();

            var response = new SaveDocumentResponse();

            response.AddError(new SaveDocumentError("error-id", "error-message"));

            _saveDocumentService
                .Setup(s => s.Save(It.IsAny<IFormFile>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            // Act 
            var result = await _controller.ProofOfAddressUpload(null, default);
            var viewResult = result as ViewResult;

            // Assert 
            viewResult?.ViewName.Should().Be("ProofOfAddressUpload");
            var model = viewResult?.Model as PortalViewModel<OrganisationModel>;
            model?.Model.Value.Error.Should().Be("error-message");
        }

        [Fact]
        public async Task UploadProofOfAddressConfirmValidatesOptionsSelected()
        {
            // Arrange 
            SetUpRedis();
            SetUpSaveDocumentService();

            // Act 
            var result = await _controller.ProofOfAddressUploadConfirm(null, default);
            var viewResult = result as ViewResult;

            // Assert 
            viewResult.Should().NotBeNull();
            viewResult?.ViewName.Should().Be("ProofOfAddressUploadConfirm");
            var model = viewResult?.Model as PortalViewModel<OrganisationModel>;
            model?.Model.Value.Error.Should().Be("Select an option");
        }

        [Fact]
        public async Task UploadProofOfAddressConfirmOptionSelectedIsYes()
        {
            // Arrange 
            SetUpRedis();
            SetUpSaveDocumentService();

            // Act 
            var result = await _controller.ProofOfAddressUploadConfirm("Yes", default);
            var viewResult = result as RedirectToActionResult;

            // Assert 
            viewResult.Should().NotBeNull();
            viewResult.ActionName.Should().Be("CheckAnswers");
        }

        [Fact]
        public async Task UploadProofOfAddressConfirmOptionSelectedIsNo()
        {
            // Arrange 
            SetUpRedis();
            SetUpSaveDocumentService();

            // Act 
            var result = await _controller.ProofOfAddressUploadConfirm("No", default);
            var viewResult = result as RedirectToActionResult;

            // Assert 
            viewResult.Should().NotBeNull();
            viewResult.ActionName.Should().Be("ProofOfAddressUpload");
            _deleteDocumentService.Verify(d => d.Delete(It.IsAny<string>(), It.IsAny<CancellationToken>()));
        }

        #region Letter Of Authority
        [Fact]
        public async Task UploadLetterOfAuthoritySavesFileName()
        {
            // Arrange
            var filename = "Filename.png";
            SetUpRedis();
            SetUpSaveDocumentService();

            // Act
            var formFile = new FormFile(default, 0, 0, filename, filename);
            await _controller.LetterOfAuthorityUpload(formFile, default, new PortalViewModel<OrganisationModel>());

            // Assert
            _mockRedisStore.Verify(
                r => r.SaveOrgRegistrationAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.Is<OrganisationValue>(model => model.LetterOfAuthorisation.FileName == filename), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UploadLetterOfAuthoritySavesFileId()
        {
            // Arrange
            var fileId = "1234";
            SetUpRedis();
            SetUpSaveDocumentService(fileId);

            // Act
            var formFile = new FormFile(default, 0, 0, "", "");
            await _controller.LetterOfAuthorityUpload(formFile, default, new PortalViewModel<OrganisationModel>());

            // Assert
            _mockRedisStore.Verify(
                r => r.SaveOrgRegistrationAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.Is<OrganisationValue>(model => model.LetterOfAuthorisation.FileId == fileId), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UploadConfirmValidatesLetterOfAuthorityProvided()
        {
            // Arrange
            SetUpRedis();

            var response = new SaveDocumentResponse();

            response.AddError(new SaveDocumentError("error-id", "error-message"));

            _saveDocumentService
                .Setup(s => s.Save(It.IsAny<IFormFile>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            // Act 
            var result = await _controller.LetterOfAuthorityUpload(null, default, new PortalViewModel<OrganisationModel>());
            var viewResult = result as ViewResult;

            // Assert 
            viewResult?.ViewName.Should().Be("LetterOfAuthorityUpload");
            var model = viewResult?.Model as PortalViewModel<OrganisationModel>;
            model?.Model.Value.Error.Should().Be("error-message");
        }

        [Fact]
        public async Task LetterOfAuthorityUploadConfirmValidatesOptionsSelected()
        {
            // Arrange 
            SetUpRedis();
            SetUpSaveDocumentService();

            // Act 
            var result = await _controller.LetterOfAuthorityUploadConfirm(null, default, new PortalViewModel<OrganisationModel>());
            var viewResult = result as ViewResult;

            // Assert 
            viewResult.Should().NotBeNull();
            viewResult?.ViewName.Should().Be("LetterOfAuthorityUploadConfirm");
            var model = viewResult?.Model as PortalViewModel<OrganisationModel>;
            model?.Model.Value.Error.Should().Be("Select an option");
        }

        [Fact]
        public async Task LetterOfAuthorityUploadConfirmOptionSelectedIsYes()
        {
            // Arrange 
            SetUpRedis();
            SetUpSaveDocumentService();

            // Act 
            var result = await _controller.LetterOfAuthorityUploadConfirm("Yes", default, new PortalViewModel<OrganisationModel>());
            var viewResult = result as RedirectToActionResult;

            // Assert 
            viewResult.Should().NotBeNull();
            viewResult.ActionName.Should().Be("PhotoIdUpload");
        }

        [Fact]
        public async Task LetterOfAuthorityUploadConfirmOptionSelectedIsNo()
        {
            // Arrange 
            SetUpRedis();
            SetUpSaveDocumentService();

            // Act 
            var result = await _controller.LetterOfAuthorityUploadConfirm("No", default);
            var viewResult = result as RedirectToActionResult;

            // Assert 
            viewResult.Should().NotBeNull();
            viewResult.ActionName.Should().Be("LetterOfAuthorityUpload");
            _deleteDocumentService.Verify(d => d.Delete(It.IsAny<string>(), It.IsAny<CancellationToken>()));
        }

        #endregion

        #region Companies House lookup tests

        [Fact]
        public async Task ShouldReturnRedirectToActionWhenRedisModelIsNull()
        {
            _mockRedisStore
                .Setup(r => r.GetOrgRegistrationAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<PortalViewModel<OrganisationModel>>(null));

            var result = await _controller.Start(_registerOrganisationModel);
            var viewResult = result as RedirectToActionResult;

            // Assert 
            viewResult.Should().NotBeNull();
            viewResult.ActionName.Should().Be("Index");
        }

        [Fact]
        public async Task ShouldReturnRedirectToActionWhenRedisModelIsNotNull()
        {
            _registerOrganisationModel.ReturnUrl = "return-url";

            _mockRedisStore
                .Setup(r => r.GetOrgRegistrationAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<PortalViewModel<OrganisationModel>>(_registerOrganisationModel));

            var result = await _controller.Start(_registerOrganisationModel);
            var viewResult = result as ViewResult;

            // Assert 
            viewResult.Should().NotBeNull();
            viewResult.ViewName.Should().Be("Start");
        }

        #endregion

        private void SetUpRedis()
        {
            _mockRedisStore.Setup(s => s.GetOrgRegistrationAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(_registerOrganisationModel));
            _mockRedisStore.Setup(s => s.SaveOrgRegistrationAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(true));
        }

        private void SetUpSaveDocumentService(string documentId = "")
        {
            _saveDocumentService.Setup(s => s.Save(It.IsAny<IFormFile>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new SaveDocumentResponse() { DocumentId = documentId });
        }
    }
}
