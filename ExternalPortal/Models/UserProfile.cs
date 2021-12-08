using System;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace ExternalPortal.Models
{
    public class UserProfile
    {
        public Guid Id { get; }
        public string Email { get; }
        public string FirstName { get; }
        public string LastName { get; }
        public string TelephoneNumber { get; }
        public string DisplayName { get => $"{FirstName} {LastName}"; }

        public UserProfile(ClaimsPrincipal user)
        {
            if (IsAuthenticated(user))
            {
                Id = new Guid(user.Claims.SingleOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value);
                FirstName = user.Claims.SingleOrDefault(claim => claim.Type == ClaimTypes.GivenName)?.Value;
                LastName = user.Claims.SingleOrDefault(claim => claim.Type == ClaimTypes.Surname)?.Value;
                Email = user.Claims.SingleOrDefault(Claim => Claim.Type == ClaimTypes.Email)?.Value
                            ?? user.Claims.SingleOrDefault(Claim => Claim.Type == "signInNames.emailAddress")?.Value;
                TelephoneNumber = user.Claims.SingleOrDefault(Claim => Claim.Type == "telephoneNumber")?.Value;
            }
        }

        public string ToLogMessageString()
        {
            var sb = new StringBuilder();

            sb.AppendLine("********* Authenticated User *********");
            sb.AppendLine($"Id: {this.Id}");
            sb.AppendLine($"Email: {this.Email}");
            sb.AppendLine($"FirstName: {this.FirstName}");
            sb.AppendLine($"LastName: {this.LastName}");
            sb.AppendLine($"TelephoneNumber: {this.TelephoneNumber}");
            sb.AppendLine($"DisplayName: {this.DisplayName}");
            sb.AppendLine("**************************************");

            return sb.ToString();
        }

        private bool IsAuthenticated(ClaimsPrincipal user)
        {
            return user != null && user.Identity != null && user.Identity.IsAuthenticated;
        }
    }
}
