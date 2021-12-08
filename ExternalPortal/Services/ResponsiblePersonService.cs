using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ExternalPortal.Extensions;
using Microsoft.AspNetCore.Http;
using Ofgem.API.GGSS.Domain.Models;
using Ofgem.API.GGSS.Domain.ModelValues;

namespace ExternalPortal.Services
{
    public interface IResponsiblePersonService
    {
        Task<ResponsiblePersonModel> GetAsync(CancellationToken token = default);
        Task<bool> SaveAsync(ResponsiblePersonModel detail, CancellationToken token = default);
    }

    public class ResponsiblePersonService : IResponsiblePersonService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ResponsiblePersonService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public async Task<ResponsiblePersonModel> GetAsync(CancellationToken token = default)
        {
            var currentUserProfile = _httpContextAccessor.HttpContext.User.UserProfile();

            var responsiblePersonViewModel = new ResponsiblePersonModel()
            {
                User = new UserModel
                {
                    ProviderId = currentUserProfile.Id.ToString(),
                    Value = new UserValue
                    {
                        EmailAddress = currentUserProfile.Email, 
                        Name = currentUserProfile.FirstName, 
                        Surname = currentUserProfile.LastName
                    }
                },
                Value = new ResponsiblePersonValue
                {
                    TelephoneNumber = currentUserProfile.TelephoneNumber 
                }
            };

            return await Task.FromResult(responsiblePersonViewModel);
        }

        public async Task<bool> SaveAsync(ResponsiblePersonModel detail, CancellationToken token = default)
        {
            return await Task.FromException<bool>(new NotImplementedException());
        }
    }
}
