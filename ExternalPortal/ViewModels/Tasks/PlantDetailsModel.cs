using GovUkDesignSystem.Attributes.ValidationAttributes;
using Ofgem.API.GGSS.DomainModels;

using ExternalPortal.Enums;
using TaskStatus = ExternalPortal.Enums.TaskStatus;
using Ofgem.API.GGSS.Domain.ModelValues;
using ExternalPortal.ViewModels.Shared.Layouts;
using ExternalPortal.ViewModels.Shared;
using GGSS.WebCommon.Attributes;

namespace ExternalPortal.ViewModels
{
    public class PlantDetailsModel : SimplePageHeaderLayoutViewModel, IApplicationTaskItemModel
    {
        public PlantDetailsModel()
        {
            InstallationAddress = new AddressViewModel();
            CapacityCheckDocument = new DocumentValue();
            LatitudeLongitudeAnaerobic = new LatitudeLongitudeValue();
            LatitudeLongitudeInjection = new LatitudeLongitudeValue();
            State = TaskStatus.NotStarted;
        }

        public TaskType Type => TaskType.PlantDetails;

        public TaskStatus State { get; set; }

        [GovUkValidateRequired(ErrorMessageIfMissing = "Select plant location")]
        public PlantLocation? Location { get; set; }

        public DocumentValue CapacityCheckDocument { get; set; }

        [GovUkValidateRequired(ErrorMessageIfMissing = "Enter plant name")]
        [ValidateCharacters]
        public string InstallationName { get; set; }
        
        [GovUkValidateRequired(ErrorMessageIfMissing = "Enter equipment description")]
        [ValidateCharacters]
        public string EquipmentDescription { get; set; }

        public AddressViewModel InstallationAddress { get; set; }

        public AddressViewModel InjectionPointAddress { get; set; }
        
        public LatitudeLongitudeValue LatitudeLongitudeAnaerobic { get; set; }
        
        public LatitudeLongitudeValue LatitudeLongitudeInjection { get; set; }
        
        public string HasPostcode { get; set; }

        public string Error { get; set; }

        public bool HasError()
        {
            return !string.IsNullOrEmpty(Error);
        }
        
        public string ReturnUrl { get; set; }
    }
}
