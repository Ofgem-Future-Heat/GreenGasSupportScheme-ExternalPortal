using GovUkDesignSystem.Attributes;

namespace ExternalPortal.Enums
{
    public enum PlanningPermissionOutcome
    {
        [GovUkRadioCheckboxLabelText(Text = "I was given planning permission")]
        HavePlanningPermission,

        [GovUkRadioCheckboxLabelText(Text = "Planning permission was not required")]
        NotRequired
    }
}
