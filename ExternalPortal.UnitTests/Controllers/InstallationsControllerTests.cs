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
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Moq;
using Ofgem.Azure.Redis.Data.Contracts;
using Xunit;

namespace ExternalPortal.UnitTests.Controllers
{
    public class InstallationsControllerTests : IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly Mock<ILogger<InstallationsController>> _logger;
        private readonly InstallationsController _controller;
        private readonly Mock<IAzureRedisStore<InstallationModel>> _mockRedisStore;
        private readonly Mock<IRedisCacheService> _mockRedisCache;
        private readonly Mock<UserManager<IdentityUser>> _mockUserManager;

        public InstallationsControllerTests(WebApplicationFactory<Startup> fixture)
        {
            _logger = new Mock<ILogger<InstallationsController>>();
            _mockRedisStore = new Mock<IAzureRedisStore<InstallationModel>>();
            _mockRedisCache = new Mock<IRedisCacheService>();
            _mockUserManager = new Mock<UserManager<IdentityUser>>();
            _controller = new InstallationsController(_logger.Object, _mockRedisStore.Object);
        }

        [Fact]
        public async Task ShouldReturnExceptionWhenOrganisationIdIsMissing()
        {
            Task result() => _controller.StartNewApplication(null);

            await Assert.ThrowsAsync<ArgumentNullException>(result);
        }

        [Fact]
        public async Task StartNewApplication_ShoudReturnIndexViewWithInstallationModel()
        {
            // Arrange 
            var NameIdentifierValue = "326fa974-7c05-4b37-a8e4-6d5fe6deb63b";
            var user = new Mock<ClaimsPrincipal>();
            user.SetupGet(u => u.Claims).Returns(new List<Claim> { new Claim(ClaimTypes.NameIdentifier, NameIdentifierValue) }.AsEnumerable());

            var organisationId = "80CBCE5B-62F0-47D3-98AB-50738775D310";

            var newApplicationTaskListModel = new InstallationModel
            {
                Id = Guid.Parse("b0c2fe16-7a68-42fb-986d-13df95084625"),
                OrganisationId = Guid.Parse(organisationId),
                UserId = user.Object.GetUserId()
            };

            var newInstallationCookie = $"{CookieKeys.NewInstallation}={newApplicationTaskListModel.Id}";

            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            _controller.ControllerContext.HttpContext.Request.Headers.Add(HeaderNames.Cookie, newInstallationCookie);
            _controller.ControllerContext.HttpContext.User = user.Object;

            // Act
            var result = await _controller.StartNewApplication(organisationId);

            // Assert
            result.Should().BeOfType<ViewResult>();
            ((ViewResult)result).ViewName.Should().Be("Index");

            var model = ((ViewResult)result).Model;
            model.Should().BeOfType<InstallationModel>();

            ((InstallationModel)model).Id.Should().NotBeEmpty();
            ((InstallationModel)model).Application.Value.Status.Should().NotBeNull();
            ((InstallationModel)model).StageOne.Should().NotBeNull();
            ((InstallationModel)model).StageTwo.Should().NotBeNull();
            ((InstallationModel)model).StageThree.Should().NotBeNull();
        }

        [Fact]
        public async Task ShouldReturnExceptionWhenOrganisationIdIsMissingFromEdit()
        {
            Task result() => _controller.EditApplication(null, "installation-id", CancellationToken.None);

            await Assert.ThrowsAsync<ArgumentNullException>(result);
        }

        [Fact]
        public async Task ShouldReturnExceptionWhenInstallationIdIsMissingFromEdit()
        {
            Task result() => _controller.EditApplication("organisation-id", null, CancellationToken.None);

            await Assert.ThrowsAsync<ArgumentNullException>(result);
        }

        [Fact]
        public void EditApplication_ShoudReturnIndexViewWithInstallationModel()
        {
            // Arrange 
            var NameIdentifierValue = "326fa974-7c05-4b37-a8e4-6d5fe6deb63b";
            var user = new Mock<ClaimsPrincipal>();
            user.SetupGet(u => u.Claims).Returns(new List<Claim> { new Claim(ClaimTypes.NameIdentifier, NameIdentifierValue) }.AsEnumerable());

            var organisationId = "80CBCE5B-62F0-47D3-98AB-50738775D310";

            var newApplicationTaskListModel = new InstallationModel
            {
                Id = Guid.Parse("b0c2fe16-7a68-42fb-986d-13df95084625"),
                OrganisationId = Guid.Parse(organisationId),
                UserId = user.Object.GetUserId()
            };

            var newInstallationCookie = $"{CookieKeys.NewInstallation}={newApplicationTaskListModel.Id}";

            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            _controller.ControllerContext.HttpContext.Request.Headers.Add(HeaderNames.Cookie, newInstallationCookie);
            _controller.ControllerContext.HttpContext.User = user.Object;

            // Act
            var result = _controller.EditApplication(organisationId, newApplicationTaskListModel.Id.ToString(), default).Result;

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();
            ((RedirectToActionResult)result).ActionName.Should().Be("ListInstallations");

            var routeValues = ((RedirectToActionResult)result).RouteValues;
            routeValues["organisationId"].Should().NotBeNull();
            routeValues["organisationId"].Should().BeOfType(typeof(string));
            ((string)routeValues["organisationId"]).ToString().Should().BeEquivalentTo(organisationId);
        }
    }
}
