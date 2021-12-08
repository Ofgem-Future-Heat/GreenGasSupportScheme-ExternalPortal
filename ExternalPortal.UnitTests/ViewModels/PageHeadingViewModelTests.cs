using ExternalPortal.ViewModels.Shared.Components;
using FluentAssertions;
using Xunit;

namespace ExternalPortal.UnitTests.ViewModels
{
    public class PageHeadingViewModelTests
    {
        [Theory]
        [InlineData("Add Responsible Person")]
        public void PageHeadingViewModel_CalledWithHeading_ReturnsCorrectHeading(string heading)
        {
            // Act
            var viewModel = new PageHeadingViewModel(heading);

            // Assert
            viewModel.Heading.Should().Be(heading);
        }
    }
}
