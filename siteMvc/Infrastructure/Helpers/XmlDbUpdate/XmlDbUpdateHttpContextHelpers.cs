using System;
using System.Collections;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Moq;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Security;
using Quantumart.QP8.WebMvc.Extensions.Helpers;
using Quantumart.QP8.WebMvc.Infrastructure.Constants.XmlDbUpdate;
using Quantumart.QP8.WebMvc.Infrastructure.Models;

namespace Quantumart.QP8.WebMvc.Infrastructure.Helpers.XmlDbUpdate
{
    internal static class XmlDbUpdateHttpContextHelpers
    {
        internal static XmlDbUpdateRecordedAction CreateXmlDbUpdateActionFromHttpContext(HttpContextBase httpContext, string actionCode, bool ignoreForm)
        {
            var action = new XmlDbUpdateRecordedAction
            {
                Code = actionCode,
                ParentId = BackendActionContext.Current.ParentEntityId.HasValue ? BackendActionContext.Current.ParentEntityId.Value : 0,
                Lcid = CultureInfo.CurrentCulture.LCID,
                Executed = DateTime.Now,
                ExecutedBy = (httpContext.User.Identity as QPIdentity)?.Name,
                Form = ignoreForm ? null : httpContext.Request.Form,
                Ids = httpContext.Items.Contains("FROM_ID")
                    ? new[] { httpContext.Items["FROM_ID"].ToString() }
                    : BackendActionContext.Current.Entities.Select(n => n.StringId).ToArray(),
                ResultId = GetContextData<int>(httpContext, "RESULT_ID"),
                VirtualFieldIds = GetContextData<string>(httpContext, "NEW_VIRTUAL_FIELD_IDS"),
                FieldIds = GetContextData<string>(httpContext, "FIELD_IDS"),
                LinkIds = GetContextData<string>(httpContext, "LINK_IDS"),
                NewLinkId = GetContextData<int>(httpContext, "NEW_LINK_ID"),
                BackwardId = GetContextData<int>(httpContext, "NEW_BACKWARD_ID"),
                NewChildFieldIds = GetContextData<string>(httpContext, "NEW_CHILD_FIELD_IDS"),
                NewChildLinkIds = GetContextData<string>(httpContext, "NEW_CHILD_LINK_IDS"),
                ActionId = GetContextData<int>(httpContext, "ACTION_ID"),
                ActionCode = GetContextData<string>(httpContext, "ACTION_CODE"),
                NewCommandIds = GetContextData<string>(httpContext, "NEW_COMMAND_IDS"),
                NewRulesIds = GetContextData<string>(httpContext, "NEW_RULES_IDS"),
                NotificationFormatId = GetContextData<int>(httpContext, "NOTIFICATION_FORMAT_ID"),
                DefaultFormatId = GetContextData<int>(httpContext, "DEFAULT_FORMAT_ID"),
            };

            return action;
        }

        internal static HttpContextBase PostAction(XmlDbUpdateRecordedAction recordedAction, string backendUrl, int userId)
        {
            Ensure.NotNull(QPConnectionScope.Current.DbConnection, "QPConnection scope should be initialized to use fake mvc context");
            var urlParts = recordedAction.BackendAction.ControllerActionUrl.Split(@"/".ToCharArray()).Where(n => !string.IsNullOrEmpty(n) && n != "~").ToArray();
            var controllerName = urlParts[0];
            var controllerAction = urlParts[1];
            var requestContext = new RequestContext(
                BuildHttpContextBase(recordedAction, backendUrl, userId),
                GetRouteData(recordedAction, controllerName, controllerAction)
            );

            BackendActionContext.ResetCurrent();
            BuildController(requestContext, controllerName, CultureHelpers.GetCultureInfoByLcid(recordedAction.Lcid)).Execute(requestContext);
            return requestContext.HttpContext;
        }

