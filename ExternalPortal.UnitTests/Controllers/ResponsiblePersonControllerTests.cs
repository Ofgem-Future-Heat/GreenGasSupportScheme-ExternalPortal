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
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;
using Ofgem.API.GGSS.Domain.Enums;
using Ofgem.API.GGSS.Domain.Models;
using Ofgem.API.GGSS.Domain.ModelValues;
using Xunit;

namespace ExternalPortal.UnitTests.Controllers
{
    public class ResponsiblePersonControllerTests : IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly IUrlHelper _urlHelper = A.Fake<IUrlHelper>();
        private readonly HttpContext _httpContext = A.Fake<HttpContext>();
        private readonly ISession _session = A.Fake<ISession>();
        private readonly ILogger<ResponsiblePersonController> _logger = A.Fake<ILogger<ResponsiblePersonController>>();
        private readonly Mock<IResponsiblePersonService> _responsibleService;
        private readonly ResponsiblePersonController _controller;
        private readonly Mock<IRedisCacheService> _mockRedisStore;
        private readonly Mock<ClaimsPrincipal> _userMock;

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

        private readonly ResponsiblePersonModel _defaultCurrentUser = new ResponsiblePersonModel
        {
            User = new UserModel
            {
                Value = new UserValue
                {
                    Name = "JOE",
                    Surname = "Bloggs",
                    EmailAddress = "test@example.com"
                }
            },
            Value = new ResponsiblePersonValue
            {
                TelephoneNumber = "01412265098"
            }
        };

        public ResponsiblePersonControllerTests()
        {
            A.CallTo(() => _urlHelper.Action(A<UrlActionContext>.Ignored)).Returns("fake url");
            A.CallTo(() => _httpContext.Session).Returns(_session);

            _mockRedisStore = new Mock<IRedisCacheService>();
            _responsibleService = new Mock<IResponsiblePersonService>();
            _mockGetUserByProviderId = new Mock<IGetUserByProviderIdService>();

            _mockGetUserByProviderId.Setup(s => s.Get(It.IsAny<GetUserRequest>())).ReturnsAsync(new GetUserResponse()
            {
                Found = true,
                UserId = Guid.NewGuid().ToString()
            });

            _controller = new ResponsiblePersonController(_logger, _responsibleService.Object, _mockGetUserByProviderId.Object, _mockRedisStore.Object)
            {
                Url = _urlHelper,
                ControllerContext = new ControllerContext
                {
                    HttpContext = _httpContext
                },
                TempData = A.Fake<TempDataDictionary>()
            };

            _userMock = new Mock<ClaimsPrincipal>();
            _userMock.SetupGet(u => u.Claims).Returns(new List<Claim> 
            { 
                new Claim(ClaimTypes.NameIdentifier, "DD370D2F-7911-474E-857F-8958145932F1"), 
                new Claim(ClaimTypes.GivenName, _defaultCurrentUser.User.Value.Name), 
                new Claim(ClaimTypes.Surname, _defaultCurrentUser.User.Value.Surname), 
                new Claim(CustomClaimTypes.EmailAddress, _defaultCurrentUser.User.Value.EmailAddress), 
                new Claim(CustomClaimTypes.TelephoneNumber, _defaultCurrentUser.Value.TelephoneNumber) 
            }.AsEnumerable());

        }


        private readonly ResponsiblePersonDetailViewModel _someoneElse = new ResponsiblePersonDetailViewModel
        {
            FirstName = "Steve",
            Surname = "Jobs",
            EmailAddress = "sjobs@apple.com",
            PhoneNumber = "014110151014",
            ResponsiblePersonIsYou = false
        };

        private Mock<IGetUserByProviderIdService> _mockGetUserByProviderId;

        #region "ResponsiblePersonIndex Action Tests"
        [Fact]
        public void GetResponsiblePersonIndex_WhenCalled_ReturnsViewResultTypeWithResponsiblePersonIndexViewModel()
        {
            // Arrange
            // Act
            var result = _controller.Index(default);

            // Assert
            Assert.IsType<ViewResult>(result);
            Assert.Equal("Index", ((ViewResult)result).ViewName);
        }

        [Fact]
        public void PostResponsiblePersonIndex_WhenCalledAndModelStateNotValid_ReturnsViewResultType()
        {
            // Arrange
            _controller.ModelState.AddModelError("test", "test");

            // Act
            var result = _controller.Type(new ResponsiblePersonIndexViewModel());

            // Assert
            Assert.IsType<ViewResult>(result);
            Assert.Equal("Type", ((ViewResult)result).ViewName);
            Assert.IsType<ResponsiblePersonIndexViewModel>(((ViewResult)result).Model);
        }

