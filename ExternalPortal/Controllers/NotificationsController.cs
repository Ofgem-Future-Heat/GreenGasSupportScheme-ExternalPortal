using System;
using System.Threading;
using System.Threading.Tasks;
using ExternalPortal.Constants;
using ExternalPortal.Models;
using ExternalPortal.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Ofgem.GovUK.Notify.Client;

namespace ExternalPortal.Controllers
{
    [AllowAnonymous]
    public class NotificationsController : Controller
    {
        private readonly ISendEmailService _sendEmailService;
        private readonly IOptions<SendEmailConfig> _sendEmailConfig;

        public NotificationsController(ISendEmailService sendEmailService, IOptions<SendEmailConfig> sendEmailConfig)
        {
            _sendEmailService = sendEmailService;
            _sendEmailConfig = sendEmailConfig;
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
                var emailParameter = new EmailParameterBuilder(EmailTemplateIds.OrganisationSubmitted, emailAddress, _sendEmailConfig.Value.ReplyToId)
                    .AddFullName(fullName)
                    .AddDashboardLink(Request.Scheme, Request.Host, Guid.NewGuid().ToString())
                    .AddCustom("ReferenceNumber", referenceNumber)
                    .Build();

                await _sendEmailService.Send(emailParameter, token);

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
                var emailParameter = new EmailParameterBuilder(EmailTemplateIds.StageOneSubmitted, emailAddress, _sendEmailConfig.Value.ReplyToId)
                    .AddFullName(fullName)
                    .AddApplicationId(Guid.NewGuid().ToString())
                    .AddDashboardLink(Request.Scheme, Request.Host, Guid.NewGuid().ToString())
                    .Build();

                await _sendEmailService.Send(emailParameter, token);

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
