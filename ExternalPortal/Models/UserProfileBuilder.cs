using ExternalPortal.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace ExternalPortal.Models
{
    public static class UserProfileBuilder
    {
        public static UserProfile CreateForCurrentUser(IServiceProvider serviceProvider)
            => serviceProvider.GetService<IHttpContextAccessor>().HttpContext.User.UserProfile();
    }
}
