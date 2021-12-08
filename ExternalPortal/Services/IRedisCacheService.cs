using System;
using System.Threading;
using System.Threading.Tasks;
using ExternalPortal.Enums;
using ExternalPortal.ViewModels;
using Ofgem.API.GGSS.Domain.Commands.Applications;
using Ofgem.API.GGSS.Domain.Models;

namespace ExternalPortal.Services
{
    public interface IRedisCacheService
    {
        #region Organisation registration

        Task<PortalViewModel<OrganisationModel>> GetOrgRegistrationAsync(Guid userId, CancellationToken token);
        Task<bool> SaveOrgRegistrationAsync(Guid userId, string targetProperty, object value, CancellationToken token);
        
        #endregion Organisation registration

        #region Application StageOne

        Task<PortalViewModel<StageOne>> GetApplicationStageOneAsync(Guid applicationId, CancellationToken token);
        Task<bool> SaveAsync(PortalViewModel<StageOne> application, CancellationToken token = default);

        #endregion Application StageOne

        #region Application Summary

        Task<TaskListViewModel> GetApplicationSummaryAsync(Guid applicationId, CancellationToken token);
        Task<bool> UpdateApplicationTaskStatusAsync(Guid applicationId, ApplicationStage stage, TaskType task, Enums.TaskStatus newStatus, CancellationToken token);

        #endregion Application Summary
    }
}

