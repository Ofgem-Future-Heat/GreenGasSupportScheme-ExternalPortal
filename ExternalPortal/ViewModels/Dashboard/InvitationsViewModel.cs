using ExternalPortal.ViewModels.Shared.Layouts;
using GGSS.WebCommon.Attributes;
using GovUkDesignSystem.Attributes.ValidationAttributes;

namespace ExternalPortal.ViewModels
{
    public class InvitationsViewModel : SimplePageHeaderLayoutViewModel
    {
        public string OrganisationName { get; set; }

        [GovUkValidateRequired(ErrorMessageIfMissing = "Enter email address")]
        [ValidateEmail(ErrorMessage = "Please enter a valid email address in the correct format, like name@example.com")]
        public string UserEmail { get; set; }
    }
}