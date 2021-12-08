using ExternalPortal.Enums;
using System;

namespace ExternalPortal.ViewModels.Value
{
    public class ValueViewModel
    {
        public TaskType TaskType { get; set; }
        public TaskPropertyName PropertyName { get; set; }
        public Type PropertyType { get; set; }
        public string Regex { get; set; }
        public string BackAction { get; set; }
        public string SaveActionName { get; set; }
        public ValueViewModel() { }
        public ValueViewModel(TaskType taskType, TaskPropertyName propertyName, Type propertyType)
        {
            this.TaskType = taskType;
            this.PropertyName = propertyName;
            this.PropertyType = propertyType;
        }
        public string ReturnUrl { get; set; }
    }
}
