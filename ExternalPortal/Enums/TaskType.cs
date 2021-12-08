using System.ComponentModel.DataAnnotations;

namespace ExternalPortal.Enums
{
    public enum TaskType
    {
        #region "Stage one tasks"

        [Display(Name = "Tell us about your site")]
        PlantDetails,

        [Display(Name = "Planning permission")]
        PlanningPermission,

        [Display(Name = "Production details")]
        ProductionDetails,

        #endregion

        #region "Stage two tasks"

        [Display(Name = "ISAE 3000 audit document")]
        Isae3000,

        [Display(Name = "Additional supporting evidence")]
        SupportingEvidence,

        #endregion

        #region "Stage three tasks"

        [Display(Name = "Network entry agreement")]
        NetworkEntryAgreement,

        [Display(Name = "Commissioning evidence")]
        CommissioningEvidence,

        [Display(Name = "Biogas production plant details")]
        BiogasProductionPlant,

        [Display(Name = "Fuel measurement and sampling questionaire")]
        FuelMeasurementAndSamplingQuestionaire,

        [Display(Name = "Metering details")]
        MeteringDetails,

        [Display(Name = "Organisation details")]
        OrganisationDetails,

        [Display(Name = "Environmental permit")]
        EnvironmentalPermit,

        [Display(Name = "Feedstock details")]
        FeedstockDetails

        #endregion
    }
}