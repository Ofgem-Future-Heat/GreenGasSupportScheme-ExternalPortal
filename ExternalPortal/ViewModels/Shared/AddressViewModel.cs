using System.ComponentModel.DataAnnotations;
using GGSS.WebCommon.Attributes;

namespace ExternalPortal.ViewModels.Shared
{
    public class AddressViewModel 
    {
        [ValidateCharacters]
        public string Name { get; set; }

        [Required(ErrorMessage = "Enter building and street")]
        [ValidateCharacters]
        public string LineOne { get; set; }

        [ValidateCharacters]
        public string LineTwo { get; set; }
        
        [Required(ErrorMessage = "Enter town or city")]
        [ValidateCharacters]
        public string Town { get; set; }

        [ValidateCharacters]
        public string County { get; set; }
        
        [Required(ErrorMessage = "Enter postcode")]
        [ValidateCharacters]
        public string Postcode { get; set; }

        [ValidateCharacters]
        public string ReturnUrl { get; set; }
    }
}
