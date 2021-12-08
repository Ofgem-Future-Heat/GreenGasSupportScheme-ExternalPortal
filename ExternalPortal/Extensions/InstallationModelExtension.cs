using ExternalPortal.Constants;
using ExternalPortal.ViewModels;
using System.Collections.Generic;

namespace ExternalPortal.Extensions
{
    public static class InstallationModelExtension
    {
        public static string GetBackActionLink(this InstallationModel value)
        {
            return $"/task-list";
        }
        public static Dictionary<string, string> GetReturnToYourApplicationLink(this InstallationModel value)
        {
            var returnToYourApplicationURL = GetBackActionLink(value);
            var referenceParams = new Dictionary<string, string>
            {
                { UrlKeys.ReturnToYourApplicationLink, returnToYourApplicationURL }
            };
            return referenceParams;

        }
    }
}
