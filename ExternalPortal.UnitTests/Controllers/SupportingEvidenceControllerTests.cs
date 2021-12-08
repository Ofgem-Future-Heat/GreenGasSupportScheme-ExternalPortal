using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
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
using Moq;
using Ofgem.API.GGSS.Domain.ModelValues;
using Ofgem.API.GGSS.Domain.ModelValues.StageTwo;
using Xunit;
using Times = Moq.Times;

namespace ExternalPortal.UnitTests.Controllers
{
    public class SupportingEvidenceControllerTests : BaseFileUploadControllerTests, IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly SupportingEvidenceController _controller;
        private readonly Mock<IUrlHelper> _urlHelper;
        private readonly Mock<HttpContext> _httpContext;
        private readonly Mock<ILogger<SupportingEvidenceController>> _logger;
        private readonly Mock<ISaveDocumentService> _saveDocumentService;
        private readonly Mock<IDeleteDocumentService> _deleteDocumentService;
        private readonly Mock<IGetApplicationService> _getApplicationService;
        private readonly Mock<IUpdateApplicationService> _updateApplicationService;
        private readonly Mock<ClaimsPrincipal> _userMock;
        private readonly InstallationModel _newInstallationModel;
        private readonly string _organisationId;
        private readonly string _nameIdentifierValue;
        private readonly Guid _organisationGuid;

