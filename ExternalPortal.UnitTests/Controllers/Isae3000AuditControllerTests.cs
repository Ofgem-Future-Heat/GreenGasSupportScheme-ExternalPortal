using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using ExternalPortal.Constants;
using ExternalPortal.Controllers;
using ExternalPortal.Extensions;
using ExternalPortal.Services;
using ExternalPortal.ViewModels;
using ExternalPortal.ViewModels.Tasks;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using Moq;
using Ofgem.API.GGSS.Domain.ModelValues;
using Ofgem.API.GGSS.Domain.ModelValues.StageTwo;
using Xunit;
using Times = Moq.Times;

namespace ExternalPortal.UnitTests.Controllers
{
    public class Isae3000AuditControllerTests : BaseFileUploadControllerTests, IClassFixture<WebApplicationFactory<Startup>>
    {
        private Isae3000AuditController _controller;
        private Mock<IUrlHelper> _urlHelper;
        private Mock<HttpContext> _httpContext;
        private Mock<ILogger<Isae3000AuditController>> _logger;
        private Mock<ISaveDocumentService> _saveDocumentService;
        private Mock<IDeleteDocumentService> _deleteDocumentService;
        private Mock<IGetApplicationService> _getApplicationService;
        private Mock<IUpdateApplicationService> _updateApplicationService;
        private Mock<ClaimsPrincipal> _userMock;
        private InstallationModel _newInstallationModel;
        private string _organisationId;
        private string _newInstallationCookie;
        private string _nameIdentifierValue;
        private Guid _organisationGuid;

        public Isae3000AuditControllerTests(WebApplicationFactory<Startup> fixture) : base(fixture)
        {
            SetupTestPrerequisites();
        }

