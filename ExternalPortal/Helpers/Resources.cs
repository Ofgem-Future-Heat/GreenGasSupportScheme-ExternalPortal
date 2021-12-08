using ExternalPortal.Enums;
using System.Collections.Generic;

namespace ExternalPortal.Helpers
{
    public static class Resources
    {
        public static Dictionary<TaskPropertyName, string> Labels => new Dictionary<TaskPropertyName, string>
        {
            { TaskPropertyName.MaxCapacity, "" },
            { TaskPropertyName.EligibleBiomethane, "Add volume of biomethane to be injected per year" },
            { TaskPropertyName.InstallationName, "Name of the installation" },
            { TaskPropertyName.PlanningPermissionDocument, "You can upload your documents as scanned copies or photos of the originals" },
            { TaskPropertyName.FeedstockPlanDocument, "Feedstock plan document" },
        };

        public static Dictionary<TaskPropertyName, string> Descriptions => new Dictionary<TaskPropertyName, string>
        {
            { TaskPropertyName.MaxCapacity, "Enter the volume in cubic metres" },
            { TaskPropertyName.EligibleBiomethane, "Enter the volume of eligible biomethane that you can inject per year based on your connection agreement. This figure should be the eligible portion of your Maximum Initial Capacity. This volume must be expressed in cubic metres." },
            { TaskPropertyName.DateInjectionStart, "" },
            { TaskPropertyName.FeedstockPlanDocument, "You can upload your documents as scanned copies or photos of the originals" },
        };
    }
}
