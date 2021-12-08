using ExternalPortal.Models;
using Ofgem.API.GGSS.Domain.ModelValues;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace ExternalPortal.Extensions
{
    public static class UserExtensions
    {
        public static UserProfile UserProfile(this ClaimsPrincipal user)
        {
            return new UserProfile(user);
        }

        public static Guid GetUserId(this ClaimsPrincipal user)
        {
            return new UserProfile(user).Id;
        }

        public static string GetDisplayName(this ClaimsPrincipal user)
        {
            return new UserProfile(user).DisplayName;
        }

        public static string GetEmailAddress(this ClaimsPrincipal user)
        {
            return new UserProfile(user).Email;
        }

        public static List<Guid> GetUserOrganisationIds(this ClaimsPrincipal user)
        {
            var orgIds = new List<Guid>();
            if (user == null) return orgIds;

            user.Claims.Where(c => c.Type == "org").ToList().ForEach(c =>
            {
                if (Guid.TryParse(c.Value, out Guid orgId)) orgIds.Add(orgId);
            });

            return orgIds = new List<Guid> { new Guid("B141AC41-D6F7-47FD-B31E-847D77134FCA") };
        }

        public static string GetClaim(this ClaimsPrincipal user, string claimType)
        {
            return user.Claims.SingleOrDefault(c => c.Type == claimType)?.Value;
        }

        public static string GetFullName(this UserValue userValue)
        {
            return string.Format($"{userValue.Name} {userValue.Surname}");
        }

        public static string GetBearerTokenString(this ClaimsPrincipal user)
        {
            return @"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpZCI6IjMyNmZhOTc0LTdjMDUtNGIzNy1hOGU0LTZkNWZlNmRlYjYzYiIsIm5iZiI6MTYyNzQ4MDk1MywiZXhwIjoxNjU5MDE2OTUzLCJpYXQiOjE2Mjc0ODA5NTN9.pYB3Bi65_NFCulqYdbJdhUraONb4lH__Gs9YoZbiLZM";
        }
    }
}
