using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.WebPages;
using QP8.Infrastructure.Logging;
using QP8.Infrastructure.Web.ActionResults;
using QP8.Infrastructure.Web.Enums;
using QP8.Infrastructure.Web.Responses;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Interfaces.Services;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Configuration;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Constants.Mvc;
using Quantumart.QP8.WebMvc.Infrastructure.Constants;
using Quantumart.QP8.WebMvc.Infrastructure.Extensions;

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

        protected override void Initialize(RequestContext requestContext)
        {
            base.Initialize(requestContext);
            Logger.Log.SetContext(LoggerData.CustomerCodeCustomVariable, QPContext.CurrentCustomerCode ?? string.Empty);
        }

        public string RenderPartialView(string partialViewName, object model)
        {
            if (string.IsNullOrEmpty(partialViewName))
            {
                partialViewName = ControllerContext.RouteData.GetRequiredString(HttpRouteData.Action);
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
            var formResult = form != null && form.AllKeys.Contains(HttpContextFormConstants.IsError) && bool.Parse(form[HttpContextFormConstants.IsError]);
            return formResult || context.Items.Contains(HttpContextItems.IsError);
        }

        public ActionResult Redirect(string actionName, object routeValues) => HttpContext.IsXmlDbUpdateReplayAction() ? null : RedirectToAction(actionName, routeValues);

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
                ControllerContext.RequestContext.HttpContext.Items.Add(HttpContextItems.IsError, true);
            }

            return result;
        }

        public JsonResult JsonError(string msg) => new JsonNetResult<object>(new { success = false, message = msg });

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
            PersistToHttpContext(HttpContextItems.FromId, id);
        }

        public void PersistFromId(int id, Guid guid)
        {
            PersistToHttpContext(HttpContextItems.FromId, id);
            PersistToHttpContext(HttpContextItems.FromGuid, guid.ToString());
        }

        public void PersistFromIds(int[] ids)
        {
            PersistToHttpContext(HttpContextItems.FromId, ids);
        }

        public void PersistFromIds(int[] ids, Guid[] guids)
        {
            PersistToHttpContext(HttpContextItems.FromId, ids);
            PersistToHttpContext(HttpContextItems.FromGuid, guids);
        }

        public void PersistResultId(int id)
        {
            BackendActionContext.Current.ResetEntityId(id);
            PersistToHttpContext(HttpContextItems.ResultId, id);
        }

        public void PersistResultId(int id, Guid guid)
        {
            PersistResultId(id);
            PersistToHttpContext(HttpContextItems.ResultGuid, guid.ToString());
        }

        public void PersistActionCode(string name)
        {
            PersistToHttpContext(HttpContextItems.ActionCode, name);
        }

        public void PersistLinkId(int? oldLinkId, int? newLinkId)
        {
            if (newLinkId.HasValue && newLinkId.Value > 0 && (!oldLinkId.HasValue || oldLinkId.Value != newLinkId.Value))
            {
                PersistToHttpContext(HttpContextItems.NewLinkId, newLinkId.Value);
            }
        }

        public void PersistActionId(int id)
        {
            PersistToHttpContext(HttpContextItems.ActionId, id);
        }

        public void PersistDefaultFormatId(int? defaultFormatId)
        {
            if (defaultFormatId.HasValue)
            {
                PersistToHttpContext(HttpContextItems.DefaultFormatId, defaultFormatId.Value);
            }
        }

        public void PersistBackwardId(Field oldBackward, Field newBackward)
        {
            if (newBackward != null && newBackward.Id > 0 && (oldBackward == null || oldBackward.Id == 0))
            {
                PersistToHttpContext(HttpContextItems.NewBackwardId, newBackward.Id);
            }
        }

        public void PersistFieldIds(int[] ids)
        {
            PersistToHttpContext(HttpContextItems.FieldIds, ids);
        }

        public void PersistLinkIds(int[] ids)
        {
            PersistToHttpContext(HttpContextItems.LinkIds, ids);
        }

        public void PersistVirtualFieldIds(int[] ids)
        {
            PersistToHttpContext(HttpContextItems.NewVirtualFieldIds, ids);
        }

        public void PersistChildFieldIds(int[] ids)
        {
            PersistToHttpContext(HttpContextItems.NewChildFieldIds, ids);
        }

        public void PersistChildLinkIds(int[] ids)
        {
            PersistToHttpContext(HttpContextItems.NewChildLinkIds, ids);
        }

        public void PersistCommandIds(int[] oldIds, int[] ids)
        {
            PersistToHttpContext(HttpContextItems.NewCommandIds, (oldIds != null ? ids.Except(oldIds) : ids).ToList());
        }

        public void PersistRulesIds(int[] oldIds, int[] ids)
        {
            PersistToHttpContext(HttpContextItems.NewRulesIds, (oldIds != null ? ids.Except(oldIds) : ids).ToList());
        }

        public void PersistNotificationFormatId(int? formatId)
        {
            if (formatId.HasValue)
            {
                PersistToHttpContext(HttpContextItems.NotificationFormatId, formatId.Value);
            }
        }

        protected void AppendFormGuidsFromIds(string formIdsKey, string formUniqueIdsKey)
        {
            var formIds = HttpContext.Request.Form[formIdsKey]?.Split(',');
            if (formIds != null)
            {
                var validatedFormIds = formIds
                    .Where(g => g.IsInt())
                    .Select(int.Parse)
                    .ToArray();

                if (validatedFormIds.Any() && validatedFormIds.Length <= (QPConfiguration.WebConfigSection?.RelationCountLimit ?? Default.RelationCountLimit))
                {
                    var substitutedGuids = DbArticleService.GetArticleGuidsByIds(validatedFormIds)
                        .Where(g => g != Guid.Empty)
                        .Select(g => g.ToString())
                        .ToArray();

                    HttpContext.Items.Add(formUniqueIdsKey, substitutedGuids);
                }
            }
        }
    }
}
