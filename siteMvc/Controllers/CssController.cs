using System.Web.Mvc;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.ViewModels;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class CssController : QPController
    {
        private readonly IStatusTypeService _statusTypeService;

        public CssController(IStatusTypeService statusTypeService)
        {
            _statusTypeService = statusTypeService;
        }

        public ActionResult CustomCss()
        {
            HttpContext.Response.ContentType = "text/css";
            return Content(RenderPartialView("CustomCss", new CustomCssViewModel(_statusTypeService.GetColouredStatuses())));
        }
    }
}
