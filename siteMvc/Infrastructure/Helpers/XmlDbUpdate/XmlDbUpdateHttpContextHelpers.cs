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
using Quantumart.QP8.Constants.Mvc;
using Quantumart.QP8.Security;
using Quantumart.QP8.WebMvc.Extensions.Helpers;
using Quantumart.QP8.WebMvc.Infrastructure.Constants;
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
                ExecutedBy = (httpContext.User.Identity as QpIdentity)?.Name,
                Ids = httpContext.Items.Contains(HttpContextItems.FromId)
                    ? new[] { httpContext.Items[HttpContextItems.FromId].ToString() }
                    : BackendActionContext.Current.Entities.Select(n => n.StringId).ToArray(),
                ResultId = GetContextData<int>(httpContext, HttpContextItems.ResultId),
                UniqueId = GetGuidsContextData(httpContext, HttpContextItems.FromGuid),
                ResultUniqueId = GetGuidContextData(httpContext, HttpContextItems.ResultGuid),
                VirtualFieldIds = GetContextData<string>(httpContext, HttpContextItems.NewVirtualFieldIds),
                FieldIds = GetContextData<string>(httpContext, HttpContextItems.FieldIds),
                LinkIds = GetContextData<string>(httpContext, HttpContextItems.LinkIds),
                NewLinkId = GetContextData<int>(httpContext, HttpContextItems.NewLinkId),
                BackwardId = GetContextData<int>(httpContext, HttpContextItems.NewBackwardId),
                NewChildFieldIds = GetContextData<string>(httpContext, HttpContextItems.NewChildFieldIds),
                NewChildLinkIds = GetContextData<string>(httpContext, HttpContextItems.NewChildLinkIds),
                ActionId = GetContextData<int>(httpContext, HttpContextItems.ActionId),
                ActionCode = GetContextData<string>(httpContext, HttpContextItems.ActionCode),
                NewCommandIds = GetContextData<string>(httpContext, HttpContextItems.NewCommandIds),
                NewRulesIds = GetContextData<string>(httpContext, HttpContextItems.NewRulesIds),
                NotificationFormatId = GetContextData<int>(httpContext, HttpContextItems.NotificationFormatId),
                DefaultFormatId = GetContextData<int>(httpContext, HttpContextItems.DefaultFormatId)
            };

            if (!ignoreForm)
            {
                action.Form = new NameValueCollection
                {
                    httpContext.Request.Form,
                    GetDynamicFieldValuesFromHttpContext(httpContext, HttpContextItems.FieldUniqueIdPrefix),
                    GetStringValuesFromHttpContext(httpContext, HttpContextItems.DefaultArticleUniqueIds),
                    GetStringValuesFromHttpContext(httpContext, HttpContextItems.DataO2MUniqueIdDefaultValue),
                    GetStringValuesFromHttpContext(httpContext, HttpContextItems.ContentDefaultFilterArticleUniqueIDs)
                };
            }

            return action;
        }

        internal static RouteData GetRouteData(XmlDbUpdateRecordedAction action, string controllerName, string controllerAction)
        {
            var data = new RouteData();
            data.Values[HttpRouteData.Id] = int.Parse(action.Ids.First());
            data.Values[HttpRouteData.Ids] = action.Ids.Select(int.Parse).ToArray();
            data.Values[HttpRouteData.TabId] = "tab_virtual";
            data.Values[HttpRouteData.Action] = controllerAction;
            data.Values[HttpRouteData.ParentId] = action.ParentId;
            data.Values[HttpRouteData.Controller] = controllerName;
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

            return useGuidSubstitution
                ? AddGlobalHttpContextVariables(httpContext.Object, backendUrl, action.ResultUniqueId)
                : AddGlobalHttpContextVariables(httpContext.Object, backendUrl);
        }

        private static HttpContextBase AddGlobalHttpContextVariables(HttpContextBase httpContext, string backendUrl)
        {
            httpContext.Items.Add(HttpContextItems.IsReplayingXmlContext, true);
            if (!string.IsNullOrWhiteSpace(backendUrl))
            {
                httpContext.Items.Add(HttpContextItems.BackendUrlContext, backendUrl);
            }

            return httpContext;
        }

        private static HttpContextBase AddGlobalHttpContextVariables(HttpContextBase httpContext, string backendUrl, Guid resultUniqueId)
        {
            httpContext = AddGlobalHttpContextVariables(httpContext, backendUrl);
            httpContext.Items.Add(HttpContextItems.XmlContextGuidSubstitution, resultUniqueId);
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
            httpRequest.Form.Add(HttpContextFormConstants.Ids, string.Join(",", action.Ids));

            if (actionTypeCode == ActionTypeCode.AddNew && options.Contains(entityTypeCode))
            {
                httpRequest.Form.Add(HttpContextFormConstants.DataForceId, action.Ids.First());
            }

            switch (action.Code)
            {
                case ActionCode.AddNewContent:
                    if (options.Contains(EntityTypeCode.Field))
                    {
                        AddListItem(httpRequest.Form, HttpContextFormConstants.DataForceFieldIds, action.ChildIds);
                    }

                    if (options.Contains(EntityTypeCode.ContentLink))
                    {
                        AddListItem(httpRequest.Form, HttpContextFormConstants.DataForceLinkIds, action.ChildLinkIds);
                    }

                    break;

                case ActionCode.CreateLikeContent:
                    if (options.Contains(EntityTypeCode.Content))
                    {
                        httpRequest.Form.Add(HttpContextFormConstants.ForceId, action.ResultId.ToString());
                    }

                    if (options.Contains(EntityTypeCode.Field))
                    {
                        httpRequest.Form.Add(HttpContextFormConstants.ForceFieldIds, action.ChildIds);
                    }

                    if (options.Contains(EntityTypeCode.ContentLink))
                    {
                        httpRequest.Form.Add(HttpContextFormConstants.ForceLinkIds, action.ChildLinkIds);
                    }

                    break;

                case ActionCode.CreateLikeField:
                    if (options.Contains(EntityTypeCode.Field))
                    {
                        httpRequest.Form.Add(HttpContextFormConstants.ForceId, action.ResultId.ToString());
                    }

                    if (options.Contains(EntityTypeCode.Field))
                    {
                        httpRequest.Form.Add(HttpContextFormConstants.ForceVirtualFieldIds, action.VirtualFieldIds);
                    }

                    if (options.Contains(EntityTypeCode.Field))
                    {
                        httpRequest.Form.Add(HttpContextFormConstants.ForceChildFieldIds, action.ChildIds);
                    }

                    if (options.Contains(EntityTypeCode.ContentLink))
                    {
                        httpRequest.Form.Add(HttpContextFormConstants.ForceLinkId, action.ChildId.ToString());
                    }

                    if (options.Contains(EntityTypeCode.ContentLink))
                    {
                        httpRequest.Form.Add(HttpContextFormConstants.ForceChildLinkIds, action.ChildLinkIds);
                    }

                    break;

                case ActionCode.AddNewVirtualContents:
                case ActionCode.VirtualContentProperties:
                case ActionCode.VirtualFieldProperties:
                    if (options.Contains(EntityTypeCode.Field))
                    {
                        AddListItem(httpRequest.Form, HttpContextFormConstants.DataForceVirtualFieldIds, action.VirtualFieldIds);
                    }

                    break;

                case ActionCode.AddNewField:
                case ActionCode.FieldProperties:
                    if (options.Contains(EntityTypeCode.ContentLink))
                    {
                        httpRequest.Form.Add(HttpContextFormConstants.DataContentLinkForceLinkId, action.ChildId.ToString());
                        AddListItem(httpRequest.Form, HttpContextFormConstants.DataForceChildLinkIds, action.ChildIds);
                    }

                    if (options.Contains(EntityTypeCode.Field))
                    {
                        AddListItem(httpRequest.Form, HttpContextFormConstants.DataForceVirtualFieldIds, action.VirtualFieldIds);
                        httpRequest.Form.Add(HttpContextFormConstants.DataForceBackwardId, action.BackwardId.ToString());
                        AddListItem(httpRequest.Form, HttpContextFormConstants.DataForceChildFieldIds, action.ChildIds);
                    }

                    break;

                case ActionCode.AddNewCustomAction:
                    httpRequest.Form.Add(HttpContextFormConstants.DataForceActionCode, action.CustomActionCode);
                    if (options.Contains(EntityTypeCode.BackendAction))
                    {
                        httpRequest.Form.Add(HttpContextFormConstants.DataForceActionId, action.ChildId.ToString());
                    }

                    break;

                case ActionCode.AddNewVisualEditorPlugin:
                case ActionCode.VisualEditorPluginProperties:
                    if (options.Contains(EntityTypeCode.VisualEditorCommand))
                    {
                        AddListItem(httpRequest.Form, HttpContextFormConstants.DataForceCommandIds, action.ChildIds);
                    }

                    break;

                case ActionCode.AddNewWorkflow:
                case ActionCode.WorkflowProperties:
                    if (options.Contains(EntityTypeCode.WorkflowRule))
                    {
                        AddListItem(httpRequest.Form, HttpContextFormConstants.DataForceRulesIds, action.ChildIds);
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

        private static QpPrincipal GetQpPrincipal(int userId)
        {
            var user = new UserService().ReadProfile(userId);
            var identity = new QpIdentity(user.Id, user.Name, QPContext.CurrentCustomerCode, "QP", true, 1, "neutral", false);
            return new QpPrincipal(identity, new string[] { });
        }

        private static T GetContextData<T>(HttpContextBase httpContext, string key) => httpContext.Items.Contains(key) ? (T)httpContext.Items[key] : default(T);

        private static Guid GetGuidContextData(HttpContextBase httpContext, string key) => httpContext.Items.Contains(key) ? Guid.Parse(httpContext.Items[key].ToString()) : Guid.Empty;

        private static Guid[] GetGuidsContextData(HttpContextBase httpContext, string key) => httpContext.Items.Contains(key) ? httpContext.Items[key].ToString().Split(",".ToCharArray()).Select(Guid.Parse).ToArray() : new Guid[] { };

        private static NameValueCollection GetDynamicFieldValuesFromHttpContext(HttpContextBase httpContext, string fieldPrefix)
        {
            var result = new NameValueCollection();
            foreach (var ctxKey in httpContext.Items.Keys.OfType<string>().Where(key => key.StartsWith(fieldPrefix)))
            {
                result.Add(GetStringValuesFromHttpContext(httpContext, ctxKey));
            }

            return result;
        }

        private static NameValueCollection GetStringValuesFromHttpContext(HttpContextBase httpContext, string key)
        {
            var result = new NameValueCollection();
            if (httpContext.Items.Contains(key))
            {
                foreach (var value in (string[])httpContext.Items[key])
                {
                    result.Add(key, value);
                }
            }

            return result;
        }
    }
}
