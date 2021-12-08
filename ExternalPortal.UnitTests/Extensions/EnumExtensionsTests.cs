using ExternalPortal.Enums;
using ExternalPortal.Extensions;
using FluentAssertions;
using Ofgem.API.GGSS.Domain.Enums;
using Xunit;

namespace ExternalPortal.UnitTests.Extensions
{
    public class EnumExtensionsTests
    {
        [Fact]
        public void GetDisplayName_Returns_TaskStatus_DisplayAttributeName()
        {
            // Arrange 
            var enumValue = TaskStatus.CannotStartYet;

            // Act
            var displayName = enumValue.GetDisplayName();

            // Assert
            displayName.Should().Be("Cannot Start Yet");
        }

        [Fact]
        public void GetDisplayName_Returns_TaskStatus_Default_DisplayAttributeName()
        {
            // Arrange 
            var enumValue = TaskStatus.Approved;

            // Act
            var displayName = enumValue.GetDisplayName();

            // Assert
            displayName.Should().Be("Approved");
        }

        [Fact]
        public void GetDisplayName_Returns_ApplicationStatus_DisplayAttributeName()
        {
            // Arrange 
            var enumValue = ApplicationStatus.StageOneSubmitted;

            // Act
            var displayName = enumValue.GetDisplayName();

            // Assert
            displayName.Should().Be("Stage One Submitted");
        }

        [Fact]
        public void GetDisplayName_Returns_ApplicationStatus_Default_DisplayAttributeName()
        {
            // Arrange 
            var enumValue = ApplicationStatus.OnHold;

            // Act
            var displayName = enumValue.GetDisplayName();

            // Assert
            displayName.Should().Be("OnHold");
        }

        [Fact]
        public void GetDisplayName_Returns_TaskType_DisplayAttributeName()
        {
            // Arrange 
            var enumValue = TaskType.PlantDetails;

            // Act
            var displayName = enumValue.GetDisplayName();

            // Assert
            displayName.Should().Be("Tell us about your site");
        }


        [Fact]
        public void GetDisplayTag_Returns_TaskStatus_CannotStartYet_Tag()
        {
            // Arrange 
            var enumValue = TaskStatus.CannotStartYet;

            // Act
            var displayName = enumValue.GetDisplayTag();

            // Assert
            displayName.Should().Be("govuk-tag--grey");
        }

        [Fact]
        public void GetDisplayTag_Returns_TaskStatus_NotStart_Tag()
        {
            // Arrange 
            var enumValue = TaskStatus.NotStarted;

            // Act
            var displayName = enumValue.GetDisplayTag();

            // Assert
            displayName.Should().Be("govuk-tag--grey");
        }

        [Fact]
        public void GetDisplayTag_Returns_TaskStatus_InProgress_Tag()
        {
            // Arrange 
            var enumValue = TaskStatus.InProgress;

            // Act
            var displayName = enumValue.GetDisplayTag();

            // Assert
            displayName.Should().Be("govuk-tag--blue");
        }

        [Fact]
        public void GetDisplayTag_Returns_TaskStatus_Approved_Tag()
        {
            // Arrange 
            var enumValue = TaskStatus.Approved;

            // Act
            var displayName = enumValue.GetDisplayTag();

            // Assert
            displayName.Should().Be("govuk-tag--green");
        }

        [Fact]
        public void GetDisplayTag_Returns_TaskStatus_Empty_Tag()
        {
            // Arrange 
            var enumValue = TaskStatus.WithApplicant;

            // Act
            var displayName = enumValue.GetDisplayTag();

            // Assert
            displayName.Should().Be("");
        }

    }
}