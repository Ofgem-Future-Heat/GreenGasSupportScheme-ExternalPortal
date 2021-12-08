using System;

namespace ExternalPortal.Helpers
{
    public class ReferenceNumber
    {
        protected ReferenceNumber() { }

        public static string GetNext()
        {
            return Guid.NewGuid().ToString().Substring(9, 9).ToUpper();
        }
    }
}
