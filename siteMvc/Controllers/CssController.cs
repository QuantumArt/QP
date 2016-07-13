using System.Web.Mvc;
using Quantumart.QP8.WebMvc.ViewModels;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.BLL.Services;

namespace Quantumart.QP8.WebMvc.Controllers
{
	public class CssController : QPController
	{
		IStatusTypeService _statusTypeService;
		
		public CssController(IStatusTypeService statusTypeService)
		{
			this._statusTypeService = statusTypeService;
		}

		[HttpGet]
		public ActionResult CustomCss()
		{
			HttpContext.Response.ContentType = "text/css";
			return Content(this.RenderPartialView("CustomCss", new CustomCssViewModel(_statusTypeService.GetColouredStatuses())));
		}
	}
}
