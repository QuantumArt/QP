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
                UniqueId = GetGuidsContextData(httpContext, "FROM_GUID"),
                ResultUniqueId = GetGuidContextData(httpContext, "RESULT_GUID"),
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

        internal static RouteData GetRouteData(XmlDbUpdateRecordedAction action, string controllerName, string controllerAction)
        {
            var data = new RouteData();
            data.Values["controller"] = controllerName;
            data.Values["action"] = controllerAction;
            data.Values["tabId"] = "tab_virtual";
            data.Values["parentId"] = action.ParentId;
            data.Values["id"] = int.Parse(action.Ids.First());
            data.Values["IDs"] = action.Ids.Select(int.Parse).ToArray();
            return data;
        }

        internal static IController BuildController(RequestContext requestContext, string controllerName, CultureInfo cultureInfo)
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

        internal static HttpContextBase BuildHttpContextBase(XmlDbUpdateRecordedAction action, string backendUrl, int userId, bool useGuidSubstitution)
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

            if (useGuidSubstitution)
            {
                return AddGlobalHttpContextVariables(httpContext.Object, backendUrl, action.ResultUniqueId);
            }

            return AddGlobalHttpContextVariables(httpContext.Object, backendUrl);
        }

        private static HttpContextBase AddGlobalHttpContextVariables(HttpContextBase httpContext, string backendUrl)
        {
            httpContext.Items.Add(XmlDbUpdateCommonConstants.IsReplayingXmlContext, true);
            if (!string.IsNullOrWhiteSpace(backendUrl))
            {
                httpContext.Items.Add(XmlDbUpdateCommonConstants.BackendUrlContext, backendUrl);
            }

            return httpContext;
        }

        private static HttpContextBase AddGlobalHttpContextVariables(HttpContextBase httpContext, string backendUrl, Guid resultUniqueId)
        {
            httpContext = AddGlobalHttpContextVariables(httpContext, backendUrl);
            httpContext.Items.Add(XmlDbUpdateCommonConstants.XmlContextGuidSubstitution, resultUniqueId);
            return httpContext;
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
            httpRequest.Form.Add("IDs", string.Join(",", action.Ids));

            if (actionTypeCode == ActionTypeCode.AddNew && options.Contains(entityTypeCode))
            {
                httpRequest.Form.Add("Data.ForceId", action.Ids.First());
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

        private static T GetContextData<T>(HttpContextBase httpContext, string key)
        {
            return httpContext.Items.Contains(key) ? (T)httpContext.Items[key] : default(T);
        }

        private static Guid GetGuidContextData(HttpContextBase httpContext, string key)
        {
            return httpContext.Items.Contains(key) ? Guid.Parse(httpContext.Items[key].ToString()) : Guid.Empty;
        }

        private static Guid[] GetGuidsContextData(HttpContextBase httpContext, string key)
        {
            return httpContext.Items.Contains(key) ? httpContext.Items[key].ToString().Split(",".ToCharArray()).Select(Guid.Parse).ToArray() : new Guid[] { };
        }
    }
}
