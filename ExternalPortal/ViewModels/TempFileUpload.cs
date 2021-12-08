using ExternalPortal.Enums;
using ExternalPortal.ViewModels.Tasks;
using Ofgem.API.GGSS.Domain.Models;
using System;

namespace ExternalPortal.ViewModels
{
    public class TempFileUpload : TaskValueViewModel
    {
        public long FileSize { get; set; }
        public byte[] File { get; set; }
        public DateTime DateAdded => DateTime.UtcNow;

        public TempFileUpload(TaskType taskType, TaskPropertyName propertyName) : base(taskType, propertyName, typeof(DocumentModel))
        {

        }
    }
}
