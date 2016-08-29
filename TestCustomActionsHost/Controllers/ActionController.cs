using System.Web.Mvc;
using TestCustomActionsHost;
using TestCustomActionsHost.Models;

namespace TestCustomActionsHost.Controllers
{
    [ValidateInput(false)]
    public class ActionController : Controller
    {
        //
        // GET: /ActionOne/

        public ActionResult Index(string hostUID)
        {
            return View(new BackendActionViewModel 
            {				
                HostUID = hostUID
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

        public ActionResult ContextIndex(string hostUID)
        {
            return View(new BackendActionViewModel
            {
                HostUID = hostUID,
                EntityId = 106401,
                ParentEntityId = 339
            });
        }

        [HttpGet]
        public ActionResult NonInterface()
        {
            return new JsonpResult {Data = null, JsonRequestBehavior=JsonRequestBehavior.AllowGet };
        }

    }
}
