using ExternalPortal.Services;
using ExternalPortal.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExternalPortal.Controllers
{
    public class StageThreeController : BaseController
    {
        public StageThreeController(IRedisCacheService redisCache)
            : base(redisCache) { }
       
        [HttpGet]
        public IActionResult FeedstockSupply()
        {
            var sessionModel = JsonConvert.DeserializeObject<StageOneModel>(HttpContext.Session.GetString("StageOneModel"));

            return View("FeedStockSupply", sessionModel);
        }

        [HttpPost]
        public IActionResult FeedstockSupply(StageOneModel model)
        {
            var sessionModel = JsonConvert.DeserializeObject<StageOneModel>(HttpContext.Session.GetString("StageOneModel"));
            sessionModel.FeedstockPlan = model.FeedstockPlan;
            //save
            HttpContext.Session.SetString("StageOneModel", JsonConvert.SerializeObject(sessionModel));

            if (model.FeedstockPlan == "I have a feedstock agreement in place")
            {
                return RedirectToAction("feedstock-upload");
            }

            if (model.FeedstockPlan == "I can self supply feedstock")
            {
                return RedirectToAction("self-supply");
            }

            model.FeedstockPlan = "I do not have plans in place yet";
            return RedirectToAction("check-answers");
        }

        [HttpGet]
        public IActionResult FeedstockUpload()
        {
            var sessionModel = JsonConvert.DeserializeObject<StageOneModel>(HttpContext.Session.GetString("StageOneModel"));
            return View("FeedstockUpload", sessionModel);
        }

        [HttpPost]
        public IActionResult FeedstockUpload(IFormFile file)
        {
            var sessionModel = JsonConvert.DeserializeObject<StageOneModel>(HttpContext.Session.GetString("StageOneModel"));
            sessionModel.FeedstockUploadFileName = file.FileName;
            //save
            HttpContext.Session.SetString("StageOneModel", JsonConvert.SerializeObject(sessionModel));
            return RedirectToAction("feedstock-upload-confirm");
        }

        [HttpGet]
        public IActionResult FeedstockUploadConfirm()
        {
            var sessionModel = JsonConvert.DeserializeObject<StageOneModel>(HttpContext.Session.GetString("StageOneModel"));
            return View("FeedstockUploadConfirm", sessionModel);
        }

        [HttpPost]
        public IActionResult FeedstockUploadConfirm(string documentAgree)
        {
            if (documentAgree == "Yes")
            {
                return RedirectToAction("check-answers");
            }
            else
            {
                return RedirectToAction("feedstock-upload");
            }
        }

        [HttpGet]
        public IActionResult SelfSupply()
        {
            var model = JsonConvert.DeserializeObject<StageOneModel>(HttpContext.Session.GetString("StageOneModel"));
            return View("FeedstockSelfSupply", model);
        }

        [HttpPost]
        public IActionResult SelfSupply(StageOneModel model)
        {
            var sessionModel = JsonConvert.DeserializeObject<StageOneModel>(HttpContext.Session.GetString("StageOneModel"));
            sessionModel.FeedstockSelfSupplyName = model.FeedstockSelfSupplyName;
            //save
            HttpContext.Session.SetString("StageOneModel", JsonConvert.SerializeObject(sessionModel));
            return RedirectToAction("check-answers");
        }


    }
}
