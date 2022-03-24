using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ExternalPortal.ViewModels.Shared.Layouts;
using GGSS.WebCommon.Attributes;

namespace ExternalPortal.ViewModels
{
    public class LatitudeLongitudeModel : SimplePageHeaderLayoutViewModel
    {
        public LatitudeLongitudeValue LatitudeLongitudeAnaerobic { get; set; }
        public LatitudeLongitudeValue LatitudeLongitudeInjection { get; set; }
        public string ReturnUrl { get; set; }
    }

    public class LatitudeLongitudeValue
    {
        [Required(ErrorMessage = "Enter latitude value between -90 and 90")]
        [ValidateLatitude]
        public double? Latitude { get; set; }

        [Required(ErrorMessage = "Enter longitude value between -180 and 180")]
        [ValidateLongitude]
        public double? Longitude { get; set; }
    }
}
