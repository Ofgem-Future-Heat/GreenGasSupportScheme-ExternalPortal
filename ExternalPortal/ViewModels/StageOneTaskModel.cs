namespace ExternalPortal.ViewModels
{
    public class StageOneTaskModel
    {
        public StageOneTaskModel()
        {
            PlantDetails = new PlantDetailsModel();
            PlanningDetails = new PlanningDetailsModel();
            ProductionDetails = new ProductionDetailsModel();
        }

        public PlantDetailsModel PlantDetails { get; set; }
        public PlanningDetailsModel PlanningDetails { get; set; }
        public ProductionDetailsModel ProductionDetails { get; set; }
    }
}
