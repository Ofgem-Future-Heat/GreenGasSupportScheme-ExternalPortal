using GovUkDesignSystem.Attributes.ValidationAttributes;
using GGSS.WebCommon.Attributes;
using System;
using Microsoft.AspNetCore.Mvc;
using GovUkDesignSystem.Attributes.DataBinding;
using GovUkDesignSystem.ModelBinders;
using ExternalPortal.ViewModels.Shared.Layouts;

namespace ExternalPortal.ViewModels
{
    public class ResponsiblePersonDetailViewModel : SimplePageHeaderLayoutViewModel
    {
        [GovUkValidateRequired(ErrorMessageIfMissing = "Enter authorised signatory first name")]
        public string FirstName { get; set; }

        [GovUkValidateRequired(ErrorMessageIfMissing = "Enter authorised signatory surname")]
        public string Surname { get; set; }

        [GovUkValidateRequired(ErrorMessageIfMissing = "Enter authorised signatory email address")]
        [ValidateEmail(ErrorMessage = "Please enter a valid email address in the correct format, like name@example.com")]
        public string EmailAddress { get; set; }
        
        
        [GovUkValidateRequired(ErrorMessageIfMissing = "Enter a phone number")]
        public string PhoneNumber { get; set; }

        public bool ResponsiblePersonIsYou { get; set; }

        [ModelBinder(typeof(GovUkMandatoryDateBinder))]
        [GovUkValidateRequired(ErrorMessageIfMissing = "Enter your date of birth")]
        [GovUkDataBindingDateErrorText("Date of birth", "date of birth")]
        public DateTime? DateOfBirth { get; set; }
        public string ReturnUrl { get; set; }
    }
}
