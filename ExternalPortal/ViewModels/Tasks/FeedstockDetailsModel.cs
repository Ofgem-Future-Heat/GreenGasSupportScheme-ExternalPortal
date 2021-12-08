using GovUkDesignSystem.Attributes.ValidationAttributes;
using Ofgem.API.GGSS.DomainModels;
using ExternalPortal.Enums;

namespace ExternalPortal.ViewModels
{
    public class FeedstockDetailsModel : IApplicationTaskItemModel
    {
        public FeedstockDetailsModel()
        {
            State = TaskStatus.NotStarted;
        }

        public TaskType Type => TaskType.FeedstockDetails;

        public TaskStatus State { get; set; }

        [GovUkValidateRequired(ErrorMessageIfMissing = "Select feedstock plan")]
        public FeedstockPlan FeedstockPlan { get; set; }

        [GovUkValidateRequired(ErrorMessageIfMissing = "Enter feedstock supplier name")]
        public string FeedstockSelfSupplyName { get; set; }


        public string FeedstockUploadFileName { get; set; }
    }
}