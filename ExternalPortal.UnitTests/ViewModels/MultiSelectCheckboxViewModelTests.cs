using ExternalPortal.ViewModels.Shared.Components;
using FluentAssertions;
using Xunit;

namespace ExternalPortal.UnitTests.ViewModels
{
    public class MultiSelectCheckboxViewModelTests
    {
        [Fact]
        public void MultiSelectCheckboxViewModel_CalledEmptyConstructor_ReturnsUninitializedProperties()
        {
            // Arrange

            // Act
            var viewModel = new MultiSelectCheckboxViewModel();

            // Assert
            viewModel.Id.Should().Be(null);
            viewModel.Name.Should().Be(null);
            viewModel.Selected.Should().Be(false);
            viewModel.SelectionClass.Should().Be(null);
        }

    }
}
