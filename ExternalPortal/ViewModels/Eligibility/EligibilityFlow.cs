namespace ExternalPortal.ViewModels.Eligibility
{
    public class EligibilityFlow
    {
        public string GasInjection { get; set; }

        public string NewPlant { get; set; }

        public string FinancialSupport { get; set; }

        public string Error { get; set; }

        public bool HasError => !string.IsNullOrWhiteSpace(Error);
    }
}
