using System;
using System.Threading;
using System.Threading.Tasks;
using ExternalPortal.Constants;
using ExternalPortal.Models;
using ExternalPortal.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExternalPortal.Controllers
{
    [AllowAnonymous]
    public class NotificationsController : Controller
    {
        private readonly ISendEmailService _sendEmailService;

        public NotificationsController(ISendEmailService sendEmailService)
        {
            _sendEmailService = sendEmailService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> OrganisationSubmitted(string emailAddress, string fullName, string referenceNumber, CancellationToken token)
        {
            ViewBag.Heading = "Send response: organisation submitted";

            try
            {
                var emailParameter = new EmailParameterBuilder(EmailTemplateIds.OrganisationSubmitted, emailAddress)
                    .AddFullName(fullName)
                    .AddDashboardLink(Request.Scheme, Request.Host, Guid.NewGuid())
                    .AddCustom("ReferenceNumber", referenceNumber)
                    .Build();

                await _sendEmailService.Send(emailParameter, token, true);

                ViewBag.StatusCode = 200;
                ViewBag.Message = "Organisation submitted email sent";
            }
            catch (Exception ex)
            {
                ViewBag.StatusCode = 500;
                ViewBag.Message = ex.Message;
            }

            return View("SendResponse");
        }

        [HttpPost]
        public async Task<IActionResult> StageOneSubmitted(string emailAddress, string fullName, CancellationToken token)
        {
            ViewBag.Heading = "Send response: stage one submitted";

            try
            {
                var emailParameter = new EmailParameterBuilder(EmailTemplateIds.StageOneSubmitted, emailAddress)
                    .AddFullName(fullName)
                    .AddApplicationId(Guid.NewGuid())
                    .AddDashboardLink(Request.Scheme, Request.Host, Guid.NewGuid())
                    .Build();

                await _sendEmailService.Send(emailParameter, token, true);

                ViewBag.StatusCode = 200;
                ViewBag.Message = "Stage one submitted email sent";
            }
            catch (Exception ex)
            {
                ViewBag.StatusCode = 500;
                ViewBag.Message = ex.Message;
            }

            return View("SendResponse");
        }
    }
}
