using System.Text.RegularExpressions;

namespace ExternalPortal.Helpers
{
    public static class PhoneNumberHelper
    {
        public static bool IsValidatePhoneNumber(string phoneNumber)
        {
            var regex = new Regex("^0([1-6][0-9]{8,9}|7[0-9]{9})$");
            return regex.IsMatch(phoneNumber);
        }
    }
}