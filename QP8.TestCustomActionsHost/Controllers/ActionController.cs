using System.Web.Mvc;
using QP8.TestCustomActionsHost.Models;

namespace QP8.TestCustomActionsHost.Controllers
{
    [ValidateInput(false)]
    public class ActionController : Controller
    {
        public ActionResult Index(string hostUid)
        {
            return View(new BackendActionViewModel
            {
                HostUid = hostUid
            });
        }

        public ActionResult Notify(string eventName, string newXml)
        {
            return Content("OK");
        }

        public ActionResult NotifyWithService(string eventName, string newXml, string oldXml, int contentId, int siteId, int[] id)
        {
            return Content("OK");
        }

        public ActionResult ContextIndex(string hostUid)
        {
            return View(new BackendActionViewModel
            {
                HostUid = hostUid,
                EntityId = 106401,
                ParentEntityId = 339
            });
        }

        [HttpGet]
        public ActionResult NonInterface()
        {
            return new JsonpResult { Data = null, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
    }
}
