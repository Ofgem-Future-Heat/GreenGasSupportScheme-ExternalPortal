using System.Collections.Generic;
using ExternalPortal.ViewModels.Shared.Components;

namespace ExternalPortal.ViewModels.Shared.Layouts
{
    public abstract class SimplePageHeaderLayoutViewModel : IStandardHeadingsLayoutViewModel
    {
        public IList<BreadcrumbViewModel> Breadcrumbs { get; set; }
        public string BackArea { get; set; }
        public string BackController { get; set; }
        public string BackAction { get; set; }
        public IDictionary<string, string> ReferenceParams { get; set; }
        public PageHeadingViewModel PageHeading { get; set; }
        public string ConfirmationText { get; set; }

        public BackLinkViewModel BackLink
        {
            get
            {
                if (!string.IsNullOrEmpty(BackAction))
                {
                    return BackLinkViewModel.FromAction(BackAction, BackController, BackArea, ReferenceParams);
                }
                return null;
            }
        }
    }
}
