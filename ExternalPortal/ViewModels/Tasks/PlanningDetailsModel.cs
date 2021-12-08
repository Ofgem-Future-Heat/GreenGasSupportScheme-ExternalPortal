using GovUkDesignSystem.Attributes.ValidationAttributes;
using ExternalPortal.Enums;
using TaskStatus = ExternalPortal.Enums.TaskStatus;
using ExternalPortal.ViewModels.Shared.Layouts;
using Ofgem.API.GGSS.Domain.ModelValues;
using ExternalPortal.ViewModels.Shared.Components;



namespace ExternalPortal.ViewModels
{
    public class PlanningDetailsModel : SimplePageHeaderLayoutViewModel, IApplicationTaskItemModel
    {
        public PlanningDetailsModel()
        {
            PageHeading = new PageHeadingViewModel("Do you have planning permission?")
            {
                SubHeading = "You must have planning permission from the local authority responsible for covering the location where you plan to build the biomethane installation."
            };
            BackController = null;
            BackAction = "apply";
            BackArea = null;

            PlanningPermissionDocument = new DocumentValue();
            State = TaskStatus.NotStarted;
        }

        public TaskType Type => TaskType.PlanningPermission;

        public TaskStatus State { get; set; }

        [GovUkValidateRequired(ErrorMessageIfMissing = "Select planning permission outcome")]
        public PlanningPermissionOutcome? PlanningPermissionOutcome { get; set; }

        public DocumentValue PlanningPermissionDocument { get; set; }
        public string PlanningPermissionDocumentFileSize { get; set; }
    }
}