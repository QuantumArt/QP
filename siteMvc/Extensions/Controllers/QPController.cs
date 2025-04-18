using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using NLog;
using QP8.Infrastructure.Web.Enums;
using QP8.Infrastructure.Web.Responses;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services.ArticleServices;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Configuration;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Constants.Mvc;
using Quantumart.QP8.WebMvc.Extensions.Helpers;
using Quantumart.QP8.WebMvc.Infrastructure.Constants;
using Quantumart.QP8.WebMvc.Infrastructure.Extensions;
using Quantumart.QP8.WebMvc.Infrastructure.Settings;

namespace Quantumart.QP8.WebMvc.Extensions.Controllers
{
    public abstract class QPController : Controller
    {
        protected IArticleService ArticleService;

        protected QPublishingOptions Options;

        protected QPController()
        {
        }

        protected QPController(IArticleService articleService, QPublishingOptions options)
            : this()
        {
            ArticleService = articleService;
            Options = options;
        }

        protected JsonResult JsonCamelCase(object data)
        {
            return Json(data, JsonSettingsRegistry.CamelCaseSettings);
        }

        protected JsonResult JsonMicrosoftDate(object data)
        {
            return Json(data, JsonSettingsRegistry.MicrosoftDateSettings);
        }

        protected async Task<string> RenderPartialView(string viewName, object model)
        {
            if (string.IsNullOrEmpty(viewName))
            {
                viewName = ControllerContext.ActionDescriptor.ActionName;
            }

            ViewData.Model = model;

            var viewEngine = HttpContext.RequestServices.GetRequiredService<ICompositeViewEngine>();

            var viewEngineResult = viewEngine.FindView(ControllerContext, viewName, false);

            using (var writer = new StringWriter())
            {
                var viewContext = new ViewContext(
                    ControllerContext,
                    viewEngineResult.View,
                    ViewData,
                    TempData,
                    writer,
                    new HtmlHelperOptions());

                await viewEngineResult.View.RenderAsync(viewContext);

                return writer.GetStringBuilder().ToString();
            }
        }

        protected async Task<JsonResult> JsonHtmlEscaped(string viewName, object model)
        {
            if (HttpContext.IsXmlDbUpdateReplayAction())
            {
                if (!ModelState.IsValid)
                {
                    var exceptions = ModelState.Values
                        .SelectMany(x => x.Errors)
                        .Select(x => x.Exception ?? new ArgumentException(x.ErrorMessage));

                    throw new AggregateException(exceptions);
                }

                return Json(null);
            }

            string view = await RenderPartialView(viewName, model);

            view = view.Replace("\"", "\\\"").Replace("\r\n", "");

            return Json(new { success = true, view });
        }

        protected async Task<JsonResult> JsonHtml(string viewName, object model)
        {
            if (HttpContext.IsXmlDbUpdateReplayAction())
            {
                if (!ModelState.IsValid)
                {
                    var exceptions = ModelState.Values
                        .SelectMany(x => x.Errors)
                        .Select(x => x.Exception ?? new ArgumentException(x.ErrorMessage));

                    throw new AggregateException(exceptions);
                }

                return Json(null);
            }

            string view = await RenderPartialView(viewName, model);

            return Json(new { success = true, view });
        }

        protected async Task<JsonResult> JsonCamelCaseHtml(string viewName, object model = null)
        {
            if (HttpContext.IsXmlDbUpdateReplayAction())
            {
                if (!ModelState.IsValid)
                {
                    var exceptions = ModelState.Values
                       .SelectMany(x => x.Errors)
                       .Select(x => x.Exception ?? new ArgumentException(x.ErrorMessage));

                    throw new AggregateException(exceptions);
                }

                return Json(null);
            }

            string view = await RenderPartialView(viewName, model);

            return JsonCamelCase(new JSendResponse
            {
                Status = JSendStatus.Success,
                Data = view
            });
        }

        public static bool IsError(HttpContext context)
        {
            if (!context.Request.HasFormContentType)
            {
                return false;
            }

            var form = context.Request.Form;
            bool formHasError = form != null &&
                form.TryGetValue(HttpContextFormConstants.IsError, out var formValue) &&
                bool.Parse(formValue);

            return formHasError || context.Items.ContainsKey(HttpContextItems.IsError);
        }

        protected ActionResult Redirect(string actionName, object routeValues)
        {
            return HttpContext.IsXmlDbUpdateReplayAction() ? (ActionResult)new JsonResult(null) : RedirectToAction(actionName, routeValues);
        }

