using ExternalPortal.Constants;
using ExternalPortal.Extensions;
using ExternalPortal.Helpers;
using ExternalPortal.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ofgem.Azure.Redis.Data.Contracts;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ExternalPortal.Controllers
{
    public class InstallationsController : Controller
    {
        private readonly ILogger<InstallationsController> _logger;
        private readonly IAzureRedisStore<InstallationModel> _redisStore;

        public InstallationsController(
            ILogger<InstallationsController> logger,
            IAzureRedisStore<InstallationModel> redisStore)
        {
            _logger = logger;
            _redisStore = redisStore;
        }

        [HttpGet]
        [Route("/Dashboard/Organisations/{organisationId}/Installations")]
        public async Task<IActionResult> StartNewApplication([FromRoute] string organisationId)
        {
            _logger.LogDebug("StartNewApplication action on InstallationsController controller called");

            CheckParameter(organisationId);

            var newApplicationTaskListModel = new InstallationModel
            {
                OrganisationId = Guid.Parse(organisationId),
                UserId = User.GetUserId()
            };

            newApplicationTaskListModel.ReferenceParams = newApplicationTaskListModel.GetReturnToYourApplicationLink();
            newApplicationTaskListModel.StageOne.PlantDetails.BackAction = newApplicationTaskListModel.GetBackActionLink();
            newApplicationTaskListModel.StageOne.PlantDetails.ReferenceParams = newApplicationTaskListModel.ReferenceParams;
            newApplicationTaskListModel.StageOne.PlanningDetails.ReferenceParams = newApplicationTaskListModel.ReferenceParams;

            Response.Cookies.Append(CookieKeys.NewInstallation, newApplicationTaskListModel.Id.ToString());

            await _redisStore.SaveAsync(newApplicationTaskListModel);

            return await Task.FromResult(View("Index", newApplicationTaskListModel));
        }

        [HttpGet]
        [Route("/dashboard/organisations/{organisationId}/installations/{installationId}")]
        public async Task<IActionResult> EditApplication([FromRoute] string organisationId, [FromRoute] string installationId, CancellationToken token)
        {
            _logger.LogDebug("EditApplication action on InstallationsController controller called");

            CheckParameters(organisationId, installationId);

            Response.Cookies.Append(CookieKeys.NewInstallation, installationId.ToString());

            // Try read from cache
            var cacheModel = await Request.GetNewInstallationFromCache(_redisStore);

            if (cacheModel != null)
            {
                return await Task.FromResult(View("Index", cacheModel));
            }
            else
            {
                // Try read from database and cache it
                cacheModel = await GetFromDatabase();

                if (cacheModel != null)
                {
                    await _redisStore.SaveAsync(cacheModel);
                }

                // If we don't find an inprogress application in the cache, and dont find in the database
                // then return view which contains a list of installations for this organisation. 
                return await Task.FromResult(RedirectToAction("ListInstallations", new { organisationId = organisationId }));
            }
        }

        private async Task<InstallationModel> GetFromDatabase()
        {
            return await Task.FromResult<InstallationModel>(null);
        }

        private void CheckParameter(string organisationId)
        {
            if (string.IsNullOrWhiteSpace(organisationId))
            {
                throw new ArgumentNullException(nameof(organisationId));
            }
        }

        private void CheckParameters(string organisationId, string installationId)
        {
            CheckParameter(organisationId);

            if (string.IsNullOrWhiteSpace(installationId))
            {
                throw new ArgumentNullException(nameof(installationId));
            }
        }
    }
}
