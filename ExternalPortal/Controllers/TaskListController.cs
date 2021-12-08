using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ExternalPortal.Constants;
using ExternalPortal.Enums;
using ExternalPortal.Extensions;
using ExternalPortal.Services;
using ExternalPortal.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Ofgem.API.GGSS.Domain.Enums;
using Ofgem.API.GGSS.Domain.ModelValues;
using Ofgem.Azure.Redis.Data.Contracts;
using TaskStatus = ExternalPortal.Enums.TaskStatus;

namespace ExternalPortal.Controllers
{
    public class TaskListController : BaseController
    {
        private readonly IAzureRedisStore<InstallationModel> _redisStore;
        private readonly IGetApplicationService _getApplicationService;
        private readonly IUpdateApplicationService _updateApplicationService;

        public TaskListController(
            IRedisCacheService redisCache,
            IAzureRedisStore<InstallationModel> redisStore,
            IGetApplicationService getApplicationService,
            IUpdateApplicationService updateApplicationService)
            : base(redisCache)
        {
            _redisStore = redisStore ?? throw new ArgumentNullException(nameof(redisStore));
            _getApplicationService = getApplicationService;
            _updateApplicationService = updateApplicationService;
        }

        #region FLOW

        // Task 1 - Tell us about your site
        // - What you will need - what-you-will-need

        // - Plant location (string/enum - England/Scotland/Wales) - plant-location

        // - Capacity upload (one file) - capacity-upload
        // - Capacity upload confirmation - capacity-upload-confirm

        // - Name of installation (string/text field) - plant-name

        // - Address of installation - plant-address

        // - Check your answers - check-answers


        // Task 2 - Planning permission
        // - What you will need - what-you-will-need

        // - Do you have planning permission (radio options) - planning-permission

        // - Planning permission doc upload (file upload) - planning-upload
        // - Planning upload confirmation - planning-upload-confirm

        // - Check your answers - check-answers

        // Task 3 - Production details
        // - What you will need - what-you-will-need

        // - Max initial capacity (number) - production-details

        // - Start date - start-date (datetime)

        // - Check your answers - check-answers

        // Task 4 - Feedstock details
        // - What you will need - what-you-will-need

        // - Feedstock supply (radio options) - feedstock-supply

        // - Feedstock upload (file upload) - feedstock-upload
        // - Feedstock upload confirmation - feedstock-upload-confirm

        // - Check your answers - check-answers

        #endregion FLOW

        public async Task<IActionResult> Index(CancellationToken token)
        {
            var setupAsync = await SetupApplicationStageOneAsync(token)
                .ContinueWith(async draftAsync =>
                {
                    var threeAndFour = await draftAsync;

                    if (threeAndFour == null)
                    {
                        throw new InvalidOperationException(nameof(threeAndFour));
                    }

                    var tempSetup = await Request
                        .GetNewInstallationFromCache(_redisStore)
                        .ContinueWith(async existingAsync =>
                        {
                            var oneAndTwo = await existingAsync;

                            if (oneAndTwo == null)
                            {
                                oneAndTwo = new InstallationModel
                                {
                                    OrganisationId = CurrentOrganisationId,
                                    UserId = User.GetUserId()
                                };

                                oneAndTwo.ReferenceParams = oneAndTwo.GetReturnToYourApplicationLink();
                                oneAndTwo.StageOne.PlantDetails.BackAction = oneAndTwo.GetBackActionLink();
                                oneAndTwo.StageOne.PlantDetails.ReferenceParams = oneAndTwo.ReferenceParams;
                                oneAndTwo.StageOne.PlanningDetails.ReferenceParams = oneAndTwo.ReferenceParams;

                                if (await _redisStore.SaveAsync(oneAndTwo))
                                {
                                    Response.Cookies.Append(CookieKeys.NewInstallation, oneAndTwo.Id.ToString());
                                    return true;
                                }
                            }

                            return oneAndTwo != null;
                        },
                        token);

                    if (await tempSetup)
                    {
                        return await EditSummaryAsync(token);
                    }

                    throw new InvalidOperationException("Failed setup");
                },
                token);
            return await setupAsync;
        }
        
        [HttpGet]
        [Route("/task-list/{applicationId}")]
        public IActionResult TaskListForApplication(string applicationId)
        {
            CurrentPersistedApplicationId = Guid.Parse(applicationId);

            return Redirect("/task-list");
        }

