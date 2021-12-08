using System.Collections.Generic;

namespace ExternalPortal.ViewModels
{
    public class DashboardViewModel
    {

        public string UserDisplayName { get; set; }
        public List<OrganisationItemViewModel> Organisations { get; set; }
    }
}
