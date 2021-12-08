using GovUkDesignSystem.Attributes;

namespace Ofgem.API.GGSS.DomainModels
{
    public enum FeedstockPlan
    {
        [GovUkRadioCheckboxLabelText(Text = "I have a feedstock agreement in place")]
        Yes,

        [GovUkRadioCheckboxLabelText(Text = "I can self supply feedstock")]
        Self,

        [GovUkRadioCheckboxLabelText(Text = "I do not have plans in place yet")]
        No
    }
}