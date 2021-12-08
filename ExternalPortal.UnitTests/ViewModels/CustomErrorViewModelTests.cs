using System.Net;
using ExternalPortal.ViewModels;
using FluentAssertions;
using Xunit;

namespace ExternalPortal.UnitTests.ViewModels
{
    public class CustomErrorViewModelTests
    {
        [Theory]
        [InlineData(HttpStatusCode.Unauthorized, "You are not currently logged in, and cannot view this page as a guest.")]
        [InlineData(HttpStatusCode.Forbidden, "You don't have the correct permissions to access this page for the current organisation. Please get in touch with this organisation's administrators to request access.")]
        public void ErrorViewModel_CalledWithStatusCode_ReturnsCorrectText(HttpStatusCode statusCode, string expectedText)
        {
            // Act
            var viewModel = new CustomErrorViewModel(statusCode);

            // Assert
            viewModel.ErrorText.Should().Be(expectedText);
            viewModel.StatusCode.Should().Be(statusCode);
        }
    }
}
