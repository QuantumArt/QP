using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Extensions.ActionResults;
using Quantumart.QP8.WebMvc.Infrastructure.Helpers;

namespace Quantumart.QP8.WebMvc.Extensions.Controllers
{
    // ReSharper disable once InconsistentNaming
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
            if (CommonHelpers.IsXmlDbUpdateReplayAction(HttpContext))
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

        public static bool IsError(HttpContextBase context)
        {
            var form = context.Request.Form;
            var formResult = form != null && form.AllKeys.Contains("isError") && bool.Parse(form["isError"]);
            return formResult || context.Items.Contains("IS_ERROR");
        }

        public ActionResult Redirect(string actionName, object routeValues)
        {
            return IsReplayAction() ? null : RedirectToAction(actionName, routeValues);
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

        public JsonResult JsonError(string msg)
        {
            return new JsonNetResult<object>(new { success = false, message = msg });
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

        private void PersistIds(string key, IReadOnlyCollection<int> ids)
        {
            if (ids != null && ids.Count > 0)
            {
                ControllerContext.HttpContext.Items[key] = string.Join(",", ids);
            }
        }

        private void PersistIds(string key, int[] oldIds, IReadOnlyCollection<int> ids)
        {
            if (ids != null && ids.Count > 0)
            {
                ControllerContext.HttpContext.Items[key] = string.Join(",", oldIds != null ? ids.Except(oldIds) : ids);
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

        // TODO: fix referenced code
        public bool IsReplayAction()
        {
            return CommonHelpers.IsXmlDbUpdateReplayAction(HttpContext);
        }
    }
}
