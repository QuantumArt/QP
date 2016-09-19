using System.Web.Mvc;
using Quantumart.QP8.WebMvc.Extensions.Controllers;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class DiagnosticsController : QPController
    {
        [HttpGet]
        public ActionResult Index()
        {
            return JsonHtml("Index", new object());
        }
    }
}
