using System.Web.Mvc;

namespace Quantumart.QP8.WebMvc.Extensions.ActionResults
{
    public class FileUploadJsonResult : JsonResult
    {
        public override void ExecuteResult(ControllerContext context)
        {
            ContentType = "text/html";
            context.HttpContext.Response.Write("<textarea>");

            base.ExecuteResult(context);
            context.HttpContext.Response.Write("</textarea>");
        }
    }
}
