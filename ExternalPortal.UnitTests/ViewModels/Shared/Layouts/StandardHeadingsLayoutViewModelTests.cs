using ExternalWebApp.ViewModels.Shared.Layouts;
using FluentAssertions;
using Xunit;

namespace ExternalPortal.UnitTests.ViewModels.Shared.Layouts
{
    public  class StandardHeadingsLayoutViewModelTests
    {
        [Fact]
        public void StandardHeadingsLayoutViewModel_CalledEmptyConstructor_ReturnsUninitializedProperties()
        {
            // Arrange
            // Act
            var viewModel = new StandardHeadingsLayoutViewModel();

            // Assert
            viewModel.Breadcrumbs.Should().BeNull();
            viewModel.BackLink.Should().Be(null);
            viewModel.ConfirmationText.Should().Be(null);
            viewModel.PageHeading.Should().Be(null);
        }
    }
}
