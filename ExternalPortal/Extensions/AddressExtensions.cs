using Ofgem.API.GGSS.Domain.Models;
using System.Text;

namespace ExternalPortal.Extensions
{
    public static class AddressExtensions
    {
        public static string ToHtmlString(this AddressModel address)
        {
            if (address == null) return string.Empty;

            var sb = new StringBuilder();

            sb.Append($"{address.LineOne}<br />");

            if (!string.IsNullOrWhiteSpace(address.LineTwo))
            {
                sb.Append($"{address.LineTwo}<br />");
            }

            sb.Append($"{address.Postcode}<br />");
            sb.Append($"{address.County}");

            return sb.ToString();
        }
    }
}
