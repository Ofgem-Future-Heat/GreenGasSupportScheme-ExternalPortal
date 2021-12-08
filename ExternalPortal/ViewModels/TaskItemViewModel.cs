using ExternalPortal.Enums;
using ExternalPortal.Extensions;

namespace ExternalPortal.ViewModels
{
    public class TaskItemViewModel
    {
        public string DisplayName => this.TaskType.GetDisplayName();
        public TaskType TaskType { get; set; }
        public TaskStatus Status { get; set; }
        public ApplicationStage Stage { get; set; }
        public string StartActionName { get; set; }
        public string ControllerName { get; set; }

        public TaskItemViewModel(TaskType taskType)
        {
            this.TaskType = taskType;
            this.ControllerName = taskType.ToString();
        }
    }
}
