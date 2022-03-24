using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.RegularExpressions;
using ExternalPortal.Helpers;

namespace GGSS.WebCommon.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ValidateLatitudeAttribute : ValidationAttribute
    {
        public ValidateLatitudeAttribute() : base("Enter latitude value between -90 and 90")
        {
        }

        public override bool IsValid(object value)
        {
            return !(Convert.ToDouble(value) < -90.0 || Convert.ToDouble(value) > 90.0);
        }
    }
}