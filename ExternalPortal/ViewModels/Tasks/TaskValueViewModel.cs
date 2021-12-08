using ExternalPortal.Enums;
using ExternalPortal.Extensions;
using ExternalPortal.Helpers;
using System;

namespace ExternalPortal.ViewModels.Tasks
{
    public class TaskValueViewModel
    {
        public TaskPropertyName PropertyName { get; private set; }       
        public string Title => this.PropertyName.GetDisplayName();
        public object Value { get; set; }
        public Type ValueType { get; private set; }

        public TaskValueStatus Status { get; set; }

        public TaskType TaskType { get; private set; }

        public TaskValueViewModel()
        {
            this.Status = TaskValueStatus.NotStarted;
        }

        public TaskValueViewModel(TaskType taskType, TaskPropertyName propertyName, Type valueType) : this()
        {
            this.TaskType = taskType;
            this.PropertyName = propertyName;
            this.ValueType = valueType;
        }
    }
}
