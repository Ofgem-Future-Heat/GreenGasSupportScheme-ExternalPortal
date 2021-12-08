using System.Net;

namespace ExternalPortal.ViewModels
{
    public class CustomErrorViewModel
    {
        public HttpStatusCode StatusCode { get; }
        public string ErrorText { get; }

        public CustomErrorViewModel(HttpStatusCode statusCode)
        {
            StatusCode = statusCode;

            ErrorText = statusCode switch
            {
                HttpStatusCode.Unauthorized => "You are not currently logged in, and cannot view this page as a guest.",
                HttpStatusCode.Forbidden =>
                    "You don't have the correct permissions to access this page for the current organisation. Please get in touch with this organisation's administrators to request access.",
                _ => ErrorText
            };
        }
    }
}
