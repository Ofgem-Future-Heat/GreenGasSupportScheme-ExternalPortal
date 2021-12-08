using System;
using System.Threading.Tasks;
using ExternalPortal.Constants;
using ExternalPortal.ViewModels;
using Microsoft.AspNetCore.Http;
using Ofgem.Azure.Redis.Data.Contracts;

namespace ExternalPortal.Extensions
{
    public static class HttpRequestExtensions 
    {
        public static async Task<InstallationModel> GetNewInstallationFromCache(this HttpRequest request, IAzureRedisStore<InstallationModel> redisStore)
        {
            if (request.Cookies.ContainsKey(CookieKeys.NewInstallation))
            {
                Guid installationGuid;
                var installationId = request.Cookies[CookieKeys.NewInstallation];

                if (Guid.TryParse(installationId, out installationGuid))
                {
                    return await redisStore.GetAsync(installationGuid, default);
                }
            }

            return null;
        }
    }
}