        private static HttpContextBase BuildHttpContextBase(XmlDbUpdateRecordedAction action, string backendUrl, int userId)
        {
            HttpContext.Current = new HttpContext(
                new HttpRequest(string.Empty, backendUrl, string.Empty),
                new HttpResponse(new StringWriter()));

            var principal = GetQpPrincipal(userId);
            var httpContext = new Mock<HttpContextBase>();
            httpContext.Setup(c => c.Request).Returns(GetHttpRequestMock(action));
            httpContext.Setup(c => c.Session).Returns(new HttpSessionMock());
            httpContext.Setup(c => c.Response).Returns(new Mock<HttpResponseBase>().Object);
            httpContext.Setup(c => c.Response.Cookies).Returns(new HttpCookieCollection());
            httpContext.Setup(c => c.Items).Returns(new Hashtable());
            httpContext.Setup(c => c.Cache).Returns(HttpRuntime.Cache);
            httpContext.Setup(c => c.User).Returns(principal);
            HttpContext.Current.User = principal;

            var result = httpContext.Object;
            AppendItems(result, backendUrl);

            return result;
        }

        private static HttpRequestMock GetHttpRequestMock(XmlDbUpdateRecordedAction action)
        {
            var httpRequest = new HttpRequestMock();
            var options = QPConnectionScope.Current.IdentityInsertOptions;
            var entityTypeCode = action.BackendAction.EntityType.Code;
            if (entityTypeCode == EntityTypeCode.VirtualContent)
            {
                entityTypeCode = EntityTypeCode.Content;
            }

            var actionTypeCode = action.BackendAction.ActionType.Code;
            httpRequest.SetForm(action.Form);

            if (action.Ids.Length > 1)
            {
                httpRequest.Form.Add("IDs", string.Join(",", action.Ids));
            }

            if (actionTypeCode == ActionTypeCode.AddNew && options.Contains(entityTypeCode))
            {
                httpRequest.Form.Add("Data.ForceId", action.Ids[0]);
            }

            switch (action.Code)
            {
                case ActionCode.AddNewContent:
                    if (options.Contains(EntityTypeCode.Field))
                    {
                        AddListItem(httpRequest.Form, "Data.ForceFieldIds", action.ChildIds);
                    }

                    if (options.Contains(EntityTypeCode.ContentLink))
                    {
                        AddListItem(httpRequest.Form, "Data.ForceLinkIds", action.ChildLinkIds);
                    }

                    break;

                case ActionCode.CreateLikeContent:
                    if (options.Contains(EntityTypeCode.Content))
                    {
                        httpRequest.Form.Add("forceId", action.ResultId.ToString());
                    }

                    if (options.Contains(EntityTypeCode.Field))
                    {
                        httpRequest.Form.Add("forceFieldIds", action.ChildIds);
                    }

                    if (options.Contains(EntityTypeCode.ContentLink))
                    {
                        httpRequest.Form.Add("forceLinkIds", action.ChildLinkIds);
                    }

                    break;

                case ActionCode.CreateLikeField:
                    if (options.Contains(EntityTypeCode.Field))
                    {
                        httpRequest.Form.Add("forceId", action.ResultId.ToString());
                    }

                    if (options.Contains(EntityTypeCode.Field))
                    {
                        httpRequest.Form.Add("forceVirtualFieldIds", action.VirtualFieldIds);
                    }

                    if (options.Contains(EntityTypeCode.Field))
                    {
                        httpRequest.Form.Add("forceChildFieldIds", action.ChildIds);
                    }

                    if (options.Contains(EntityTypeCode.ContentLink))
                    {
                        httpRequest.Form.Add("forceLinkId", action.ChildId.ToString());
                    }

                    if (options.Contains(EntityTypeCode.ContentLink))
                    {
                        httpRequest.Form.Add("forceChildLinkIds", action.ChildLinkIds);
                    }

                    break;

                case ActionCode.AddNewVirtualContents:
                case ActionCode.VirtualContentProperties:
                case ActionCode.VirtualFieldProperties:
                    if (options.Contains(EntityTypeCode.Field))
                    {
                        AddListItem(httpRequest.Form, "Data.ForceVirtualFieldIds", action.VirtualFieldIds);
                    }

                    break;

                case ActionCode.AddNewField:
                case ActionCode.FieldProperties:
                    if (options.Contains(EntityTypeCode.ContentLink))
                    {
                        httpRequest.Form.Add("Data.ContentLink.ForceLinkId", action.ChildId.ToString());
                        AddListItem(httpRequest.Form, "Data.ForceChildLinkIds", action.ChildIds);
                    }

                    if (options.Contains(EntityTypeCode.Field))
                    {
                        AddListItem(httpRequest.Form, "Data.ForceVirtualFieldIds", action.VirtualFieldIds);
                        httpRequest.Form.Add("Data.ForceBackwardId", action.BackwardId.ToString());
                        AddListItem(httpRequest.Form, "Data.ForceChildFieldIds", action.ChildIds);
                    }

                    break;

                case ActionCode.AddNewCustomAction:
                    httpRequest.Form.Add("Data.ForceActionCode", action.CustomActionCode);
                    if (options.Contains(EntityTypeCode.BackendAction))
                    {
                        httpRequest.Form.Add("Data.ForceActionId", action.ChildId.ToString());
                    }

                    break;

                case ActionCode.AddNewVisualEditorPlugin:
                case ActionCode.VisualEditorPluginProperties:
                    if (options.Contains(EntityTypeCode.VisualEditorCommand))
                    {
                        AddListItem(httpRequest.Form, "Data.ForceCommandIds", action.ChildIds);
                    }

                    break;

                case ActionCode.AddNewWorkflow:
                case ActionCode.WorkflowProperties:
                    if (options.Contains(EntityTypeCode.WorkflowRule))
                    {
                        AddListItem(httpRequest.Form, "Data.ForceRulesIds", action.ChildIds);
                    }

                    break;
            }

            httpRequest.SetPath(action.BackendAction.ControllerActionUrl.Replace("~", string.Empty));
            return httpRequest;
        }

