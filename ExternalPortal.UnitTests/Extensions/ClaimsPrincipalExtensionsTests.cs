using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using ExternalPortal.Constants;
using ExternalPortal.Extensions;
using FluentAssertions;
using Moq;
using Xunit;

namespace ExternalPortal.UnitTests.Extensions
{
    public class ClaimsPrincipalExtensionsTests
    {
        const string UserId = "326fa974-7c05-4b37-a8e4-6d5fe6deb63b";

        private readonly Mock<ClaimsPrincipal> _cpMock;

        public ClaimsPrincipalExtensionsTests()
        {
            _cpMock = new Mock<ClaimsPrincipal>();

            _cpMock.Setup(u => u.Identity).Returns(new ClaimsIdentity());

            _cpMock.Setup(u => u.Identity.IsAuthenticated).Returns(true);

            _cpMock.SetupGet(u => u.Claims)
                .Returns(new List<Claim> {
                    new Claim(ClaimTypes.NameIdentifier, UserId),
                    new Claim(ClaimTypes.GivenName, "James"),
                    new Claim(ClaimTypes.Surname, "Anderson"),
                    new Claim(CustomClaimTypes.EmailAddress, "james.anderson@test.com"),
                    new Claim(CustomClaimTypes.TelephoneNumber, "01412265098"),
                }.AsEnumerable());
        }

        [Fact]
        public void GetUserId_Returns_NameIdentifier_Claim()
        {
            // Act
            var userIdValue = _cpMock.Object.GetUserId();

            // Assert
            userIdValue.Should().Be(UserId);
        }

        [Fact]
        public void GetUserFullName_Returns_Name_Claim()
        {   
            // Act
            var userNameValue = _cpMock.Object.GetDisplayName();

            // Assert
            userNameValue.Should().Be("James Anderson");
        }

        [Theory]
        [InlineData(ClaimTypes.GivenName, "James")]
        [InlineData(ClaimTypes.Surname, "Anderson")]
        [InlineData(CustomClaimTypes.EmailAddress, "james.anderson@test.com")]
        [InlineData(CustomClaimTypes.TelephoneNumber, "01412265098")]
        public void GetClaim_Returns_ClaimValue(string claimType, string claimValue)
        {
            // Act
            var result = _cpMock.Object.GetClaim(claimType);

            // Assert
            result.Should().Be(claimValue);
        }

    }
}
