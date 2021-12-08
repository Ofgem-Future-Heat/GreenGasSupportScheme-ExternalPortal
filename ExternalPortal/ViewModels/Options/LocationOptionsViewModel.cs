using ExternalPortal.Enums;
using ExternalPortal.Extensions;
using ExternalPortal.ViewModels.Value;
using Microsoft.AspNetCore.Mvc.Rendering;
using Ofgem.API.GGSS.Domain.Enums;
using Ofgem.Azure.Redis.Data.Contracts;
using System;

namespace ExternalPortal.ViewModels.Options
{
    public class LocationOptionsViewModel : OptionsViewModel, IAzureRedisStoreEntity
    {
        public Guid Id { get; set; }
        public LocationOptionsViewModel() : base(TaskType.PlantDetails, TaskPropertyName.Location)
        {
            Heading = "Select lovation";
        }

        public override void SetupOptions()
        {
            foreach (Location loc in Enum.GetValues(typeof(Location)))
            {
                base.Options.Add(new SelectListItem
                {
                    Value = loc.ToString(),
                    Text = loc.GetDisplayName()
                });
            }
        }
    }
}
