using System;
using ExternalPortal.ViewModels.Value;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace ExternalPortal.Extensions
{
    public static class ValidationExtensions
    {
        public static bool IsInvalidString(this ModelStateDictionary modelState, StringViewModel vm, string errorMessageIfMissing=null)
        {
            if (modelState.GetFieldValidationState(nameof(vm.Value)) == ModelValidationState.Invalid)
            {
                if (String.IsNullOrEmpty(errorMessageIfMissing))
                {
                    return true;
                }
                else
                {
                    modelState.Clear();
                    modelState.AddModelError(nameof(vm.Value), errorMessageIfMissing);

                    return true;
                }
            }

            return false;
        }
    }
}
