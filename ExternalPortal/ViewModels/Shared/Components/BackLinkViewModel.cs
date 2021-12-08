using System.Collections.Generic;

namespace ExternalPortal.ViewModels.Shared.Components
{
    public class BackLinkViewModel
    {
        public string Href { get; }
        public string Area { get; }
        public string Controller { get; }
        public string Action { get; }
        public IDictionary<string, string> ReferenceParams { get; set; }

        private BackLinkViewModel(
            string href,
            string action,
            string controller,
            string area,
            IDictionary<string, string> referenceParams)
        {
            Href = href;
            Action = action;
            Controller = controller;
            Area = area;
            ReferenceParams = referenceParams;
        }

        public static BackLinkViewModel FromAction(
            string action,
            string controller = null,
            string area = null,
            IDictionary<string, string> referenceParams = null)
        {
            return new BackLinkViewModel(
                null,
                action,
                controller,
                area,
                referenceParams);
        }

        public static BackLinkViewModel FromUrl(
            string url)
        {
            return new BackLinkViewModel(
                url,
                null,
                null,
                null,
                null);
        }
    }

    
}
