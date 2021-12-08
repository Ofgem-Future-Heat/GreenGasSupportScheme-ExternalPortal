using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ExternalPortal.Extensions;
using ExternalPortal.Services;
using Microsoft.AspNetCore.Mvc;
using Ofgem.API.GGSS.Domain.ModelValues;

namespace ExternalPortal.Controllers
{
    public class ApplicationController : Controller
    {
        private readonly IGetApplicationService _getApplicationService;
        private readonly IUpdateApplicationService _updateApplicationService;

        public ApplicationController(
            IGetApplicationService getApplicationService,
            IUpdateApplicationService updateApplicationService)
        {
            _getApplicationService = getApplicationService;
            _updateApplicationService = updateApplicationService;
        }

        public override RedirectToActionResult RedirectToAction(string actionName)
        {
            return base.RedirectToAction(actionName, new { ApplicationId = CurrentPersistedApplicationId });
        }

        public override RedirectToActionResult RedirectToAction(string actionName, string controllerName)
        {
            return base.RedirectToAction(actionName, controllerName, new { ApplicationId = CurrentPersistedApplicationId });
        }

        protected string CurrentPersistedApplicationId
        {
            get
            {
                return Guid.Parse(Request.Query["ApplicationId"]).ToString();
            }
        }

        protected async Task<ApplicationValue> RetrieveCurrentApplicationFromApi()
        {
            var response = await _getApplicationService.Get(
                new GetApplicationRequest()
                {
                    ApplicationId = CurrentPersistedApplicationId
                },
                CancellationToken.None);

            return response.Application;
        }

        protected async Task PersistApplicationToApi(ApplicationValue application)
        {
            await _updateApplicationService.Update(
                new UpdateApplicationRequest()
                {
                    Application = application,
                    ApplicationId = CurrentPersistedApplicationId,
                    UserId = User.GetUserId().ToString()
                },
                CancellationToken.None);
        }
    }
}
