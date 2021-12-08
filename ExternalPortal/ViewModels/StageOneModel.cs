// Once there is no use for this it SHOULD be DELETED from the project.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using GovUkDesignSystem.Attributes.ValidationAttributes;

namespace ExternalPortal.ViewModels
{
    public class StageOneModel
    {
        public string PlantLocation { get; set; }
        public string CapacityUploadFileName { get; set; }
        public Address PlantAddress { get; set; }
        public string MaxCapacity { get; set; }
        public string EligibleBiomethane { get; set; }
        public string InjectionStartDay { get; set; }
        public string InjectionStartMonth { get; set; }
        public string InjectionStartYear { get; set; }
        public string PlanningPermissionFileName { get; set; }
        public bool HasPlanningPermission { get; set; }
        public string FeedstockPlan { get; set; }
        public string FeedstockSelfSupplyName { get; set; }
        public string FeedstockUploadFileName { get; set; }
    }

    public class Address
    {
        public string Name { get; set; }
        public string Line1 { get; set; }
        public string Line2 { get; set; }
        public string Town { get; set; }
        public string County { get; set; }
        public string Postcode { get; set; }
    }

}
