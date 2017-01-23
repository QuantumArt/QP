using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.WebPages;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Interfaces.Services;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Extensions.ActionResults;
using Quantumart.QP8.WebMvc.Infrastructure.Enums;
using Quantumart.QP8.WebMvc.Infrastructure.Extensions;
using Quantumart.QP8.WebMvc.ViewModels;

namespace Quantumart.QP8.WebMvc.Extensions.Controllers
{
    // ReSharper disable once InconsistentNaming
    public class QPController : Controller
    {
        protected IArticleService DbArticleService;

        public QPController()
        {
        }

        public QPController(IArticleService dbArticleService)
        {
            DbArticleService = dbArticleService;
        }

        public string RenderPartialView(string partialViewName, object model)
        {
            if (string.IsNullOrEmpty(partialViewName))
            {
                partialViewName = ControllerContext.RouteData.GetRequiredString("action");
            }

            ViewData.Model = model;
            using (var sw = new StringWriter())
            {
                var viewResult = ViewEngines.Engines.FindPartialView(ControllerContext, partialViewName);
                var viewContext = new ViewContext(ControllerContext, viewResult.View, ViewData, TempData, sw);
                viewResult.View.Render(viewContext, sw);

                return sw.GetStringBuilder().ToString();
            }
        }

        public ActionResult JsonHtml(string viewName, object model)
        {
            if (HttpContext.IsXmlDbUpdateReplayAction())
            {
                if (!ModelState.IsValid)
                {
                    throw new AggregateException(ModelState.Values
                        .SelectMany(x => x.Errors)
                        .Select(x => x.Exception ?? new ArgumentException(x.ErrorMessage)));
                }

                return null;
            }

            return new JsonNetResult<object>(new { success = true, view = RenderPartialView(viewName, model) });
        }

        public JsonCamelCaseResult<JSendResponse> JsonCamelCaseHtml(string viewName, object model = null)
        {
            if (HttpContext.IsXmlDbUpdateReplayAction())
            {
                if (!ModelState.IsValid)
                {
                    throw new AggregateException(ModelState.Values
                        .SelectMany(x => x.Errors)
                        .Select(x => x.Exception ?? new ArgumentException(x.ErrorMessage)));
                }

                return null;
            }

            return new JSendResponse
            {
                Status = JSendStatus.Success,
                Data = RenderPartialView(viewName, model)
            };
        }

        public static bool IsError(HttpContextBase context)
        {
            var form = context.Request.Form;
            var formResult = form != null && form.AllKeys.Contains("isError") && bool.Parse(form["isError"]);
            return formResult || context.Items.Contains("IS_ERROR");
        }

        public ActionResult Redirect(string actionName, object routeValues)
        {
            return HttpContext.IsXmlDbUpdateReplayAction() ? null : RedirectToAction(actionName, routeValues);
        }

        public JsonNetResult<MessageResult> JsonMessageResult(MessageResult result)
        {
            if (HttpContext.IsXmlDbUpdateReplayAction())
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

        private void PersistToHttpContext(string key, int item)
        {
            ControllerContext.HttpContext.Items[key] = item;
        }

        private void PersistToHttpContext(string key, string item)
        {
            ControllerContext.HttpContext.Items[key] = item;
        }

        private void PersistToHttpContext<T>(string key, IReadOnlyCollection<T> items)
        {
            if (items != null && items.Count > 0)
            {
                PersistToHttpContext(key, string.Join(",", items));
            }
        }

        public void PersistFromId(int id)
        {
            PersistToHttpContext("FROM_ID", id);
        }

        public void PersistFromId(int id, Guid guid)
        {
            PersistToHttpContext("FROM_ID", id);
            PersistToHttpContext("FROM_GUID", guid.ToString());
        }

        public void PersistFromIds(int[] ids)
        {
            PersistToHttpContext("FROM_ID", ids);
        }

        public void PersistFromIds(int[] ids, Guid[] guids)
        {
            PersistToHttpContext("FROM_ID", ids);
            PersistToHttpContext("FROM_GUID", guids);
        }

        public void PersistResultId(int id)
        {
            BackendActionContext.Current.ResetEntityId(id);
            PersistToHttpContext("RESULT_ID", id);
        }

        public void PersistResultId(int id, Guid guid)
        {
            PersistResultId(id);
            PersistToHttpContext("RESULT_GUID", guid.ToString());
        }

        public void PersistActionCode(string name)
        {
            PersistToHttpContext("ACTION_CODE", name);
        }

        public void PersistLinkId(int? oldLinkId, int? newLinkId)
        {
            if (newLinkId.HasValue && newLinkId.Value > 0 && (!oldLinkId.HasValue || oldLinkId.Value != newLinkId.Value))
            {
                PersistToHttpContext("NEW_LINK_ID", newLinkId.Value);
            }
        }

        public void PersistActionId(int id)
        {
            PersistToHttpContext("ACTION_ID", id);
        }

        public void PersistDefaultFormatId(int? defaultFormatId)
        {
            if (defaultFormatId.HasValue)
            {
                PersistToHttpContext("DEFAULT_FORMAT_ID", defaultFormatId.Value);
            }
        }

        public void PersistBackwardId(Field oldBackward, Field newBackward)
        {
            if (newBackward != null && newBackward.Id > 0 && (oldBackward == null || oldBackward.Id == 0))
            {
                PersistToHttpContext("NEW_BACKWARD_ID", newBackward.Id);
            }
        }

        public void PersistFieldIds(int[] ids)
        {
            PersistToHttpContext("FIELD_IDS", ids);
        }

        public void PersistLinkIds(int[] ids)
        {
            PersistToHttpContext("LINK_IDS", ids);
        }

        public void PersistVirtualFieldIds(int[] ids)
        {
            PersistToHttpContext("NEW_VIRTUAL_FIELD_IDS", ids);
        }

        public void PersistChildFieldIds(int[] ids)
        {
            PersistToHttpContext("NEW_CHILD_FIELD_IDS", ids);
        }

        public void PersistChildLinkIds(int[] ids)
        {
            PersistToHttpContext("NEW_CHILD_LINK_IDS", ids);
        }

        public void PersistCommandIds(int[] oldIds, int[] ids)
        {
            PersistToHttpContext("NEW_COMMAND_IDS", (oldIds != null ? ids.Except(oldIds) : ids).ToList());
        }

        public void PersistRulesIds(int[] oldIds, int[] ids)
        {
            PersistToHttpContext("NEW_RULES_IDS", (oldIds != null ? ids.Except(oldIds) : ids).ToList());
        }

        public void PersistNotificationFormatId(int? formatId)
        {
            if (formatId.HasValue)
            {
                PersistToHttpContext("NOTIFICATION_FORMAT_ID", formatId.Value);
            }
        }

        protected void AppendFormGuidsFromIds(string formIdsKey, string formUniqueIdsKey)
        {
            var formIds = HttpContext.Request.Form[formIdsKey]?.Split(',');
            if (formIds != null)
            {
                var substitutedGuids = formIds
                    .Where(f => f.IsInt())
                    .Select(DbArticleService.GetArticleGuidById)
                    .Where(g => g != Guid.Empty)
                    .Select(g => g.ToString())
                    .ToArray();

                HttpContext.Items.Add(formUniqueIdsKey, substitutedGuids);
            }
        }
    }
}
