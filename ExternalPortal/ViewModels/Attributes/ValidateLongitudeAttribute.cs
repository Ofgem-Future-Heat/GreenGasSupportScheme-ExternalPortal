using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.RegularExpressions;
using ExternalPortal.Helpers;

namespace GGSS.WebCommon.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ValidateLongitudeAttribute : ValidationAttribute
    {
        public ValidateLongitudeAttribute() : base("Enter longitude value between -180 and 180")
        {
        }

        public override bool IsValid(object value)
        {
            return !(Convert.ToDouble(value) < -180.0 || Convert.ToDouble(value) > 180.0);
        }
    }
}