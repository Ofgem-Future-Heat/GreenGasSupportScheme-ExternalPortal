using System.ComponentModel.DataAnnotations;

namespace ExternalPortal.Enums
{
    public enum TaskPropertyName
    {
        [Display(Name = "What is the name of the installation ?")]
        InstallationName,
        [Display(Name = "Enter the maximum initial capacity of biomethane you plan to inject per year")]
        MaxCapacity,
        [Display(Name = "Enter the volume of eligible biomethane you expect to inject per year")]
        EligibleBiomethane,
        [Display(Name = "When do you expect to start injecting biomethane?")]
        DateInjectionStart,
        [Display(Name = "Tell us about your installation")]
        Location,
        [Display(Name = "What is the address of the installation ?")]
        InstallationAddress,
        [Display(Name = "Upload your gas network capacity check document")]
        CapacityCheckDocument,
        [Display(Name = "Upload your planning permission documents")]
        PlanningPermissionDocument,
        [Display(Name = "Upload your feedstock supply documents")]
        FeedstockPlanDocument,
        FeedstockSupplierName,
        FeedstockPlan,
        MaximumInitialCapacityOfBiomethane
    }

    public enum TaskPropertyOptions
    {
        [Display(Name = "Tell us about any plans to secure feedstock")]
        FeedstockPlanDocument
    }
}
