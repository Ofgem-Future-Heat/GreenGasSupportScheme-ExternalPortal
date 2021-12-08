using ExternalPortal.Enums;
using TaskStatus = ExternalPortal.Enums.TaskStatus;

namespace ExternalPortal.ViewModels.Tasks
{
    public class ApplicationTaskItemModel : IApplicationTaskItemModel
    {
        public TaskType Type { get; set; }

        public TaskStatus State { get; set; }
    }
}
