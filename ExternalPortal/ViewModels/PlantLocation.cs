using GovUkDesignSystem.Attributes;

namespace Ofgem.API.GGSS.DomainModels
{
    public enum PlantLocation
    {
        [GovUkRadioCheckboxLabelText(Text = "England")]
        England,

        [GovUkRadioCheckboxLabelText(Text = "Scotland")]
        Scotland,

        [GovUkRadioCheckboxLabelText(Text = "Wales")]
        Wales
    }
}
