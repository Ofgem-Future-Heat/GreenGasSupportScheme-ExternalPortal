using System.Linq;
using ExternalPortal.ViewModels.Shared.Components;
using FluentAssertions;
using Xunit;

namespace ExternalPortal.UnitTests.ViewModels
{
    public class FormButtonViewModelTests
    {
        [Fact]
        public void FormButtonsViewModel_CalledEmptyConstructor_ReturnsSingleDefaultButton()
        {
            // Arrange

            // Act
            var viewModel = new FormButtonsViewModel();

            // Assert
            viewModel.Buttons.Count().Should().Be(1);
            viewModel.Message.Should().Be(null);
            viewModel.CancelLinkUrl.Should().Be(null);
        }

        [Fact]
        public void FormButtonsViewModel_CalledWithButtonText_ReturnsCorrectButtonText()
        {
            // Arrange
            const string buttonText = "Continue";

            // Act
            var viewModel = new FormButtonsViewModel(buttonText);

            // Assert
            viewModel.Buttons.First().Text.Should().Be(buttonText);
            viewModel.Message.Should().Be(null);
            viewModel.CancelLinkUrl.Should().Be(null);
        }

        [Fact]
        public void FormButtonsViewModel_CalledWithButtonTextAndStyle_ReturnsCorrectButtonTextAndCssStyle()
        {
            // Arrange
            const string buttonText = "Edit";
            const string style = "ggs-style";

            // Act
            var viewModel = new FormButtonsViewModel(buttonText, style);

            // Assert
            viewModel.Buttons.First().Text.Should().Be(buttonText);
            viewModel.Buttons.First().CssClasses.Should().Be(style);
            viewModel.Message.Should().Be(null);
            viewModel.CancelLinkUrl.Should().Be(null);
        }
    }
}