        [Fact]
        public void PostResponsiblePersonIndex_WhenCalledWithReponsiblePersonTypeYou_ReturnsRedirectToAction()
        {
            // Arrange
            // Act
            var result = _controller.Type(new ResponsiblePersonIndexViewModel()
            {
                ResponsiblePersonType = ResponsiblePersonType.You
            });

            // Assert
            Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Confirmation", ((RedirectToActionResult)result).ActionName);
            Assert.Equal(true, ((RedirectToActionResult)result).RouteValues["responsiblePersonIsYou"]);
        }

        [Fact]
        public void PostResponsiblePersonType_WhenCalledWithReponsiblePersonTypeSomeoneElse_ReturnsRedirectToAction()
        {
            // Arrange
            // Act
            var result = _controller.Type(new ResponsiblePersonIndexViewModel()
            {
                ResponsiblePersonType = ResponsiblePersonType.No
            });

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();
            ((RedirectToActionResult)result).ActionName.Should().Be("NotResponsibleKickout");
        }
        #endregion

        #region "ResponsiblePersonSomeoneElseDetails Action Tests"
        //[Fact]
        //public void GetResponsiblePersonSomeoneElseDetails_WhenCalled_ReturnsViewResultTypeWithResponsiblePersonDetailViewModel()
        //{
        //    // Arrange
        //    // Act
        //    var result = _controller.SomeoneElseDetails();

        //    // Assert
        //    Assert.IsType<ViewResult>(result);
        //    Assert.Equal("SomeoneElseDetails", ((ViewResult)result).ViewName);
        //    Assert.IsType<ResponsiblePersonDetailViewModel>(((ViewResult)result).Model);
        //}

        //[Fact]
        //public void PostResponsiblePersonSomeoneElseDetails_WhenCalledAndModelStateNotValid_ReturnsViewResultType()
        //{
        //    // Arrange
        //    _controller.ModelState.AddModelError("test", "test");

        //    // Act
        //    var result = _controller.SomeoneElseDetails(new ResponsiblePersonDetailViewModel());

        //    // Assert
        //    Assert.IsType<ViewResult>(result);
        //    Assert.Equal("SomeoneElseDetails", ((ViewResult)result).ViewName);
        //    Assert.IsType<ResponsiblePersonDetailViewModel>(((ViewResult)result).Model);
        //}

        //[Fact]
        //public void PostResponsiblePersonSomeoneElseDetails_WhenCalledWithValidModel_ReturnsRedirectToAction()
        //{
        //    // Arrange
        //    // Act
        //    var result = _controller.SomeoneElseDetails(_someoneElse);

        //    // Assert
        //    Assert.IsType<RedirectToActionResult>(result);
        //    Assert.Equal("Confirmation", ((RedirectToActionResult)result).ActionName);
        //}
        #endregion

