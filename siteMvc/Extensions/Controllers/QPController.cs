using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Constants;
using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Quantumart.QP8.WebMvc.Extensions.ActionResults;

namespace Quantumart.QP8.WebMvc.Extensions.Controllers
{
    public class QPController : Controller
    {
        public string RenderPartialView(string viewName, object model)
        {
            if (string.IsNullOrEmpty(viewName))
            {
                viewName = ControllerContext.RouteData.GetRequiredString("action");
            }

            ViewData.Model = model;
            using (var sw = new StringWriter())
            {
                var viewResult = ViewEngines.Engines.FindPartialView(ControllerContext, viewName);
                var viewContext = new ViewContext(ControllerContext, viewResult.View, ViewData, TempData, sw);
                viewResult.View.Render(viewContext, sw);

                return sw.GetStringBuilder().ToString();
            }
        }

        public ActionResult JsonHtml(string viewName, object model)
        {
            if (IsReplayAction())
            {
                if (ModelState.IsValid)
                {
                    return null;
                }

                var exceptions = ModelState.Values
                    .SelectMany(x => x.Errors)
                    .Select(x => x.Exception ?? new ArgumentException(x.ErrorMessage))
                    .ToArray();

                throw new AggregateException(exceptions);
            }

            return new JsonNetResult<object>(new { success = true, view = RenderPartialView(viewName, model) });
        }

        public bool IsReplayAction()
        {
            return IsReplayAction(HttpContext);
        }

        public static bool IsReplayAction(HttpContextBase context)
        {
            return context.Items.Contains("IS_REPLAY");
        }

        public static bool IsError(HttpContextBase context)
        {
            var form = context.Request.Form;
            var formResult = (form != null && form.AllKeys.Contains("isError") && bool.Parse(form["isError"]));
            return formResult || context.Items.Contains("IS_ERROR");
        }

        public ActionResult Redirect(string actionName, object routeValues)
        {
            if (IsReplayAction())
            {
                return null;
            }

            return RedirectToAction(actionName, routeValues);
        }

        public JsonNetResult<MessageResult> JsonMessageResult(MessageResult result)
        {
            if (IsReplayAction())
            {
                if (result != null && result.Type == ActionMessageType.Error)
                {
                    throw new AggregateException(new InvalidOperationException(result.Text));
                }

                return null;
            }

            if (result != null && result.Type == ActionMessageType.Error)
            {
                ControllerContext.RequestContext.HttpContext.Items.Add("IS_ERROR", true);
            }

            return result;
        }

        public ActionResult JsonEmpty()
        {
            return new JsonNetResult<object>(new { success = true, view = string.Empty });
        }

        public JsonResult JsonError(string msg)
        {
            return new JsonNetResult<object>(new { success = false, message = msg });
        }

        public JsonResult JsonError(Exception ex)
        {
            return JsonError(ex.Message);
        }

        public void PersistResultId(int id)
        {
            BackendActionContext.Current.ResetEntityId(id);
            ControllerContext.HttpContext.Items["RESULT_ID"] = id;
        }

        public void PersistFromId(int id)
        {
            ControllerContext.HttpContext.Items["FROM_ID"] = id;
        }

        public void PersistActionCode(string name)
        {
            ControllerContext.HttpContext.Items["ACTION_CODE"] = name;
        }

        public void PersistLinkId(int? oldLinkId, int? newLinkId)
        {
            if (newLinkId.HasValue && newLinkId.Value > 0 && (!oldLinkId.HasValue || oldLinkId.Value != newLinkId.Value))
            {
                ControllerContext.HttpContext.Items["NEW_LINK_ID"] = newLinkId.Value;
            }
        }

        public void PersistActionId(int id)
        {
            ControllerContext.HttpContext.Items["ACTION_ID"] = id;
        }

        public void PersistDefaultFormatId(int? defaultFormatId)
        {
            if (defaultFormatId.HasValue)
            {
                ControllerContext.HttpContext.Items["DEFAULT_FORMAT_ID"] = defaultFormatId.Value;
            }
        }

        public void PersistBackwardId(Field oldBackward, Field newBackward)
        {
            if (newBackward != null && newBackward.Id > 0 && (oldBackward == null || oldBackward.Id == 0))
            {
                ControllerContext.HttpContext.Items["NEW_BACKWARD_ID"] = newBackward.Id;
            }
        }

        private void PersistIds(string key, int[] ids)
        {
            if (ids != null && ids.Length > 0)
                ControllerContext.HttpContext.Items[key] = string.Join(",", ids);
        }

        private void PersistIds(string key, int[] oldIds, int[] ids)
        {
            if (ids != null && ids.Length > 0)
            {
                ControllerContext.HttpContext.Items[key] = string.Join(",", (oldIds != null) ? ids.Except(oldIds) : ids);
            }
        }

        public void PersistFieldIds(int[] ids)
        {
            PersistIds("FIELD_IDS", ids);
        }

        public void PersistLinkIds(int[] ids)
        {
            PersistIds("LINK_IDS", ids);
        }

        public void PersistVirtualFieldIds(int[] ids)
        {
            PersistIds("NEW_VIRTUAL_FIELD_IDS", ids);
        }

        public void PersistChildFieldIds(int[] ids)
        {
            PersistIds("NEW_CHILD_FIELD_IDS", ids);
        }

        public void PersistChildLinkIds(int[] ids)
        {
            PersistIds("NEW_CHILD_LINK_IDS", ids);
        }

        public void PersistCommandIds(int[] oldIds, int[] ids)
        {
            PersistIds("NEW_COMMAND_IDS", oldIds, ids);
        }

        public void PersistRulesIds(int[] oldIds, int[] ids)
        {
            PersistIds("NEW_RULES_IDS", oldIds, ids);
        }

        public void PersistNotificationFormatId(int? formatId)
        {
            if (formatId.HasValue)
            {
                ControllerContext.HttpContext.Items["NOTIFICATION_FORMAT_ID"] = formatId.Value;
            }
        }

        public string GetBackendUrl()
        {
            return GetBackendUrl(HttpContext);
        }

        public static string GetBackendUrl(HttpContextBase context)
        {
            if (IsReplayAction(context))
            {
                return context.Items.Contains("BACKEND_URL") ? context.Items["BACKEND_URL"].ToString() : string.Empty;
            }

            var request = context.Request;
            return $"{request.Url.Scheme}://{request.Url.Host}:{request.Url.Port}{request.ApplicationPath}/";
        }
    }
}
