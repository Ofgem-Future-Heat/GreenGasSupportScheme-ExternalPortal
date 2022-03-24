using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ExternalPortal.ViewModels.Shared.Layouts;
using GGSS.WebCommon.Attributes;
using GovUkDesignSystem.Attributes.ValidationAttributes;
using Ofgem.API.GGSS.Domain.ModelValues;

namespace ExternalPortal.ViewModels
{
    public class InvitationsViewModel : SimplePageHeaderLayoutViewModel, IValidatableObject
    {
        public string OrganisationName { get; set; }
        public string OrganisationId { get; set; }

        [GovUkValidateRequired(ErrorMessageIfMissing = "Enter email address")]
        [ValidateEmail(ErrorMessage = "Please enter a valid email address in the correct format, like name@example.com")]
        public string UserEmail { get; set; }

        public bool IsAuthorisedSignatory { get; set; }

        public List<UserValue> OrganisationUsers { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (OrganisationUsers != null && OrganisationUsers.Find(user => user.EmailAddress.Equals(UserEmail, StringComparison.OrdinalIgnoreCase)) != null)
            {
                yield return new ValidationResult(
                    $"A user with this email address has already been added to the organisation", new [] { nameof(UserEmail) });
            }
        }
    }
}