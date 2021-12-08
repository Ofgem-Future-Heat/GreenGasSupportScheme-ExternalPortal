using ExternalPortal.ViewModels.Shared.Components;
using ExternalPortal.ViewModels.Shared.Layouts;
using System.Collections.Generic;

namespace ExternalWebApp.ViewModels.Shared.Layouts
{
    public class StandardHeadingsLayoutViewModel : IStandardHeadingsLayoutViewModel
    {
        public IList<BreadcrumbViewModel> Breadcrumbs { get; set; }
        public BackLinkViewModel BackLink { get; set; }
        public string ConfirmationText { get; set; }
        public PageHeadingViewModel PageHeading { get; set; }
    }
}
