using ExternalPortal.Enums;
using TaskStatus = ExternalPortal.Enums.TaskStatus;

namespace ExternalPortal.ViewModels
{
    public interface IApplicationTaskItemModel
    {
        TaskType Type { get; }
        TaskStatus State { get; set; }
    }
}