        private static void AddListItem(NameValueCollection collection, string name, string commaList)
        {
            if (!string.IsNullOrEmpty(commaList))
            {
                foreach (var id in commaList.Split(",".ToCharArray()))
                {
                    collection.Add(name, id);
                }
            }
        }

        private static QPPrincipal GetQpPrincipal(int userId)
        {
            var user = new UserService().ReadProfile(userId);
            var identity = new QPIdentity(user.Id, user.Name, QPContext.CurrentCustomerCode, "QP", true, 1, "neutral", false);
            return new QPPrincipal(identity, new string[] { });
        }

        private static void AppendItems(HttpContextBase result, string backendUrl)
        {
            result.Items.Add(XmlDbUpdateCommonConstants.IsReplayingXmlContext, true);
            if (!string.IsNullOrWhiteSpace(backendUrl))
            {
                result.Items.Add(XmlDbUpdateCommonConstants.BackendUrlContext, backendUrl);
            }
        }

        private static RouteData GetRouteData(XmlDbUpdateRecordedAction action, string controllerName, string controllerAction)
        {
            var data = new RouteData();
            data.Values["controller"] = controllerName;
            data.Values["action"] = controllerAction;
            data.Values["tabId"] = "tab_virtual";
            data.Values["parentId"] = action.ParentId;
            if (action.Ids.Length > 0 && !string.IsNullOrEmpty(action.Ids[0]))
            {
                if (action.Ids.Length == 1)
                {
                    data.Values["id"] = int.Parse(action.Ids[0]);
                }
                else
                {
                    data.Values["IDs"] = action.Ids.Select(int.Parse).ToArray();
                }
            }

            return data;
        }

        private static IController BuildController(RequestContext requestContext, string controllerName, CultureInfo cultureInfo)
        {
            var controller = new DefaultControllerFactory().CreateController(requestContext, controllerName) as ControllerBase;
            if (controller == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Controller '{0}' is not found ", controllerName));
            }

            controller.ControllerContext = new ControllerContext(requestContext.HttpContext, requestContext.RouteData, controller);
            controller.ValueProvider = new ValueProviderCollection(new[]
            {
                new DictionaryValueProvider<object>(requestContext.RouteData.Values, cultureInfo),
                new FormCollection(requestContext.HttpContext.Request.Form).ToValueProvider(),
                new NameValueCollectionValueProvider(requestContext.HttpContext.Request.QueryString, cultureInfo)
            });

            Thread.CurrentThread.CurrentCulture = cultureInfo;
            return controller;
        }

        private static T GetContextData<T>(HttpContextBase httpContext, string key)
        {
            return httpContext.Items.Contains(key) ? (T)httpContext.Items[key] : default(T);
        }
    }
}
