using System;

namespace ExternalPortal.ViewModels
{
    public class OrganisationItemViewModel
    {
        public Guid Id { get; set; }
        
        public string Name { get; set; }

        public int NumberOfInstallations { get; set; }

        public int TasksOutstanding { get; set; }
    }
}
