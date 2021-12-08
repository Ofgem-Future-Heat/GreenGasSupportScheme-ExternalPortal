using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ExternalPortal.Constants;
using ExternalPortal.Enums;
using ExternalPortal.Extensions;
using ExternalPortal.Services;
using ExternalPortal.ViewModels;
using ExternalPortal.ViewModels.Tasks;
using Microsoft.AspNetCore.Mvc;
using Ofgem.API.GGSS.Domain.Commands.Applications;

namespace ExternalPortal.Controllers
{
    public class BaseController : Controller
    {
        public BaseController(IRedisCacheService redisCache)
        {
            this.RedisCache = redisCache ?? throw new ArgumentNullException(nameof(redisCache));
        }

        protected readonly IRedisCacheService RedisCache;
        protected Guid UserId => User.GetUserId();
        protected Guid[] UserOrganisationIds => User.GetUserOrganisationIds().ToArray(); 
        protected string UserDisplayName => User.GetDisplayName();

        protected Guid CurrentOrganisationId
        {
            get
            {
                return Guid.Parse("b141ac41-d6f7-47fd-b31e-847d77134fca");
            }
            set
            {
                Response.Cookies.Append(CookieKeys.CurrentOrganisationId, value.ToString());
            }
        }

        protected Guid CurrentApplicationId
        {
            get
            {
                if (Guid.TryParse(Request.Cookies[CookieKeys.NewApplication], out Guid id))
                {
                    return id;
                }

                return Guid.Empty;
            }
            set
            {
                Response.Cookies.Append(CookieKeys.NewApplication, value.ToString());
            }
        }
        
        protected Guid CurrentPersistedApplicationId
        {
            get
            {
                if (Guid.TryParse(Request.Cookies[CookieKeys.PersistedApplicationId], out Guid id))
                {
                    return id;
                }

                return Guid.Empty;
            }
            set
            {
                Response.Cookies.Append(CookieKeys.PersistedApplicationId, value.ToString());
            }
        }

        protected Dictionary<TaskType,string> TaskStartRouteUris 
        { 
            get
            {
                var routes = new Dictionary<TaskType, string>
                {
                    { TaskType.PlantDetails, "/apply/plant/anaerobic-digester-location" },
                    { TaskType.PlanningPermission, "/planning-permission/what-you-will-need" },
                    { TaskType.ProductionDetails, "/production-details/what-you-will-need" },
                    { TaskType.FeedstockDetails, "/feedstock-details/what-you-will-need" }
                };

                return routes;
            } 
        }

        protected Dictionary<TaskType,List<string>> TaskRequirements
        {
            get
            {
                return new Dictionary<TaskType, List<string>>()
                {
                    { TaskType.PlantDetails, new List<string>{ "Plant name", "Plant location", "Plant Address" } },
                    { TaskType.PlanningPermission, new List<string>{ "wether you have planning permission", "upload documents about your planning permission" } },
                    { TaskType.ProductionDetails, new List<string>{ "when you plan to start injection", "maximum initial capacity of biomethane you’re permitted to inject per year", "the volume in cubic metres of eligible biomethane which you expect to inject each year" } },
                    { TaskType.FeedstockDetails, new List<string>{ "tell us who'll supply your feedstock", "upload documents about your feedstock" } },
                };
            }
        }

        protected async Task<PortalViewModel<StageOne>> SetupApplicationStageOneAsync(CancellationToken token)
        {
            var currentApplicationId = (this.CurrentApplicationId == Guid.Empty) 
                ? Guid.NewGuid() 
                : CurrentApplicationId;

            var setupAsync = await RedisCache
                .GetApplicationStageOneAsync(CurrentApplicationId, token)
                .ContinueWith(async getStageOneAsync =>
            {
                var currentApplication = await getStageOneAsync ??

                new PortalViewModel<StageOne>
                {
                    Id = currentApplicationId,
                    Model = new StageOne { OrganisationId = CurrentOrganisationId }
                };

                if (await RedisCache.SaveAsync(currentApplication, token))
                {
                    this.CurrentApplicationId = currentApplication.Id;
                    return currentApplication;
                }

                throw new InvalidOperationException($"Failed redis cache setup for application Id {currentApplicationId}");
            },
            token);

            return await setupAsync;
        }

