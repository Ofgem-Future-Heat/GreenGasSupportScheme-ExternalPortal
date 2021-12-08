using System;
using System.Threading.Tasks;
using ExternalPortal.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace ExternalPortal.UnitTests.Models
{
    public class EmailParameterBuilderTests
    {
        [Fact]
        public void ShouldReturnExceptionWhenTemplateIdIsMissing()
        {
            Assert.Throws<ArgumentNullException>(() => new EmailParameterBuilder(null, "email-address"));
        }

        [Fact]
        public void ShouldReturnExceptionWhenEmailAddressIsMissing()
        {
            Assert.Throws<ArgumentNullException>(() => new EmailParameterBuilder("template-id", null));
        }

        [Fact]
        public void ShouldReturnTemplateIdFromEmailParameter()
        {
            var emailParameter = new EmailParameterBuilder("template-id", "email-address").Build();

            Assert.Equal("template-id", emailParameter.TemplateId);
        }

        [Fact]
        public void ShouldReturnEmailAddressFromEmailParameter()
        {
            var emailParameter = new EmailParameterBuilder("template-id", "email-address").Build();

            Assert.Equal("email-address", emailParameter.EmailAddress);
        }

        [Fact]
        public void ShouldReturnNotificationReferenceFromEmailParameter()
        {
            var emailParameter = new EmailParameterBuilder("template-id", "email-address").Build();

            var reference = Guid.Parse(emailParameter.Reference);

            Assert.IsType<Guid>(reference);
        }

        [Fact]
        public void ShouldThrowExceptionWhenTemplateIdIsAdded()
        {
            Assert.Throws<ArgumentException>(() => new EmailParameterBuilder("template-id", "email-address")
                .AddCustom("TEMPLATEID", "template-id-again")
                .Build());
        }

        [Fact]
        public void ShouldThrowExceptionWhenEmailAddressIsAdded()
        {
            Assert.Throws<ArgumentException>(() => new EmailParameterBuilder("template-id", "email-address")
                .AddCustom("emailaddress", "email-address-again")
                .Build());
        }

        [Fact]
        public void ShouldThrowExceptionWhenTwoIdenticalCustomIsAdded()
        {
            Assert.Throws<ArgumentException>(() => new EmailParameterBuilder("template-id", "email-address")
                .AddCustom("custom-one", "one")
                .AddCustom("custom-one", "two")
                .Build());
        }

        [Fact]
        public void ShouldThrowExceptionWhenTwoIdenticalCustomIsAddedWithOneInBetween()
        {
            Assert.Throws<ArgumentException>(() => new EmailParameterBuilder("template-id", "email-address")
                .AddCustom("custom-one", "one")
                .AddCustom("custom-abc", "two")
                .AddCustom("custom-one", "three")
                .Build());
        }

        [Fact]
        public async Task ShouldReturnTemplateIdFromEmailParameterContent()
        {
            var emailParameter = new EmailParameterBuilder("template-id", "email-address")
                .Build();

            var result = await emailParameter.Content.ReadAsStringAsync();

            var matchIndex = result.IndexOf("TemplateId=template-id");

            matchIndex.Should().BeGreaterThan(-1);
        }

        [Fact]
        public async Task ShouldReturnReferenceFromEmailParameterContent()
        {
            var emailParameter = new EmailParameterBuilder("template-id", "email-address")
                .Build();

            var result = await emailParameter.Content.ReadAsStringAsync();

            var matchIndex = result.IndexOf("Reference=");

            matchIndex.Should().BeGreaterThan(-1);
        }

        [Fact]
        public async Task ShouldReturnEmailAddressFromEmailParameterContent()
        {
            var emailParameter = new EmailParameterBuilder("template-id", "email-address")
                .Build();

            var result = await emailParameter.Content.ReadAsStringAsync();

            var matchIndex = result.IndexOf("EmailAddress=email-address");

            matchIndex.Should().BeGreaterThan(-1);
        }

        [Fact]
        public async Task ShouldReturnFirstNameFromEmailParameter()
        {
            var emailParameter = new EmailParameterBuilder("template-id", "email-address")
                .AddFirstName("first-name")
                .Build();

            var result = await emailParameter.Content.ReadAsStringAsync();

            var matchIndex = result.IndexOf("FirstName=first-name");

            matchIndex.Should().BeGreaterThan(-1);
        }

        [Fact]
        public async Task ShouldReturnLastNameFromEmailParameter()
        {
            var emailParameter = new EmailParameterBuilder("template-id", "email-address")
                .AddLastName("last-name")
                .Build();

            var result = await emailParameter.Content.ReadAsStringAsync();

            var matchIndex = result.IndexOf("LastName=last-name");

            matchIndex.Should().BeGreaterThan(-1);
        }

        [Fact]
        public async Task ShouldReturnFullNameFromEmailParameter()
        {
            var emailParameter = new EmailParameterBuilder("template-id", "email-address")
                .AddFullName("Bob The Builder")
                .Build();

            var result = await emailParameter.Content.ReadAsStringAsync();

            var matchIndex = result.IndexOf("FullName=Bob+The+Builder");

            matchIndex.Should().BeGreaterThan(-1);
        }

        [Fact]
        public async Task ShouldReturnApplicationIdFromEmailParameter()
        {
            var applicationId = new Guid("881c35c2-9712-4843-a92d-c63152b6950a");

            var emailParameter = new EmailParameterBuilder("template-id", "email-address")
                .AddApplicationId(applicationId)
                .Build();

            var result = await emailParameter.Content.ReadAsStringAsync();

            var matchIndex = result.IndexOf("ApplicationId=GGSS-881C3");

            matchIndex.Should().BeGreaterThan(-1);
        }

        [Fact]
        public async Task ShouldReturnCustomKeyValueFromEmailParameter()
        {
            var emailParameter = new EmailParameterBuilder("template-id", "email-address")
                .AddCustom("the-key","the-value")
                .Build();

            var result = await emailParameter.Content.ReadAsStringAsync();

            var matchIndex = result.IndexOf("the-key=the-value");

            matchIndex.Should().BeGreaterThan(-1);
        }

        [Fact]
        public async Task ShouldReturnFirstNameAndApplicationIdFromEmailParameter()
        {
            var applicationId = new Guid("881c35c2-9712-4843-a92d-c63152b6950a");

            var emailParameter = new EmailParameterBuilder("template-id", "email-address")
                .AddFirstName("the-first-name")
                .AddApplicationId(applicationId)
                .Build();

            var result = await emailParameter.Content.ReadAsStringAsync();

            var matchIndex = result.IndexOf("FirstName=the-first-name&ApplicationId=GGSS-881C3");

            matchIndex.Should().BeGreaterThan(-1);
        }

        [Fact]
        public async Task ShouldReturnApplicationIdAndFirstNameFromEmailParameter()
        {
            var applicationId = new Guid("881c35c2-9712-4843-a92d-c63152b6950a");

            var emailParameter = new EmailParameterBuilder("template-id", "email-address")
                .AddApplicationId(applicationId)
                .AddFirstName("the-first-name")
                .Build();

            var result = await emailParameter.Content.ReadAsStringAsync();

            var matchIndex = result.IndexOf("ApplicationId=GGSS-881C3&FirstName=the-first-name");

            matchIndex.Should().BeGreaterThan(-1);
        }

        [Fact]
        public async Task ShouldReturnFormattedDateFromEmailParameter()
        {
            var date = new DateTime(2020, 12, 12);

            var emailParameter = new EmailParameterBuilder("template-id", "email-address")
                .AddDate("custom-date", date)
                .Build();

            var result = await emailParameter.Content.ReadAsStringAsync();

            var matchIndex = result.IndexOf("custom-date=12+December+2020");

            matchIndex.Should().BeGreaterThan(-1);
        }

        [Fact]
        public async Task ShouldReturnLinkToDashboardFromEmailParameter()
        {
            var applicationId = new Guid("f95bcc50-4ec7-4795-88f5-3e1d464f8e03");

            var httpContext = new DefaultHttpContext();

            httpContext.Request.Scheme = "my-scheme";
            httpContext.Request.Host = new HostString("my-host");

            var emailParameter = new EmailParameterBuilder("template-id", "email-address")
                .AddDashboardLink(httpContext.Request.Scheme, httpContext.Request.Host, applicationId)
                .Build();

            var result = await emailParameter.Content.ReadAsStringAsync();

            var matchIndex = result.IndexOf("DashboardLink=my-scheme%3A%2F%2Fmy-host%2Ftask-list%2Ff95bcc50-4ec7-4795-88f5-3e1d464f8e03");

            matchIndex.Should().BeGreaterThan(-1);
        }
    }
}
