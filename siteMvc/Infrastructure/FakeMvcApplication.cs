using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Web;
using System.Web.Compilation;
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
using Quantumart.QP8.WebMvc.Infrastructure.Helpers;
using Quantumart.QP8.WebMvc.Infrastructure.Models;

namespace Quantumart.QP8.WebMvc.Infrastructure
{
    public class FakeMvcApplication : IDisposable
    {
        internal FakeMvcApplication()
        {
            MvcApplication.RegisterModelBinders();
            MvcApplication.RegisterModelValidatorProviders();
            MvcApplication.RegisterMappings();
            CheatBuildManager();

            AreaRegistration.RegisterAllAreas();
            MvcApplication.RegisterUnity();
            MvcApplication.RegisterRoutes(new RouteCollection());
        }

        internal static HttpContextBase PostAction(XmlDbUpdateRecordedAction recordedAction, string backendUrl, int userId)
        {
            Ensure.NotNull(QPConnectionScope.Current, "QPConnection scope should be initialized to use fake mvc context");

            var urlParts = recordedAction.BackendAction.ControllerActionUrl.Split(@"/".ToCharArray()).Where(n => !string.IsNullOrEmpty(n) && (n != "~")).ToArray();
            var controllerName = urlParts[0];
            var controllerAction = urlParts[1];
            var requestContext = new RequestContext(
                BuildHttpContextBase(recordedAction, backendUrl, userId),
                GetRouteData(recordedAction, controllerName, controllerAction)
            );

            BuildController(requestContext, controllerName, CultureHelpers.GetCultureInfoByLcid(recordedAction.Lcid)).Execute(requestContext);
            return requestContext.HttpContext;
        }

        private static HttpContextBase BuildHttpContextBase(XmlDbUpdateRecordedAction action, string backendUrl, int userId)
        {
            HttpContext.Current = new HttpContext(
                new HttpRequest("foo", "http://tempuri.org/foo", string.Empty),
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

            if ((actionTypeCode == ActionTypeCode.AddNew) && options.Contains(entityTypeCode))
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
            if ((action.Ids.Length > 0) && !string.IsNullOrEmpty(action.Ids[0]))
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

        private static void CheatBuildManager()
        {
            var memberInfo = typeof(BuildManager).GetField("_theBuildManager", BindingFlags.NonPublic | BindingFlags.Static);
            if (memberInfo != null)
            {
                var manager = memberInfo.GetValue(null);
                typeof(BuildManager).GetProperty("PreStartInitStage", BindingFlags.NonPublic | BindingFlags.Static).SetValue(null, 2, null);

                var fieldInfo = typeof(BuildManager).GetField("_topLevelFilesCompiledStarted", BindingFlags.NonPublic | BindingFlags.Instance);
                fieldInfo?.SetValue(manager, true);

                var field = typeof(BuildManager).GetField("_topLevelReferencedAssemblies", BindingFlags.NonPublic | BindingFlags.Instance);
                field?.SetValue(manager, new List<Assembly> { typeof(MvcApplication).Assembly });
            }
        }

        public void Dispose()
        {
            MvcApplication.UnregisterModelBinders();
            MvcApplication.UnregisterModelValidatorProviders();
            MvcApplication.UnregisterRoutes();
        }
    }
}
