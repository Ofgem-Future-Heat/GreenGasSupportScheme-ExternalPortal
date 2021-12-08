using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExternalPortal.Extensions
{
    public static class CollectionExtensions
    {
        public static string ToHtmlList(this IList<string> list)
        {
            if (list == null || !list.Any()) return string.Empty;

            var sb = new StringBuilder();

            var last = list.Last();

            foreach(string item in list)
            {
                if (item.Equals(last)) sb.Append($"{item}");
                else sb.Append($"{item}<br />");
            }
                    
            return sb.ToString();
        }
    }
}
