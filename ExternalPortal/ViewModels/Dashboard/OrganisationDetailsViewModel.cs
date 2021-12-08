using System.Collections.Generic;
using System.Text;
using Ofgem.API.GGSS.Domain.Models;
using Ofgem.API.GGSS.Domain.ModelValues;

namespace ExternalPortal.ViewModels
{
    public class OrganisationDetailsViewModel
    {
        public List<string> Errors { get; set; } = new List<string>();
        public string OrganisationId { get; set; }
        public string OrganisationType { get; set; }
        public string OrganisationName { get; set; }
        public AddressModel OrganisationAddress { get; set; }
        public string ResponsiblePersonName { get; set; }
        public string ResponsiblePersonEmail { get; set; }
        public DocumentValue PhotoId { get; set; }
        public DocumentValue ProofOfAddress { get; set; }
        public DocumentValue LetterOfAuthority { get; set; }
        public List<UserModel> OrganisationUsers { get; set; }

        public string GetFormattedAddress()
        {
            if (OrganisationAddress == null)
            {
                return "<p>Address not found</p>";
            }

            var lineOne = !string.IsNullOrWhiteSpace(OrganisationAddress.LineOne)
                ? OrganisationAddress.LineOne
                : "Address line one not provided";
            
            var town = !string.IsNullOrWhiteSpace(OrganisationAddress.Town)
                ? OrganisationAddress.Town
                : "Town/city not provided";
            
            var postcode = !string.IsNullOrWhiteSpace(OrganisationAddress.Postcode)
                ? OrganisationAddress.Postcode
                : "Postcode not provided";

            var stringBuilder = new StringBuilder();
            
            stringBuilder.Append($"<p>{lineOne}</p>");

            if (!string.IsNullOrWhiteSpace(OrganisationAddress.LineTwo))
            {
                stringBuilder.Append($"<p>{OrganisationAddress.LineTwo}</p>");
            }
            
            stringBuilder.Append($"<p>{town}</p>");
            
            if (!string.IsNullOrWhiteSpace(OrganisationAddress.County))
            {
                stringBuilder.Append($"<p>{OrganisationAddress.County}</p>");
            }
 
            stringBuilder.Append($"<p>{postcode}</p>");

            return stringBuilder.ToString();
        }
    }
}