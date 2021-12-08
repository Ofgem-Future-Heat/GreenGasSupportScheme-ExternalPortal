using ExternalPortal.ViewModels.Shared.Layouts;
using GovUkDesignSystem.Attributes.ValidationAttributes;

namespace ExternalPortal.ViewModels
{
    public class ResponsiblePersonIndexViewModel : SimplePageHeaderLayoutViewModel
    {
        [GovUkValidateRequired(ErrorMessageIfMissing = "Select yes if you are the authorised signatory for this organisation")]
        public ResponsiblePersonType? ResponsiblePersonType { get; set; }

        public ResponsiblePersonIndexViewModel()
        {
        }
    }
}