        [Fact]
        public async Task ProgressingPastIsaeUploadStepMarksIsaeTaskAsStarted()
        {
            SetUpTheController();

            await _controller.Upload();

            _updateApplicationService.Verify(
                a => a.Update(It.IsAny<UpdateApplicationRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UploadSavesIsaeFileName()
        {
            const string fileName = "bob.png";

            var formFile = new FormFile(default, 0, 0, fileName, fileName);

            SetUpTheControllerWithFile(formFile);

            _getApplicationService
                .Setup(a => a.Get(It.IsAny<GetApplicationRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetApplicationResponse()
                {
                    Application = new ApplicationValue()
                    {
                        StageTwo = new StageTwoValue()
                        {
                            Isae3000 = new Isae3000Value()
                            {
                                Document = new DocumentValue()
                            }
                        }
                    }
                });

            _saveDocumentService.Setup(s => s.Save(It.IsAny<IFormFile>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new SaveDocumentResponse() { DocumentId = "document-id" });

            await _controller.Upload(formFile, default);

            _updateApplicationService.Verify(
                a => a.Update(It.IsAny<UpdateApplicationRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        }
        
        [Fact]
        public async Task UploadSavesIsaeDocumentId()
        {
            const string documentId = "1234";
            const string fileName = "bob.png";

            var formFile = new FormFile(default, 0, 0, fileName, fileName);

            SetUpTheControllerWithFile(formFile);

            _getApplicationService
                .Setup(a => a.Get(It.IsAny<GetApplicationRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetApplicationResponse()
                {
                    Application = new ApplicationValue()
                    {
                        StageTwo = new StageTwoValue()
                        {
                            Isae3000 = new Isae3000Value()
                            {
                                Document = new DocumentValue()
                            }
                        }
                    }
                });

            _saveDocumentService
                .Setup(s => s.Save(It.IsAny<IFormFile>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new SaveDocumentResponse() { DocumentId = documentId });

            await _controller.Upload(formFile, default);

            _updateApplicationService.Verify(
                a => a.Update(It.IsAny<UpdateApplicationRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UploadValidatesFileProvided()
        {
            SetUpTheControllerWithFile(null);

            var response = new SaveDocumentResponse();

            response.AddError(new SaveDocumentError("error-id", "error-message"));

            _saveDocumentService
               .Setup(s => s.Save(It.IsAny<IFormFile>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(response);

            var result = await _controller.Upload(null, default);
            var viewResult = result as ViewResult;

            viewResult?.ViewName.Should().Be("Upload");
            var model = viewResult?.Model as Isae3000AuditModel;

            model.HasError().Should().BeTrue();
        }

        [Fact]
        public async Task UploadConfirmationValidatesOptions()
        {
            SetUpTheController();

            SetUpSaveDocumentService();

            var result = await _controller.UploadConfirm(null, CancellationToken.None);
            var viewResult = result as ViewResult;

            viewResult.Should().NotBeNull();
            viewResult.ViewName.Should().Be("UploadConfirm");

            var model = viewResult.Model as Isae3000AuditModel;
            model.HasError().Should().BeTrue();
        }

        [Fact]
        public async Task ConfirmingAnswersMarksIsaeTaskAsComplete()
        {
            SetUpTheController();

            await _controller.SubmitYourAnswers();

            _updateApplicationService.Verify(
                a => a.Update(It.IsAny<UpdateApplicationRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        }
        
        private void SetUpSaveDocumentService(string documentId = "")
        {
            _saveDocumentService.Setup(s => s.Save(It.IsAny<IFormFile>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new SaveDocumentResponse() {DocumentId = documentId});
        }

        private void SetUpTheController(string fileName = "", long fileSize = 0)
        {
            var formFile = new FormFile(default, 0, fileSize, fileName, fileName);

            SetUpTheControllerWithFile(formFile);
        }
        
        private void SetUpTheControllerWithFile(FormFile formFile)
        {
            _controller.ControllerContext = new ControllerContext {HttpContext = new DefaultHttpContext()};
            _controller.ControllerContext.HttpContext.Request.Headers.Add(HeaderNames.Cookie, _newInstallationCookie);
            _controller.ControllerContext.HttpContext.Request.Form =
                new FormCollection(new Dictionary<string, StringValues>(), new FormFileCollection()
                {
                    formFile
                });

            _controller.ControllerContext.HttpContext.Request.QueryString = new QueryString($"?ApplicationId={_newInstallationModel.Application.Id}");

            _controller.ControllerContext.HttpContext.User = _userMock.Object;
        }
        
        private void SetupTestPrerequisites()
        {
            _urlHelper = new Mock<IUrlHelper>();
            _httpContext = new Mock<HttpContext>();
            _logger = new Mock<ILogger<Isae3000AuditController>>();
            _saveDocumentService = new Mock<ISaveDocumentService>();
            _deleteDocumentService = new Mock<IDeleteDocumentService>();
            _getApplicationService = new Mock<IGetApplicationService>();
            _updateApplicationService = new Mock<IUpdateApplicationService>();

            _controller = new Isae3000AuditController(
                _logger.Object,
                _saveDocumentService.Object,
                _deleteDocumentService.Object,
                _getApplicationService.Object,
                _updateApplicationService.Object)
            {
                Url = _urlHelper.Object,
                ControllerContext = new ControllerContext
                {
                    HttpContext = _httpContext.Object
                },
                TempData = A.Fake<TempDataDictionary>()
            };

            _getApplicationService.Setup(s => s.Get(It.IsAny<GetApplicationRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetApplicationResponse
                {
                    Application = new ApplicationValue()
                });

            _nameIdentifierValue = "326fa974-7c05-4b37-a8e4-6d5fe6deb63b";
            _userMock = new Mock<ClaimsPrincipal>();
            _userMock.SetupGet(u => u.Claims)
                .Returns(new List<Claim> {new Claim(ClaimTypes.NameIdentifier, _nameIdentifierValue)}.AsEnumerable());

            _organisationId = "b141ac41-d6f7-47fd-b31e-847d77134fca";
            _organisationGuid = Guid.Parse(_organisationId);
            _newInstallationModel = new InstallationModel
            {
                Id = Guid.Parse("b0c2fe16-7a68-42fb-986d-13df95084625"),
                OrganisationId = _organisationGuid,
                UserId = Guid.Parse(_userMock.Object.GetUserId().ToString())
            };

            _newInstallationCookie = $"{CookieKeys.NewInstallation}={_newInstallationModel.Id}";
        }
    }
}