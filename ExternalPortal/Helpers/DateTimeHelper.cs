using System;

namespace ExternalPortal.Helpers 
{
    public static class DateTimeHelper
    {
        const int minimumDateOfBirthAge = 18;

        public static bool IsValidateDateOfBirth(DateTime dateOfBirth)
        {
            return dateOfBirth < DateTime.Now.AddYears(-minimumDateOfBirthAge);
        }

        public static bool IsValidYear(DateTime date)
        {
            return date.Year.ToString().Length == 4;
        }
    }
}