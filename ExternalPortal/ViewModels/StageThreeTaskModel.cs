namespace ExternalPortal.ViewModels
{
    public class StageThreeTaskModel 
    {
        public StageThreeTaskModel()
        {
            NetworkEntryAgreement = new NetworkEntryAgreementModel();
            CommissioningEvidence = new CommissioningEvidenceModel();
            BiogasProductionPlant = new BiogasProductionPlantModel();
            FuelMeasurementAndSamplingQuestionaire = new FuelMeasurementAndSamplingQuestionaireModel();
            MeteringDetails = new MeteringDetailsModel();
            FeedstockDetails = new FeedstockDetailsModel();
        }

        public NetworkEntryAgreementModel NetworkEntryAgreement { get; set; }
        public CommissioningEvidenceModel CommissioningEvidence { get; set; }
        public BiogasProductionPlantModel BiogasProductionPlant { get; set; }
        public FuelMeasurementAndSamplingQuestionaireModel FuelMeasurementAndSamplingQuestionaire { get; set; }
        public MeteringDetailsModel MeteringDetails { get; set; }
        public FeedstockDetailsModel FeedstockDetails { get; set; }
    }
}