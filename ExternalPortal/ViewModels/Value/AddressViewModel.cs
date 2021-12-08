using Ofgem.API.GGSS.Domain.Models;

namespace ExternalPortal.ViewModels.Value
{
    public class AddressViewModel : ValueViewModel
    {
        public AddressModel Value { get; set; }
        public AddressViewModel() : base(Enums.TaskType.PlantDetails, Enums.TaskPropertyName.InstallationAddress, typeof(AddressModel))
        {

        }
    }
}
