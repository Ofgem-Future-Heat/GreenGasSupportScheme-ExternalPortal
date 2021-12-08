using GovUkDesignSystem.Attributes.ValidationAttributes;
using Ofgem.API.GGSS.Domain.Enums;

namespace ExternalPortal.ViewModels.Organisation
{
    public class EnterOrgDetailsViewModel
    {
        [GovUkValidateRequired(ErrorMessageIfMissing = "Enter organisation name")]
        public string Name { get; set; }
        [GovUkValidateRequired(ErrorMessageIfMissing = "Enter building and street")]
        public string LineOne { get; set; }
        public string LineTwo { get; set; }
        [GovUkValidateRequired(ErrorMessageIfMissing = "Enter town or city")]
        public string Town { get; set; }
        public string County { get; set; }
        [GovUkValidateRequired(ErrorMessageIfMissing = "Enter postcode")]
        public string Postcode { get; set; }
        public OrganisationType? Type { get; set; }
        public string ReturnUrl { get; set; }

    }
}