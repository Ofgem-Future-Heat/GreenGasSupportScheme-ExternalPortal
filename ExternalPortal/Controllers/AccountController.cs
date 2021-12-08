using ExternalPortal.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;


namespace ExternalPortal.Controllers
{
    public class AccountController : Controller
    {
        [Route("/register")]
        public IActionResult Register()
        {
            return RedirectToAction("Index", "Dashboard");
        }

        [AllowAnonymous]
        [Route("/account/sign-out")]
        public IActionResult SignedOut()
        {
            return RedirectToAction("Index", "Home");
        }
    }
}
