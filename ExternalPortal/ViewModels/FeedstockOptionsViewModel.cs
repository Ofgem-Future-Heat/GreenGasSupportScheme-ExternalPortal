using ExternalPortal.ViewModels.Value;
using Microsoft.AspNetCore.Mvc.Rendering;
using Ofgem.Azure.Redis.Data.Contracts;
using System;
using System.Collections.Generic;

namespace ExternalPortal.ViewModels
{
    public class FeedstockOptionsViewModel : OptionsViewModel, IAzureRedisStoreEntity
    {
        public Guid Id { get; set; }

        public string SelfSupplyName { get; set; }

        public FeedstockOptionsViewModel() : base(Enums.TaskType.FeedstockDetails, Enums.TaskPropertyName.FeedstockPlanDocument)
        {
            base.Heading = "Tell us about any plans to secure feedstock";
            base.Description = "Any evidence of plans to ensure continuous supply of feedstock reduces the risk that you cannot generate biomethane.";
        }
        public FeedstockOptionsViewModel(string saveActionName) : this() { base.SaveActionName = saveActionName; }

        public override void SetupOptions()
        {
            base.Options.AddRange(new List<SelectListItem>
                        {
                            new SelectListItem
                            {
                                Value = "agreement",
                                Text = "I have a feedstock agreement in place",
                                Selected = false
                            },
                            new SelectListItem
                            {
                                Value = "self-supply",
                                Text = "I can self supply feedstock",
                                Selected = true
                            },
                            new SelectListItem
                            {
                                Value = "none",
                                Text = "I do not have plans in place yet",
                                Selected = false
                            }
                        });
        }
    }
}