        public SupportingEvidenceControllerTests(WebApplicationFactory<Startup> fixture) : base(fixture)
        {
            _urlHelper = new Mock<IUrlHelper>();
            _httpContext = new Mock<HttpContext>();
            _logger = new Mock<ILogger<SupportingEvidenceController>>();
            _saveDocumentService = new Mock<ISaveDocumentService>();
            _deleteDocumentService = new Mock<IDeleteDocumentService>();
            _getApplicationService = new Mock<IGetApplicationService>();
            _updateApplicationService = new Mock<IUpdateApplicationService>();

            _getApplicationService.Setup(s => s.Get(It.IsAny<GetApplicationRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetApplicationResponse
                {
                    Application = new ApplicationValue()
                    {
                        StageTwo = new StageTwoValue()
                        {
                            AdditionalSupportingEvidence = new AdditionalSupportingEvidenceValue()
                            {
                                AdditionalSupportingEvidenceDocuments = new List<DocumentValue>()
                                {
                                    new DocumentValue()
                                    {
                                        FileId = "document-id",
                                        FileName = "fake-document"
                                    }
                                }
                            }
                        }
                    }
                });

            _nameIdentifierValue = "326fa974-7c05-4b37-a8e4-6d5fe6deb63b";
            _userMock = new Mock<ClaimsPrincipal>();
            _userMock.SetupGet(u => u.Claims)
                .Returns(new List<Claim> { new Claim(ClaimTypes.NameIdentifier, _nameIdentifierValue) }.AsEnumerable());

            _organisationId = "b141ac41-d6f7-47fd-b31e-847d77134fca";
            _organisationGuid = Guid.Parse(_organisationId);
            _newInstallationModel = new InstallationModel
            {
                Id = Guid.Parse("b0c2fe16-7a68-42fb-986d-13df95084625"),
                OrganisationId = _organisationGuid,
                UserId = Guid.Parse(_userMock.Object.GetUserId().ToString())
            };

            _controller = new SupportingEvidenceController(
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

            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            _controller.ControllerContext.HttpContext.Request.QueryString = new QueryString($"?ApplicationId={_newInstallationModel.Application.Id}");
        }

        [Fact]
        public void ShouldReturnWhatYouWillNeedAction()
        {
            // Act
            var result = _controller.WhatYouWillNeed();

            // Assert
            result.Should().BeOfType<ViewResult>();
            ((ViewResult)result).ViewName.Should().Be("WhatYouWillNeed");
        }

        [Fact]
        public async Task ShouldReturnErrorMessageWhenNoAddEvidenceSelected()
        {
            var result = await _controller.AddSupportingEvidence(null, CancellationToken.None);

            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;

            viewResult.ViewName.Should().Be("AddSupportingEvidence");

            var model = viewResult?.Model as AdditionalFinancialEvidenceModel;

            model.HasError().Should().BeTrue();
        }

        [Fact]
        public async Task ShouldReturnRedirectToActionWhenAddEvidenceNoSelected()
        {
            var result = await _controller.AddSupportingEvidence("no", CancellationToken.None);

            result.Should().BeOfType<RedirectToActionResult>();
            var viewResult = result as RedirectToActionResult;

            viewResult.ActionName.Should().Be("CheckYourAnswers");
        }

        [Fact]
        public async Task ShouldReturnRedirectToActionWhenAddEvidenceYesSelected()
        {
            var result = await _controller.AddSupportingEvidence("yes", CancellationToken.None);

            result.Should().BeOfType<RedirectToActionResult>();
            var viewResult = result as RedirectToActionResult;

            viewResult.ActionName.Should().Be("Upload");
        }

        [Fact]
        public async Task ShouldReturnUploadActionWhenCalledViaGet()
        {
            // Act
            var result = await _controller.Upload();

            // Assert
            result.Should().BeOfType<ViewResult>();

            var viewResult = result as ViewResult;

            viewResult?.ViewName.Should().Be("Upload");
        }

        [Fact]
        public async Task ShouldReturnErrorMessageWhenNoFileIsUploaded()
        {
            var saveResponse = new SaveDocumentResponse();

            saveResponse.AddError(new SaveDocumentError("error-id", "error-message"));

            _saveDocumentService.Setup(s => s.Save(It.IsAny<IFormFile>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(saveResponse);

            var result = await _controller.Upload(null, CancellationToken.None);

            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;

            viewResult.ViewName.Should().Be("Upload");

            var model = viewResult?.Model as AdditionalFinancialEvidenceModel;

            model.HasError().Should().BeTrue();
            model.Error.Should().Be("error-message");
        }

        [Fact]
        public async Task ShouldReturnRedirectToActionWhenFileIsUploaded()
        {
            const string fileName = "bob.png";

            var formFile = new FormFile(default, 0, 0, fileName, fileName);

            var saveResponse = new SaveDocumentResponse() { DocumentId = "document-id" };

            _saveDocumentService.Setup(s => s.Save(It.IsAny<IFormFile>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(saveResponse);

            var result = await _controller.Upload(formFile, CancellationToken.None);

            result.Should().BeOfType<RedirectToActionResult>();
            var viewResult = result as RedirectToActionResult;

            viewResult.ActionName.Should().Be("UploadConfirm");

            _updateApplicationService.Verify(
                    a => a.Update(It.IsAny<UpdateApplicationRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ShouldReturnErrorMessageWhenNoOptionIsSelected ()
        {
            var result = await _controller.UploadConfirm(null, "document-id", CancellationToken.None);

            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;

            viewResult.ViewName.Should().Be("UploadConfirm");

            var model = viewResult?.Model as AdditionalFinancialEvidenceModel;

            model.HasError().Should().BeTrue();
            model.Error.Should().Be("Select an option");
        }

        [Fact]
        public async Task ShouldReturnRedirectToUploadWhenOptionNoIsSelected()
        {
            var result = await _controller.UploadConfirm("no", "document-id", CancellationToken.None);

            result.Should().BeOfType<RedirectToActionResult>();
            var viewResult = result as RedirectToActionResult;

            viewResult.ActionName.Should().Be("Upload");
        }

        [Fact]
        public async Task ShouldReturnRedirectToUploadWhenOptionYesIsSelected()
        {
            var result = await _controller.UploadConfirm("yes", "document-id", CancellationToken.None);

            result.Should().BeOfType<RedirectToActionResult>();
            var viewResult = result as RedirectToActionResult;

            viewResult.ActionName.Should().Be("AddReference");
        }

        [Fact]
        public async Task ShouldReturnErrorMessageWhenNoReferenceIsProvided()
        {
            var result = await _controller.AddReference(null, "document-id", CancellationToken.None);

            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;

            viewResult.ViewName.Should().Be("AddReference");

            var model = viewResult?.Model as AdditionalFinancialEvidenceModel;

            model.HasError().Should().BeTrue();
            model.Error.Should().Be("Enter a reference");
        }

        [Fact]
        public async Task ShouldReturnRedirectWhenReferenceIsProvided()
        {
            var result = await _controller.AddReference("reference", "document-id", CancellationToken.None);

            result.Should().BeOfType<RedirectToActionResult>();
            var viewResult = result as RedirectToActionResult;

            viewResult.ActionName.Should().Be("UploadMore");
        }

        [Fact]
        public async Task ShouldReturnErrorMessageWhenNoOptionIsSelectedForUploadMore()
        {
            var result = await _controller.UploadMore(null, CancellationToken.None);

            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;

            viewResult.ViewName.Should().Be("UploadMore");

            var model = viewResult?.Model as AdditionalFinancialEvidenceModel;

            model.HasError().Should().BeTrue();
            model.Error.Should().Be("Select an option");
        }

        [Fact]
        public async Task ShouldReturnRedirectWhenNoOptionIsProvidedForUploadMore()
        {
            var result = await _controller.UploadMore("no", CancellationToken.None);

            result.Should().BeOfType<RedirectToActionResult>();
            var viewResult = result as RedirectToActionResult;

            viewResult.ActionName.Should().Be("CheckYourAnswers");
        }

        [Fact]
        public async Task ShouldReturnRedirectWhenYesOptionIsProvidedForUploadMore()
        {
            var result = await _controller.UploadMore("yes",  CancellationToken.None);

            result.Should().BeOfType<RedirectToActionResult>();
            var viewResult = result as RedirectToActionResult;

            viewResult.ActionName.Should().Be("Upload");
        }

        [Fact]
        public async Task ShouldRemoveDocumentsFromCollectionPreviouslyUploadedWhenNoAddEvidenceSelected()
        {
            _getApplicationService.Setup(s => s.Get(It.IsAny<GetApplicationRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetApplicationResponse
                {
                    Application = new ApplicationValue()
                    {
                        StageTwo = new StageTwoValue()
                        {
                            AdditionalSupportingEvidence = new AdditionalSupportingEvidenceValue()
                            {
                                AdditionalSupportingEvidenceDocuments = new List<DocumentValue>()
                                {
                                    new DocumentValue()
                                    {
                                        FileId = "document-id-one",
                                        FileName = "fake-document"
                                    },
                                    new DocumentValue()
                                    {
                                        FileId = "document-id-two",
                                        FileName = "fake-document"
                                    }
                                }
                            }
                        }
                    }
                });

            var result = await _controller.AddSupportingEvidence("no", CancellationToken.None);

            _deleteDocumentService
                .Verify(d => d.Delete(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        }

        [Fact]
        public async Task ShouldHandleAnEmptyDocumentsFromCollectionPreviouslyUploadedWhenNoAddEvidenceSelected()
        {
            _getApplicationService.Setup(s => s.Get(It.IsAny<GetApplicationRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetApplicationResponse
                {
                    Application = new ApplicationValue()
                    {
                        StageTwo = new StageTwoValue()
                        {
                            AdditionalSupportingEvidence = new AdditionalSupportingEvidenceValue()
                            {
                                AdditionalSupportingEvidenceDocuments = new List<DocumentValue>()
                            }
                        }
                    }
                });

            var result = await _controller.AddSupportingEvidence("no", CancellationToken.None);

            _deleteDocumentService
                .Verify(d => d.Delete(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task ShouldHandleANullDocumentsFromCollectionPreviouslyUploadedWhenNoAddEvidenceSelected()
        {
            _getApplicationService.Setup(s => s.Get(It.IsAny<GetApplicationRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetApplicationResponse
                {
                    Application = new ApplicationValue()
                    {
                        StageTwo = new StageTwoValue()
                        {
                            AdditionalSupportingEvidence = new AdditionalSupportingEvidenceValue()
                            {
                                AdditionalSupportingEvidenceDocuments = null
                            }
                        }
                    }
                });

            var result = await _controller.AddSupportingEvidence("no", CancellationToken.None);

            _deleteDocumentService
                .Verify(d => d.Delete(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
