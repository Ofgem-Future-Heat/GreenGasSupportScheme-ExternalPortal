using ExternalPortal.ViewModels.Shared.Components;
using FluentAssertions;
using System.Collections.Generic;
using Xunit;

namespace ExternalPortal.UnitTests.ViewModels
{
    public class BackLinkViewModelTests
    {
        [Fact]
        public void BackLinkViewModel_CalledFromUrl_ReturnsCorrectProperties()
        {
            // Arrange
            var backHref = "/Home/Index";

            // Act
            var backLinkViewModel = BackLinkViewModel.FromUrl(backHref);

            // Assert
            backLinkViewModel.Href.Should().Be(backHref);
            backLinkViewModel.Action.Should().Be(null);
            backLinkViewModel.Controller.Should().Be(null);
            backLinkViewModel.Area.Should().Be(null);
            backLinkViewModel.ReferenceParams.Should().BeNull();
        }

        [Fact]
        public void BackLinkViewModel_CalledFromAction_ReturnsCorrectProperties()
        {
            // Arrange
            const string area = "base";
            const string controller = "home";
            const string action = "index";

            const string routeParamKey = "responsiblePersonIsYou";
            string routeParamKeyValue = true.ToString();
            IDictionary<string, string> referenceParams = new Dictionary<string, string> { { routeParamKey, routeParamKeyValue } };

            // Act
            var backLinkViewModel = BackLinkViewModel.FromAction(action, controller, area, referenceParams);

            // Assert
            backLinkViewModel.Href.Should().Be(null);
            backLinkViewModel.Action.Should().Be(action);
            backLinkViewModel.Controller.Should().Be(controller);
            backLinkViewModel.Area.Should().Be(area);

            backLinkViewModel.ReferenceParams.Should().HaveCount(1);
            backLinkViewModel.ReferenceParams.Should().ContainKey(routeParamKey);
            backLinkViewModel.ReferenceParams.Should().ContainKey(routeParamKey).WhichValue.Should().Be(routeParamKeyValue);
        }
    }
}