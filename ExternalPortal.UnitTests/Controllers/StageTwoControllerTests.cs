using ExternalPortal.Constants;
using ExternalPortal.Controllers;
using ExternalPortal.Extensions;
using ExternalPortal.Models;
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Xunit;

namespace ExternalPortal.UnitTests.Controllers
{
    public class StageTwoControllerTests : BaseFileUploadControllerTests, IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly StageTwoController _controller;
        private readonly Mock<ILogger<StageTwoController>> _logger;
        private readonly Mock<IRedisCacheService> _mockRedisCacheService;
        private readonly Mock<ClaimsPrincipal> _userMock;
        private readonly Mock<UserManager<IdentityUser>> _mockUserManager;

        public StageTwoControllerTests(WebApplicationFactory<Startup> fixture) : base(fixture)
        {
            _logger = new Mock<ILogger<StageTwoController>>();
            _mockRedisCacheService = new Mock<IRedisCacheService>();
            var store = new Mock<IUserStore<IdentityUser>>();
            _mockUserManager = new Mock<UserManager<IdentityUser>>(store.Object, null, null, null, null, null, null, null, null);

            _controller = new StageTwoController(_logger.Object, _mockRedisCacheService.Object);

            var _nameIdentifierValue = "326fa974-7c05-4b37-a8e4-6d5fe6deb63b";
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
        }


        [Fact]
        public void PostStage2Confirmation_WhenCalled_ReturnsViewResultType_ConfirmationViewModel()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            _controller.ControllerContext.HttpContext.User = _userMock.Object;

            // Act
            var result = _controller.Stage2Confirmation();

            // Assert
            result.Should().BeOfType<ViewResult>();
            ((ViewResult)result).ViewName.Should().Be("~/Views/Shared/Confirmation.cshtml");

            var model = ((ViewResult)result).Model;
            model.Should().BeOfType<ConfirmationViewModel>();

            ((ConfirmationViewModel)model).BackAction.Should().Be("/task-list");
        }
    }
}
