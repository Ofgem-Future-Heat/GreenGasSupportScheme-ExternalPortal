using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.RegularExpressions;
using ExternalPortal.Helpers;

namespace GGSS.WebCommon.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ValidateCharactersAttribute : ValidationAttribute
    {
        public ValidateCharactersAttribute() : base("Your answer must not include < > \" ' % ( ) & + \\ # * or ;")
        {
        }

        public override bool IsValid(object value)
        {
            if (value == null)
            {
                return true;
            }

            var regex = new Regex("[;*#<>/\"\\\\/-/\'%()&+]");
            return !regex.IsMatch(value.ToString());
        }
    }
}
