using System;
using GovUkDesignSystem.Attributes.DataBinding;
using GovUkDesignSystem.Attributes.ValidationAttributes;
using GovUkDesignSystem.ModelBinders;
using Microsoft.AspNetCore.Mvc;

namespace ExternalPortal.ViewModels.ProductionDetails
{
    public class ProductionDetailsStartDateViewModel
    {
        [ModelBinder(typeof(GovUkMandatoryDateBinder))]
        [GovUkValidateRequired(ErrorMessageIfMissing = "Enter date of injection")]
        [GovUkDataBindingDateErrorText("Date of injection", "date of injection")]
        public DateTime? StartDate { get; set; }
    }
}