using ExternalPortal.Enums;
using System;
using System.Collections.Generic;

namespace ExternalPortal.ViewModels.Tasks
{
    public class TaskViewModel// : IApplicationTaskItemModel
    {
        public Guid OrganisationId { get; set; }
        public Guid ApplicationId { get; set; }

        /// <summary>
        /// This is the name of the property on the underlaying model
        /// The Value will pe mapped by property name
        /// </summary>
        public TaskType Name { get; }
        public string Description { get; set; }

        public List<TaskValueViewModel> Values { get; set; }

        private TaskViewModel()
        {
            this.Values = new List<TaskValueViewModel>();
        }

        public TaskViewModel(TaskType name) : this()
        {
            this.Name = name;         
        }
    }
}
