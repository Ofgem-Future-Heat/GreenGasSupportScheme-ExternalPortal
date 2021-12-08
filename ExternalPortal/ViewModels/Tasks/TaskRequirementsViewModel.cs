using ExternalPortal.Enums;
using ExternalPortal.Extensions;
using System;
using System.Collections.Generic;

namespace ExternalPortal.ViewModels.Tasks
{
    public class TaskRequirementsViewModel
    {
        public Guid OrganisationId { get; set; }
        public Guid ApplicationId { get; set; }
        public TaskType TaskType { get; set; }
        public string ProceedActionName { get; set; }
        public string ControllerName { get; set; }
        public string BackLink { get; set; }
        public List<string> Requirements { get; set; }

        public TaskRequirementsViewModel()
        {
            this.Requirements = new List<string>();
        }

        public TaskRequirementsViewModel(TaskType taskType) : this()
        {
            this.TaskType = taskType;
        }
    }
}