        #region "ResponsiblePersonConfirmation Action Tests"
        [Fact]
        public async Task GetResponsiblePersonConfirmation_WhenCalled_ReturnsRedirectToActionResultType()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() { User = _userMock.Object } };
            _responsibleService.Setup(r => r.GetAsync(default)).Returns(Task.FromResult(_defaultCurrentUser));

            // Act
            var result = await _controller.Confirmation(true, default);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();
            ((RedirectToActionResult)result).ActionName.Should().Be("ChooseType");
            ((RedirectToActionResult)result).ControllerName.Should().Be("Organisation");
        }
        #endregion

        #region "Date Of Birth Action Tests"
        [Fact]
        public async Task GetResponsiblePersonDateOfBirth_WhenCalled__ReturnsViewResultTypeWithResponsiblePersonDetailViewModel()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() { User = _userMock.Object } };

            // Act
            var result = await _controller.DateOfBirth(default);

            // Assert
            result.Should().BeOfType<ViewResult>();

            ((ViewResult)result).ViewName.Should().Be("DateOfBirth");

            var model = ((ViewResult)result).Model;
            model.Should().BeOfType<ResponsiblePersonDetailViewModel>();

        }

        [Fact]
        public async Task PostResponsiblePersonDateOfBirth_WhenCalledAndResponsiblePersonDetailViewModelStateNotValid_ReturnsViewResultType()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() { User = _userMock.Object } };

            var mockResponsiblePerson = new ResponsiblePersonDetailViewModel { DateOfBirth = System.DateTime.Now };

            // Act
            var result = await _controller.DateOfBirth(mockResponsiblePerson, default);

            // Assert
            result.Should().BeOfType<ViewResult>();

            ((ViewResult)result).ViewName.Should().Be("DateOfBirth");

            var model = ((ViewResult)result).Model;
            model.Should().BeOfType<ResponsiblePersonDetailViewModel>();
        }

        [Fact]
        public async Task PostResponsiblePersonDateOfBirth_WhenCalledAndResponsiblePersonDetailViewModelStateIsValid_AndDateOfBirthBelowMinimumAge_ReturnsViewResultType()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() { User = _userMock.Object } };

            var mockResponsiblePerson = new ResponsiblePersonDetailViewModel { DateOfBirth = System.DateTime.Now.AddYears(-16) };

            // Act
            var result = await _controller.DateOfBirth(mockResponsiblePerson, default);

            // Assert
            result.Should().BeOfType<ViewResult>();

            ((ViewResult)result).ViewName.Should().Be("DateOfBirth");

            var model = ((ViewResult)result).Model;
            model.Should().BeOfType<ResponsiblePersonDetailViewModel>();
        }

        [Fact]
        public async Task PostResponsiblePersonDateOfBirth_WhenCalledAndResponsiblePersonDetailViewModelStateIsValid_ReturnsRedirectToActionResultType()
        {
            // Arrange
            _mockOrganisationModel.Model.ResponsiblePeople = new List<ResponsiblePersonModel>() { _defaultCurrentUser };
            _mockRedisStore.Setup(r => r.GetOrgRegistrationAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(_mockOrganisationModel));
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() { User = _userMock.Object } };

            var mockResponsiblePerson = new ResponsiblePersonDetailViewModel { DateOfBirth = System.DateTime.Now.AddYears(-18) };

            // Act
            var result = await _controller.DateOfBirth(mockResponsiblePerson, default);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();
            ((RedirectToActionResult)result).ActionName.Should().Be("PhoneNumber");
        }

        #endregion

        #region "LetterOfAuthorityIndex Action Tests"
        [Fact]
        public void GetLetterOfAuthorityIndex_WhenCalledReturnsViewResultType()
        {
            // Act
            var result = _controller.LetterOfAuthorityIndex();

            // Assert
            Assert.IsType<ViewResult>(result);
            Assert.Equal("Index", ((ViewResult)result).ViewName);
        }
        #endregion
        
        [Fact]
        public async Task GetResponsiblePersonPhoneNumber_WhenCalled__ReturnsViewResultTypeWithResponsiblePersonDetailViewModel()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() { User = _userMock.Object } };

            // Act
            var result = await _controller.PhoneNumber(default);
            var viewResult = result as ViewResult;
            
            // Assert
            viewResult.Should().NotBeNull();
            viewResult?.ViewName.Should().Be("PhoneNumber");

            var model = viewResult?.Model as ResponsiblePersonDetailViewModel;
            model.Should().NotBeNull();
        }
        
        [Fact]
        public async Task PostResponsiblePersonPhoneNumber_WhenCalledAndResponsiblePersonDetailViewModelStateNotValid_ReturnsViewResultType()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() { User = _userMock.Object } };

            var mockResponsiblePerson = new ResponsiblePersonDetailViewModel { PhoneNumber = "abcd" };

            // Act
            var result = await _controller.PhoneNumber(mockResponsiblePerson, default);
            var viewResult = result as ViewResult;

            // Assert
            viewResult.Should().NotBeNull();
            viewResult?.ViewName.Should().Be("PhoneNumber");

            var model = viewResult?.Model as ResponsiblePersonDetailViewModel;
            model.Should().NotBeNull();
        }
        
        [Fact]
        public async Task PostResponsiblePersonPhoneNumber_WhenCalledAndResponsiblePersonDetailViewModelStateIsValid_ReturnsRedirectToActionResultType()
        {
            // Arrange
            _mockOrganisationModel.Model.ResponsiblePeople = new List<ResponsiblePersonModel>() { _defaultCurrentUser };
            _mockRedisStore.Setup(r => r.GetOrgRegistrationAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(_mockOrganisationModel));
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() { User = _userMock.Object } };

            var mockResponsiblePerson = new ResponsiblePersonDetailViewModel { PhoneNumber = "07895673451"};

            // Act
            var result = await _controller.PhoneNumber(mockResponsiblePerson, default);
            var viewResult = result as RedirectToActionResult;

            // Assert
            viewResult.Should().NotBeNull();
            viewResult?.ActionName.Should().Be("LetterOfAuthorityUpload");
        }
    }
}
