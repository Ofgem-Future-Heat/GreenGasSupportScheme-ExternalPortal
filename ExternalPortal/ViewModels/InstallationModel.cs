using System;
using ApplicationStatus = Ofgem.API.GGSS.Domain.Enums.ApplicationStatus;
using Ofgem.API.GGSS.Domain.Models;
using Ofgem.Azure.Redis.Data.Contracts;
using Ofgem.API.GGSS.Domain.ModelValues;
using ExternalPortal.ViewModels.Shared.Layouts;

namespace ExternalPortal.ViewModels
{
    public class InstallationModel : SimplePageHeaderLayoutViewModel, IAzureRedisStoreEntity
    {
        public InstallationModel()
        {
            Id = Guid.NewGuid();

            Application = new ApplicationModel
            {
                Id = Id.ToString(),
                Value = new ApplicationValue() { Status = ApplicationStatus.Draft }
            };

            StageOne = new StageOneTaskModel();
            StageTwo = new StageTwoTaskModel();
            StageThree = new StageThreeTaskModel();
        }

        public Guid Id { get; set; }
        public StageOneTaskModel StageOne { get; set; }
        public StageTwoTaskModel StageTwo { get; set; }
        public StageThreeTaskModel StageThree { get; set; }
        public ApplicationModel Application { get; set; }
        public Guid OrganisationId { get; set; }
        public Guid UserId { get; set; }
    }
}