        protected async Task<IActionResult> WhatYouWillNeedAsync(TaskType taskType, string proceedActionName, CancellationToken token)
        {
            var setupAsync = await SetupApplicationStageOneAsync(token)
                .ContinueWith(async setupAsync =>
            {
                var draft = await setupAsync;

                if (draft == null)
                {
                    throw new InvalidOperationException(nameof(draft));
                }

                return await GoToFirstScreenAsync(taskType, proceedActionName, token);
            },  token);

            return await setupAsync;
        }

        private async Task<IActionResult> GoToFirstScreenAsync(TaskType taskType, string proceedActionName, CancellationToken token)
        {
            var currentDraftAsync = await RedisCache
                .GetApplicationSummaryAsync(CurrentApplicationId, token)
                .ContinueWith(async findSummaryAsync =>
                {
                    var summary = await findSummaryAsync;

                    if (summary != null && summary.IsTaskStarted(taskType))
                    {
                        var task = summary.StageOneTasks().FirstOrDefault(t => t.TaskType == taskType);
                        // Skip WhatYouWillNeed => already accepted
                        return await RedirectToNextProperty(task);
                    }

                    var vm = new TaskRequirementsViewModel(taskType)
                    {
                        OrganisationId = CurrentOrganisationId,
                        ApplicationId = CurrentApplicationId,
                        Requirements = TaskRequirements[taskType],
                        ProceedActionName = proceedActionName,
                        BackLink = $"/task-list"
                    };

                    return View("WhatYouWillNeed", vm);
                },
                token);

            return await currentDraftAsync;
        }

        protected async Task<bool> ProceedAsync(Guid currentApplicationId, ApplicationStage stage, TaskType task, CancellationToken token)
        {
            var proceedAsync = await RedisCache.GetApplicationSummaryAsync(currentApplicationId, token)
                .ContinueWith(async summaryAsync =>
                {
                    var summary = await summaryAsync;

                    if (summary == null)
                    {
                        throw new InvalidOperationException(nameof(summary));
                    }

                    switch (stage)
                    {
                        case ApplicationStage.StageOne:
                            var currentTask = summary.StageOneTasks().FirstOrDefault(t => t.TaskType == task);
                            var taskInProgressOrComplete 
                                = (currentTask.Status == Enums.TaskStatus.InProgress) || (currentTask.Status == Enums.TaskStatus.Completed);
                            return taskInProgressOrComplete || await RedisCache.UpdateApplicationTaskStatusAsync(currentApplicationId, 
                                stage, task, Enums.TaskStatus.InProgress, token);

                        default: return false;
                    }
                }, 
                token);

            return await proceedAsync;
        }

        protected async Task<IActionResult> CompleteTaskAsync(Guid currentApplicationId, ApplicationStage stage, TaskType task, CancellationToken token)
        {
            var confirmAsync = await RedisCache
                .UpdateApplicationTaskStatusAsync(currentApplicationId, stage, task, Enums.TaskStatus.Completed, token)
                .ContinueWith(async confirmationAsync =>
                {
                    var confirmed = await confirmationAsync;
                    if (confirmed) 
                        return RedirectToAction("SaveCurrentTask", "TaskList", new { currentApplicationId, stage, task });
                    return RedirectToAction("WhatYouWillNeed");
                },
            token);

            return await confirmAsync;
        }

        private async Task<IActionResult> RedirectToNextProperty(TaskItemViewModel task)
        {
            return await Task.Factory.StartNew(() =>
            {
                switch (task.TaskType)
                {
                    case TaskType.PlantDetails: return RedirectToAction("PlantLocation", task.TaskType.ToString());
                    case TaskType.PlanningPermission: return RedirectToAction("PlanningUpload", task.TaskType.ToString());
                    case TaskType.ProductionDetails: return RedirectToAction("ProductionDetails", task.TaskType.ToString());
                    case TaskType.FeedstockDetails: return RedirectToAction("FeedstockSupply", task.TaskType.ToString());
                    default: return RedirectToAction("WhatYouWillNeed", task.TaskType.ToString());
                }
            });
        }
    }
}
