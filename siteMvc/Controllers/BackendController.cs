using Microsoft.AspNetCore.Mvc;
using Quantumart.QP8.WebMvc.Extensions.Controllers;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class BackendController : AuthQpController
    {
        public ActionResult Index()
        {
            return Redirect("~/");
        }
    }
}
