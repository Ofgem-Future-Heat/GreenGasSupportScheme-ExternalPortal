using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExternalPortal.Controllers
{
    public class ProvisionalTariffController : Controller
    {
        public IActionResult Introduction()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult PlantLocation()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult Feedstock()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult Reporting()
        {
            return View();
        }
    }
}
