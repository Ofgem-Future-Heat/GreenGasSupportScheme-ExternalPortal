using ExternalPortal.ViewModels.Shared.Components;
using FluentAssertions;
using Xunit;

namespace ExternalPortal.UnitTests.ViewModels
{
    public class FormButtonsViewModelTests
    {
        [Fact]
        public void FormsButtonViewModel_EmptyConstructor_ReturnsDefaultValueForText()
        {
            // Act
            var viewModel = new FormButtonViewModel();

            // Assert
            viewModel.Text.Should().Be("Submit");
            viewModel.Value.Should().Be(null);
            viewModel.Name.Should().Be(null);
            viewModel.Href.Should().Be(null);
            viewModel.Disabled.Should().Be(false);
            viewModel.CssClasses.Should().Be(null);
        }
    }
}
