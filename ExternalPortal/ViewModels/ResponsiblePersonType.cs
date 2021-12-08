using GovUkDesignSystem.Attributes;

namespace ExternalPortal.ViewModels
{
    public enum ResponsiblePersonType
    {
        [GovUkRadioCheckboxLabelText(Text = "Yes")]
        You,

        [GovUkRadioCheckboxLabelText(Text = "No")]
        No
    }
}
