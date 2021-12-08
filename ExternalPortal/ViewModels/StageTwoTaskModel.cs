using ExternalPortal.ViewModels.Tasks;

namespace ExternalPortal.ViewModels
{
    public class StageTwoTaskModel 
    {
        public Isae3000AuditModel Isae3000AuditModel { get; set; }
        public AdditionalFinancialEvidenceModel AdditionalFinancialEvidence { get; set; }

        public StageTwoTaskModel()
        {
            Isae3000AuditModel = new Isae3000AuditModel();
            AdditionalFinancialEvidence = new AdditionalFinancialEvidenceModel();
        }
    }
}