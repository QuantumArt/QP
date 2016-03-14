using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Quantumart.QP8.WebMvc.Extensions.ActionResults;
using Quantumart.QP8.WebMvc.ViewModels.VisualEditorConfig;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using System.Collections.Specialized;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class VisualEditorConfigController : QPController
    {
		[HttpGet]
		public ActionResult LoadVeConfig(int fieldId, int siteId)
		{
			HttpContext.Response.ContentType = "text/javascript";
			return JavaScript(this.RenderPartialView("Config", new VisualEditorConfigViewModel(fieldId, siteId)));
		}

        [HttpPost]
        public ActionResult AspellCheck(string text)
        {            
            HttpContext.Response.ContentType = "text/html; charset=utf-8";

            return View(new AspellCheckViewModel(text));
        }
    }
}