        private async Task<IActionResult> EditSummaryAsync(CancellationToken token)
        {
            var applicationSummary = new TaskListViewModel();

            applicationSummary.InitTaskItems();
            
            var persistedApplication = await _getApplicationService.Get(new GetApplicationRequest()
            {
                ApplicationId = CurrentPersistedApplicationId.ToString()
            }, token);

            applicationSummary.StageOneApproved = persistedApplication.Application.Status == ApplicationStatus.StageOneApproved;

            applicationSummary.StageTwoApproved = persistedApplication.Application.Status == ApplicationStatus.StageTwoApproved;

            applicationSummary.ApplicationProgress = persistedApplication.Application.Status.GetStatusInt();

            applicationSummary.ApplicationId = CurrentPersistedApplicationId;

            applicationSummary.StageOneTasks().Find(t => t.TaskType == TaskType.PlantDetails).Status =
                Enum.Parse<TaskStatus>(persistedApplication.Application.StageOne.TellUsAboutYourSite
                    .Status);

            applicationSummary.StageOneTasks().Find(t => t.TaskType == TaskType.PlanningPermission).Status =
                Enum.Parse<TaskStatus>(persistedApplication.Application.StageOne.ProvidePlanningPermission
                    .Status);

            applicationSummary.StageOneTasks().Find(t => t.TaskType == TaskType.ProductionDetails).Status =
                Enum.Parse<TaskStatus>(persistedApplication.Application.StageOne.ProductionDetails.Status);

            applicationSummary.StageTwoTasks().Find(t => t.TaskType == TaskType.Isae3000).Status =
                Enum.Parse<TaskStatus>(persistedApplication.Application.StageTwo.Isae3000.Status);

            applicationSummary.StageTwoTasks().Find(t => t.TaskType == TaskType.SupportingEvidence).Status =
                Enum.Parse<TaskStatus>(persistedApplication.Application.StageTwo
                    .AdditionalSupportingEvidence.Status);

            applicationSummary.StageThreeTasks().Find(t => t.TaskType == TaskType.FeedstockDetails).Status =
                Enum.Parse<TaskStatus>(persistedApplication.Application.StageThree.FeedstockDetails.Status);

            return View(applicationSummary);
        }
        
        [HttpPost]
        public async Task<IActionResult> SaveStageOne(CancellationToken token)
        {
            var persistedApplication = await RetrieveApplicationFromApi(token);

            persistedApplication.Status = ApplicationStatus.StageOneSubmitted;
            
            persistedApplication.StageOne.TellUsAboutYourSite.Status = TaskStatus.Submitted.ToString();
            persistedApplication.StageOne.ProvidePlanningPermission.Status = TaskStatus.Submitted.ToString();
            persistedApplication.StageOne.ProductionDetails.Status = TaskStatus.Submitted.ToString();
            
            persistedApplication.StageTwo.Isae3000.Status = TaskStatus.NotStarted.ToString();
            persistedApplication.StageTwo.AdditionalSupportingEvidence.Status = TaskStatus.NotStarted.ToString();

            await PersistApplicationToApi(persistedApplication, token);
            
            return RedirectToAction("Stage1Confirmation", "StageOne");
        }

        [HttpPost]
        public async Task<IActionResult> SaveStageTwo(CancellationToken token)
        {
            var persistedApplication = await RetrieveApplicationFromApi(token);

            persistedApplication.Status = ApplicationStatus.StageTwoSubmitted;
            persistedApplication.StageTwo.Isae3000.Status = TaskStatus.Submitted.ToString();
            persistedApplication.StageTwo.AdditionalSupportingEvidence.Status = TaskStatus.Submitted.ToString();

            await PersistApplicationToApi(persistedApplication, token);
            
            return RedirectToAction("Stage2Confirmation", "StageTwo");
        }

        [HttpPost]
        public async Task<IActionResult> SaveStageThree(CancellationToken token)
        {
            var persistedApplication = await RetrieveApplicationFromApi(token);

            persistedApplication.Status = ApplicationStatus.StageThreeSubmitted;
            persistedApplication.StageThree.FeedstockDetails.Status = TaskStatus.Submitted.GetDisplayName();

            await PersistApplicationToApi(persistedApplication, token);

            return RedirectToAction("Stage3Confirmation", "StageThree");
        }

        //[Route("/feedstock-details/what-you-will-need")]
        //[Route("/producation-details/what-you-will-need")]
        //[Route("/planning-permission/what-you-will-need")]
        //[Route("/apply/plant")]
        public async Task<IActionResult> WhatYouWillNeedStageOne(CancellationToken token)
        {
            var app = await SetupApplicationStageOneAsync(token);

            return await Task.FromResult(View(app));
        }
        
        private async Task PersistApplicationToApi(ApplicationValue persistedApplication, CancellationToken token)
        {
            await _updateApplicationService.Update(new UpdateApplicationRequest()
            {
                Application = persistedApplication,
                ApplicationId = CurrentPersistedApplicationId.ToString(),
                UserId = UserId.ToString()
            }, token);
        }

        private async Task<ApplicationValue> RetrieveApplicationFromApi(CancellationToken token)
        {
            return (await _getApplicationService.Get(new GetApplicationRequest()
            {
                ApplicationId = CurrentPersistedApplicationId.ToString()
            }, token)).Application;
        }

    }
}
