using Ofgem.API.GGSS.Domain.Enums;
using Ofgem.API.GGSS.Domain.ModelValues;
using ExternalPortal.Enums;
using Ofgem.API.GGSS.DomainModels;
using System.Collections.Generic;

namespace ExternalPortal.ViewModels
{
    public class BiogasProductionPlantModel : IApplicationTaskItemModel
    {
        public TaskType Type => TaskType.BiogasProductionPlant;

        public TaskStatus State { get; set; }

        public IList<DocumentValue> Files { get; set; }

    }
}