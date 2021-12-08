using ExternalPortal.ViewModels.Shared.Components;
using FluentAssertions;
using Xunit;

namespace ExternalPortal.UnitTests.ViewModels
{
    public class BreadcrumbViewModelTests
    {
        [Fact]
        public void BreadcrumbViewModel_CalledEmptyConstructor_ReturnsUninitializedProperties()
        {
            // Arrange
            // Act
            var viewModel = new BreadcrumbViewModel();

            // Assert
            viewModel.Text.Should().Be(null);
            viewModel.Url.Should().Be(null);
        }
    }
}
