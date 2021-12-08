using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using ExternalPortal.Enums;
using ExternalPortal.ViewModels;
using Ofgem.API.GGSS.Domain.Commands.Applications;
using Ofgem.API.GGSS.Domain.Models;
using Ofgem.Azure.Redis.Data.Contracts;

namespace ExternalPortal.Services
{
    public class RedisCacheService : IRedisCacheService
    {
        private readonly IAzureRedisStore<PortalViewModel<OrganisationModel>> _organisationRegistration;
        private readonly IAzureRedisStore<PortalViewModel<StageOne>> _appStageOne;
        private readonly IAzureRedisStore<TaskListViewModel> _appSummary;

        public RedisCacheService(
            IAzureRedisStore<PortalViewModel<OrganisationModel>> organisationRegistrationCache,
            IAzureRedisStore<PortalViewModel<StageOne>> appStageOne,
            IAzureRedisStore<TaskListViewModel> appSummary
            )
        {
            _organisationRegistration = organisationRegistrationCache ?? throw new ArgumentNullException(nameof(organisationRegistrationCache));
            _appStageOne = appStageOne ?? throw new ArgumentNullException(nameof(appStageOne));
            _appSummary = appSummary ?? throw new ArgumentNullException(nameof(appSummary));
        }

        #region Organisation registration

        public async Task<bool> SaveOrgRegistrationAsync(Guid userId, string targetProperty, object value, CancellationToken token)
        {
            var updateAsync = await _organisationRegistration.GetAsync(userId, token)
                .ContinueWith(async foundAsync =>
                {
                    var found = await foundAsync;
                    if (found != null)
                    {
                        this.SetProperty(targetProperty, found, value);
                        var saved = await _organisationRegistration.SaveAsync(found, token);

                        return saved;
                    }
                    return await _organisationRegistration.SaveAsync((PortalViewModel<OrganisationModel>)value, token);
                },
                token);

            return await updateAsync;
        }

        public async Task<PortalViewModel<OrganisationModel>> GetOrgRegistrationAsync(Guid userId, CancellationToken token)
        {
            return await _organisationRegistration.GetAsync(userId, token);
        }

        #endregion Organisation registration

        #region GGSS StageOne application

        public async Task<PortalViewModel<StageOne>> GetApplicationStageOneAsync(Guid applicationId, CancellationToken token)
        {
            return await _appStageOne.GetAsync(applicationId, token);
        }

        public async Task<bool> SaveAsync(PortalViewModel<StageOne> application, CancellationToken token = default)
        {
            return await _appStageOne.SaveAsync(application, token);
        }

        #endregion GGSS StageOne application

        #region Application Summary

        public async Task<TaskListViewModel> GetApplicationSummaryAsync(Guid applicationId, CancellationToken token)
        {
            var getSummaryAsync = await _appStageOne
                .GetAsync(applicationId, token)
                .ContinueWith(async findAsync =>
                {
                    var draft = await findAsync;

                    if (draft == null)
                    {
                        return null;
                    }

                    if (draft.RedisKeys.TryGetValue(typeof(TaskListViewModel), out Guid redisKey))
                    {
                        return await _appSummary.GetAsync(redisKey, token);
                    }
                    else
                    {
                        // No summary stored yet, add new
                        var summary = new TaskListViewModel
                        { Id = Guid.NewGuid(), ApplicationId = applicationId };
                        summary.InitTaskItems();

                        if (await SaveApplicationSummaryAsync(applicationId, summary, token))
                        {
                            return summary;
                        }

                        throw new InvalidOperationException("Failed adding new summary to redis cash.");
                    }

                }, token);

            return await getSummaryAsync;
        }

        public async Task<bool> UpdateApplicationTaskStatusAsync(Guid applicationId, ApplicationStage stage, TaskType task, Enums.TaskStatus newStatus, CancellationToken token)
        {
            var updateSummaryAsync = await this
                .GetApplicationSummaryAsync(applicationId, token)
                .ContinueWith(async findAsync =>
                {
                    var summary = await findAsync;

                    if (summary == null)
                    {
                        throw new InvalidOperationException(nameof(summary));
                    }

                    switch (stage)
                    {
                        case ApplicationStage.StageOne:
                            summary.StageOneTasks().Where(t => t.TaskType == task).ToList().ForEach(s => s.Status = newStatus);
                            break;
                        case ApplicationStage.StageTwo:
                            summary.StageTwoTasks().Where(t => t.TaskType == task).ToList().ForEach(s => s.Status = newStatus);
                            break;
                        case ApplicationStage.StageThree:
                            summary.StageThreeTasks().Where(t => t.TaskType == task).ToList().ForEach(s => s.Status = newStatus);
                            break;
                    }

                    return await _appSummary.SaveAsync(summary, token);
                },
                token);
            return await updateSummaryAsync;
        }

        private async Task<bool> SaveApplicationSummaryAsync(Guid applicationId, TaskListViewModel summary, CancellationToken token)
        {
            var saveAsync = await _appStageOne
                .GetAsync(applicationId, token)
                .ContinueWith(async findAsync =>
                {
                    var draft = await findAsync;

                    if (draft == null)
                    {
                        throw new InvalidOperationException(nameof(draft));
                    }

                    if (draft.RedisKeys.TryGetValue(typeof(TaskListViewModel), out Guid redisKey))
                    {
                        summary.Id = redisKey;
                        return await _appSummary.SaveAsync(summary, token);
                    }
                    else
                    {
                        var tasks = new List<Task<bool>>();
                        if (summary.Id == Guid.Empty) summary.Id = Guid.NewGuid();
                        draft.RedisKeys.Add(typeof(TaskListViewModel), summary.Id);
                        tasks.Add(_appStageOne.SaveAsync(draft, token));
                        tasks.Add(_appSummary.SaveAsync(summary, token));
                        var saved = await Task.WhenAll(tasks.ToArray());
                        return saved.All(t => true);
                    }
                },
            token);

            return await saveAsync;
        }

        #endregion Application Summary

        private void SetProperty(string compoundProperty, object target, object value)
        {
            string[] bits = compoundProperty.Split('.');
            for (int i = 0; i < bits.Length - 1; i++)
            {
                var propertyToGet = target.GetType().GetProperty(bits[i]);
                target = propertyToGet.GetValue(target, null);
            }
            PropertyInfo propertyToSet = target.GetType().GetProperty(bits.Last());
            propertyToSet.SetValue(target, value, null);
        }

    }
}