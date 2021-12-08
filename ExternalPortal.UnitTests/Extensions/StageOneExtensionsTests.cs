using System;
using System.Collections.Generic;
using ExternalPortal.Constants;
using ExternalPortal.Extensions;
using FluentAssertions;
using Moq;
using Ofgem.API.GGSS.Domain.Enums;
using Ofgem.API.GGSS.Domain.Models;
using Ofgem.API.GGSS.Domain.ModelValues;
using Xunit;

namespace ExternalPortal.UnitTests.Extensions
{
    public class StageOneExtensionsTests
    {
        private ApplicationModel _installation1;

        public StageOneExtensionsTests()
        {
            _installation1 = new ApplicationModel
            {
                Id = Guid.NewGuid().ToString(),
                IsActive = true,
                Value = new ApplicationValue
                {
                    Location = Location.Scotland,
                    InstallationName = "CNG EUROCENTRAL GLASGOW",
                    InstallationAddress = new AddressModel { LineOne = "WOODSIDE", Town = "MOTHERWELL", Postcode = "ML1" },
                    MaxCapacity = 786000,
                    DateInjectionStart = DateTime.Now.AddDays(365),
                    Reference = "TEST-REF-001",
                    Status = ApplicationStatus.Draft
                },
                Documents = new List<DocumentModel>
                    {
                        new DocumentModel
                        {
                            Id = Guid.NewGuid().ToString(),
                            Value = new DocumentValue
                            {
                                FileId = "1779fcc5-bdd7-4980-be9e-a852e5ed8904_CapacityCheck.pdf",
                                FileName= "CapacityCheck.pdf",
                                Tags = DocumentTags.CapacityCheck}
                        },
                        new DocumentModel
                        {
                            Id = Guid.NewGuid().ToString(),
                            Value = new DocumentValue
                            {
                                FileId = "5b4837f3-8faf-4e27-877f-d10a0bb455a9_MyPlanningPermission.pdf",
                                FileName= "MyPlanningPermission.pdf",
                                Tags = DocumentTags.PlanningPermission}
                        }
                    }
            };

        }

        [Fact]
        public void GetStageOneOutstandingTasks_ReturnsSuccessful()
        {
            // Arrange 
            var applicationModel = _installation1;

            // Act
            var outstandingTasks = applicationModel.GetStageOneOutstandingTasks();

            // Assert
            outstandingTasks.Should().Be(1);

        }

        [Fact]
        public void GetStageOneOutstandingTasks_WhenValueIsNull_ReturnsArgumentNullException()
        {
            // Arrange 
            var mockApplicationModel = new Mock<ApplicationModel>();

            // Act // Assert
            mockApplicationModel.Object.Invoking(u => u.GetStageOneOutstandingTasks()).Should().Throw<InvalidOperationException>();
        }

    }

}
