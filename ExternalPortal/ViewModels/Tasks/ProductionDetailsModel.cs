using GovUkDesignSystem.Attributes.ValidationAttributes;
using System;
using Microsoft.AspNetCore.Mvc;
using GovUkDesignSystem.Attributes.DataBinding;
using GovUkDesignSystem.ModelBinders;
using ExternalPortal.Enums;

namespace ExternalPortal.ViewModels
{
    public class ProductionDetailsModel : IApplicationTaskItemModel
    {
        public ProductionDetailsModel()
        {
            State = TaskStatus.NotStarted;
        }

        public TaskType Type => TaskType.ProductionDetails;

        public TaskStatus State { get; set; }

        [GovUkValidateRequired(ErrorMessageIfMissing = "Enter maximum initial capacity")]
        public string MaxCapacity { get; set; }

        public DateTime DateInjectionStart { get; set; }

        [ModelBinder(typeof(GovUkOptionalIntBinder))]
        [GovUkDataBindingOptionalIntErrorText("Enter injection start day")]
        public int? InjectionStartDay { get; set; }

        [ModelBinder(typeof(GovUkOptionalIntBinder))]
        [GovUkDataBindingOptionalIntErrorText("Enter injection start month")]
        public int? InjectionStartMonth { get; set; }

        [ModelBinder(typeof(GovUkOptionalIntBinder))]
        [GovUkDataBindingOptionalIntErrorText("Enter injection start year")]
        public int? InjectionStartYear { get; set; }
    }
}