        protected JsonResult JsonMessageResult(MessageResult result)
        {
            if (HttpContext.IsXmlDbUpdateReplayAction())
            {
                if (result != null && result.Type == ActionMessageType.Error)
                {
                    throw new AggregateException(new InvalidOperationException(result.Text));
                }

                return Json(null);
            }

            if (result != null && result.Type == ActionMessageType.Error)
            {
                HttpContext.Items.Add(HttpContextItems.IsError, true);
            }

            return Json(result);
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

        protected void PersistFromId(int id)
        {
            PersistToHttpContext(HttpContextItems.FromId, id);
        }

        protected void PersistFromId(int id, Guid guid)
        {
            PersistToHttpContext(HttpContextItems.FromId, id);
            PersistToHttpContext(HttpContextItems.FromGuid, guid.ToString());
        }

        protected void PersistFromIds(int[] ids)
        {
            PersistToHttpContext(HttpContextItems.FromId, ids);
        }

        protected void PersistFromIds(int[] ids, Guid[] guids)
        {
            PersistToHttpContext(HttpContextItems.FromId, ids);
            PersistToHttpContext(HttpContextItems.FromGuid, guids);
        }

        protected void PersistResultId(int id)
        {
            BackendActionContext.Current.ResetEntityId(id);
            PersistToHttpContext(HttpContextItems.ResultId, id);
        }

        protected void PersistResultId(int id, Guid guid)
        {
            PersistResultId(id);
            PersistToHttpContext(HttpContextItems.ResultGuid, guid.ToString());
        }

        protected void PersistActionCode(string name)
        {
            PersistToHttpContext(HttpContextItems.ActionCode, name);
        }

        protected void PersistLinkId(int? oldLinkId, int? newLinkId)
        {
            if (newLinkId.HasValue && newLinkId.Value > 0 && (!oldLinkId.HasValue || oldLinkId.Value != newLinkId.Value))
            {
                PersistToHttpContext(HttpContextItems.NewLinkId, newLinkId.Value);
            }
        }

        protected void PersistActionId(int id)
        {
            PersistToHttpContext(HttpContextItems.ActionId, id);
        }

        protected void PersistDefaultFormatId(int? defaultFormatId)
        {
            if (defaultFormatId.HasValue)
            {
                PersistToHttpContext(HttpContextItems.DefaultFormatId, defaultFormatId.Value);
            }
        }

        protected void PersistBackwardId(Field oldBackward, Field newBackward)
        {
            if (newBackward != null && newBackward.Id > 0 && (oldBackward == null || oldBackward.Id == 0))
            {
                PersistToHttpContext(HttpContextItems.NewBackwardId, newBackward.Id);
            }
        }

        protected void PersistFieldIds(int[] ids)
        {
            PersistToHttpContext(HttpContextItems.FieldIds, ids);
        }

        protected void PersistLinkIds(int[] ids)
        {
            PersistToHttpContext(HttpContextItems.LinkIds, ids);
        }

        protected void PersistVirtualFieldIds(int[] ids)
        {
            PersistToHttpContext(HttpContextItems.NewVirtualFieldIds, ids);
        }

        protected void PersistChildFieldIds(int[] ids)
        {
            PersistToHttpContext(HttpContextItems.NewChildFieldIds, ids);
        }

        protected void PersistChildLinkIds(int[] ids)
        {
            PersistToHttpContext(HttpContextItems.NewChildLinkIds, ids);
        }

        protected void PersistCommandIds(int[] oldIds, int[] ids)
        {
            PersistToHttpContext(HttpContextItems.NewCommandIds, (oldIds != null ? ids.Except(oldIds) : ids).ToList());
        }

        protected void PersistRulesIds(int[] oldIds, int[] ids)
        {
            PersistToHttpContext(HttpContextItems.NewRulesIds, (oldIds != null ? ids.Except(oldIds) : ids).ToList());
        }

        protected void PersistNotificationFormatId(int? formatId)
        {
            if (formatId.HasValue)
            {
                PersistToHttpContext(HttpContextItems.NotificationFormatId, formatId.Value);
            }
        }

        protected void PersistUserAndGroupIds(int? userId, int? groupId)
        {
            if (userId.HasValue)
            {
                PersistToHttpContext(HttpContextItems.UserId, userId.Value);
            }

            if (groupId.HasValue)
            {
                PersistToHttpContext(HttpContextItems.GroupId, groupId.Value);
            }
        }

        protected void AppendFormGuidsFromIds(string formIdsKey, string formUniqueIdsKey)
        {
            string formIdsValue = HttpContext.Request.Form[formIdsKey].ToString();

            int[] validatedFormIds = formIdsValue
                .Split(',')
                .Where(str => int.TryParse(str, out int _))
                .Select(int.Parse)
                .ToArray();

            if (validatedFormIds.Length > 0 &&
                validatedFormIds.Length <= Options.RelationCountLimit)
            {
                var substitutedGuids = ArticleService.GetArticleGuidsByIds(validatedFormIds)
                    .Where(g => g != Guid.Empty)
                    .Select(g => g.ToString())
                    .ToArray();

                HttpContext.Items.Add(formUniqueIdsKey, new StringValues(substitutedGuids));
            }
        }

        protected static ListCommand GetListCommand(int page, int pageSize, string orderBy)
        {
            return new ListCommand
            {
                StartPage = page,
                PageSize = pageSize,
                SortExpression = GridExtensions.ToSqlSortExpression(orderBy ?? "")
            };
        }
    }
}
