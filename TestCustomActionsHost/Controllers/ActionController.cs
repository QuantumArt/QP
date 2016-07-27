using System.Web.Mvc;
using TestCustomActionsHost;
using TestCustomActionsHost.Models;

namespace TestCustomActionsHost.Controllers
{
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
