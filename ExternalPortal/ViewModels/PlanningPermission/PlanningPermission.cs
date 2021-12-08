using System.Collections.Generic;
using System.Linq;
using GGSS.WebCommon.Attributes;
using GovUkDesignSystem.Attributes.ValidationAttributes;

namespace ExternalPortal.ViewModels.PlanningPermission
{
    public class PlanningPermission
    {
        public string FileId { get; set; }

        public string Filename { get; set; }

        public string FileSizeAsString { get; set; }
        
        [GovUkValidateRequired(ErrorMessageIfMissing = "Tell us why planning permission is not required")]
        [ValidateCharacters]
        public string ExemptionStatement { get; set; }

        public List<string> Errors { get; set; }

        public bool HasErrors => Errors != null && Errors.Any();
    }
}
