using Ofgem.API.GGSS.Domain.Enums;
using Ofgem.API.GGSS.Domain.ModelValues;
using ExternalPortal.Enums;
using Ofgem.API.GGSS.DomainModels;

namespace ExternalPortal.ViewModels
{
    public class NetworkEntryAgreementModel : IApplicationTaskItemModel
    {
        public TaskType Type => TaskType.NetworkEntryAgreement;

        public TaskStatus State { get; set; }

        public DocumentValue NetworkEntryAgreementFile { get; set; }
    }
}