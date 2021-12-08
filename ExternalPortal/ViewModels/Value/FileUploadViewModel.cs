using ExternalPortal.Enums;
using Ofgem.API.GGSS.Domain.Models;

namespace ExternalPortal.ViewModels.Value
{
    public class FileUploadViewModel : ValueViewModel
    {
        public FileUploadViewModel(TaskType taskType, TaskPropertyName propertyName) 
            : base(taskType, propertyName, typeof(DocumentModel))
        {}
    }
}
