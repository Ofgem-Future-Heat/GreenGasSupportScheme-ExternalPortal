using ExternalPortal.ViewModels.Shared.Components;
using System.Collections.Generic;

namespace ExternalPortal.ViewModels.Shared.Layouts
{
    public interface IStandardHeadingsLayoutViewModel
    {
        public IList<BreadcrumbViewModel> Breadcrumbs { get; }
        public BackLinkViewModel BackLink { get; }
        public string ConfirmationText { get; }
        public PageHeadingViewModel PageHeading { get; }
    }
}
