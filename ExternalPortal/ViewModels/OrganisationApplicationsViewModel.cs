using System.Collections.Generic;

namespace ExternalPortal.ViewModels
{
    public class OrganisationApplicationsViewModel
    {
        public List<Application> Applications { get; set; }
        public string Name { get; set; }
        public string OrganisationId { get; set; }

        public class Application
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string Status { get; set; }
        }
    }
}