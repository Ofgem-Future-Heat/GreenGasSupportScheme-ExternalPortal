using ExternalPortal.Helpers;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace ExternalPortal.UnitTests.Helpers
{
    public class DateTimeHelperTests
    {
        [Fact]
        public void CanValidateDateOfBirth_ShouldSucceed()
        {
            // Arrange
            var dob = new DateTime(1965, 8, 17);

            // Act
            var result = DateTimeHelper.IsValidateDateOfBirth(dob);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void CanValidateDateOfBirth_ShouldFail()
        {
            // Arrange
            var dob = new DateTime(2007, 3, 28);

            // Act
            var result = DateTimeHelper.IsValidateDateOfBirth(dob);

            // Assert
            result.Should().BeFalse();
        }
    }
}
