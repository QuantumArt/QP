using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.Constants.Mvc;
using Quantumart.QP8.Security;
using Quantumart.QP8.WebMvc.Infrastructure.Models;
using static Quantumart.QP8.BLL.BackendActionContext;
using Microsoft.Extensions.Primitives;
using QP8.Infrastructure.Extensions;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Infrastructure.Constants;
using HttpContext = Microsoft.AspNetCore.Http.HttpContext;

namespace Quantumart.QP8.WebMvc.Infrastructure.Helpers.XmlDbUpdate
{
    internal static class XmlDbUpdateHttpContextHelpers
    {
        internal static XmlDbUpdateRecordedAction CreateXmlDbUpdateActionFromHttpContext(HttpContext httpContext, string actionCode, bool ignoreForm)
        {
            var action = new XmlDbUpdateRecordedAction
            {
                Code = actionCode,
                ParentId = Current.ParentEntityId ?? 0,
                Lcid = CultureInfo.CurrentCulture.LCID,
                Executed = DateTime.Now,
                ExecutedBy = QPContext.CurrentUserName,
                Ids = httpContext.Items.ContainsKey(HttpContextItems.FromId)
                    ? new[] { httpContext.Items[HttpContextItems.FromId].ToString() }
                    : Current.Entities.Select(n => n.StringId).ToArray(),
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
                DefaultFormatId = GetContextData<int>(httpContext, HttpContextItems.DefaultFormatId),
                UserId = GetContextData<int>(httpContext, HttpContextItems.UserId),
                GroupId = GetContextData<int>(httpContext, HttpContextItems.GroupId),
            };

            if (!ignoreForm && httpContext.Request.HasFormContentType)
            {
                var allValues = new Dictionary<string, StringValues>();
                allValues.AddRange(httpContext.Request.Form);
                allValues.AddRange(GetDynamicFieldValuesFromHttpContext(httpContext, HttpContextItems.FieldUniqueIdPrefix));
                allValues.AddRange(GetStringValuesFromHttpContext(httpContext, HttpContextItems.DefaultArticleUniqueIds));
                allValues.AddRange(GetStringValuesFromHttpContext(httpContext, HttpContextItems.DataO2MUniqueIdDefaultValue));
                allValues.AddRange(GetStringValuesFromHttpContext(httpContext, HttpContextItems.ContentDefaultFilterArticleUniqueIDs));
                action.Form = allValues;
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

        internal static HttpContext BuildHttpContext(XmlDbUpdateRecordedAction action, string backendUrl, int userId, bool useGuidSubstitution)
        {
            var httpContext = new DefaultHttpContext();
            FillHttpRequest(httpContext.Request, action);
            httpContext.User = GetPrincipal(userId);
            var guid = useGuidSubstitution ? action.ResultUniqueId : (Guid?)null;
            AddGlobalHttpContextVariables(httpContext, backendUrl, guid);
            return httpContext;
        }

        private static void AddGlobalHttpContextVariables(HttpContext httpContext, string backendUrl)
        {
            httpContext.Items.Add(HttpContextItems.IsReplayingXmlContext, true);
            if (!string.IsNullOrWhiteSpace(backendUrl))
            {
                httpContext.Items.Add(HttpContextItems.BackendUrlContext, backendUrl);
            }
        }

        private static void AddGlobalHttpContextVariables(HttpContext httpContext, string backendUrl, Guid? resultUniqueId)
        {
            AddGlobalHttpContextVariables(httpContext, backendUrl);
            if (resultUniqueId.HasValue)
            {
                httpContext.Items.Add(HttpContextItems.XmlContextGuidSubstitution, resultUniqueId);
            }
        }

        private static void FillHttpRequest(HttpRequest httpRequest, XmlDbUpdateRecordedAction action)
        {
            var options = QPConnectionScope.Current.IdentityInsertOptions;
            var entityTypeCode = action.BackendAction.EntityType.Code;
            if (entityTypeCode == EntityTypeCode.VirtualContent)
            {
                entityTypeCode = EntityTypeCode.Content;
            }

            var actionTypeCode = action.BackendAction.ActionType.Code;
            action.Form.Add(HttpContextFormConstants.Ids, string.Join(",", action.Ids));

            if (actionTypeCode == ActionTypeCode.AddNew && options.Contains(entityTypeCode))
            {
                action.Form.Add(HttpContextFormConstants.DataForceId, action.Ids.First());
            }

            switch (action.Code)
            {
                case ActionCode.AddNewContent:
                    if (options.Contains(EntityTypeCode.Field))
                    {
                        AddListItem(action.Form, HttpContextFormConstants.DataForceFieldIds, action.ChildIds);
                    }

                    if (options.Contains(EntityTypeCode.ContentLink))
                    {
                        AddListItem(action.Form, HttpContextFormConstants.DataForceLinkIds, action.ChildLinkIds);
                    }

                    break;

                case ActionCode.CreateLikeContent:
                    if (options.Contains(EntityTypeCode.Content))
                    {
                        action.Form.Add(HttpContextFormConstants.ForceId, action.ResultId.ToString());
                    }

                    if (options.Contains(EntityTypeCode.Field))
                    {
                        action.Form.Add(HttpContextFormConstants.ForceFieldIds, action.ChildIds);
                    }

                    if (options.Contains(EntityTypeCode.ContentLink))
                    {
                        action.Form.Add(HttpContextFormConstants.ForceLinkIds, action.ChildLinkIds);
                    }

                    break;

                case ActionCode.CreateLikeField:
                    if (options.Contains(EntityTypeCode.Field))
                    {
                        action.Form.Add(HttpContextFormConstants.ForceId, action.ResultId.ToString());
                    }

                    if (options.Contains(EntityTypeCode.Field))
                    {
                        action.Form.Add(HttpContextFormConstants.ForceVirtualFieldIds, action.VirtualFieldIds);
                    }

                    if (options.Contains(EntityTypeCode.Field))
                    {
                        action.Form.Add(HttpContextFormConstants.ForceChildFieldIds, action.ChildIds);
                    }

                    if (options.Contains(EntityTypeCode.ContentLink))
                    {
                        action.Form.Add(HttpContextFormConstants.ForceLinkId, action.ChildId.ToString());
                    }

                    if (options.Contains(EntityTypeCode.ContentLink))
                    {
                        action.Form.Add(HttpContextFormConstants.ForceChildLinkIds, action.ChildLinkIds);
                    }

                    break;

                case ActionCode.AddNewVirtualContents:
                case ActionCode.VirtualContentProperties:
                case ActionCode.VirtualFieldProperties:
                    if (options.Contains(EntityTypeCode.Field))
                    {
                        AddListItem(action.Form, HttpContextFormConstants.DataForceVirtualFieldIds, action.VirtualFieldIds);
                    }

                    break;

                case ActionCode.AddNewField:
                case ActionCode.FieldProperties:
                    if (options.Contains(EntityTypeCode.ContentLink))
                    {
                        action.Form.Add(HttpContextFormConstants.DataContentLinkForceLinkId, action.ChildId.ToString());
                        AddListItem(action.Form, HttpContextFormConstants.DataForceChildLinkIds, action.ChildIds);
                    }

                    if (options.Contains(EntityTypeCode.Field))
                    {
                        AddListItem(action.Form, HttpContextFormConstants.DataForceVirtualFieldIds, action.VirtualFieldIds);
                        action.Form.Add(HttpContextFormConstants.DataForceBackwardId, action.BackwardId.ToString());
                        AddListItem(action.Form, HttpContextFormConstants.DataForceChildFieldIds, action.ChildIds);
                    }

                    break;

                case ActionCode.AddNewCustomAction:
                    if (options.Contains(EntityTypeCode.CustomAction))
                    {
                        action.Form.Add(HttpContextFormConstants.ForceId, action.ResultId.ToString());
                    }

                    action.Form.Add(HttpContextFormConstants.DataForceActionCode, action.CustomActionCode);

                    if (options.Contains(EntityTypeCode.BackendAction))
                    {
                        action.Form.Add(HttpContextFormConstants.DataForceActionId, action.ChildIds);
                    }
                    break;

                case ActionCode.CreateLikeCustomAction:
                    if (options.Contains(EntityTypeCode.CustomAction))
                    {
                        action.Form.Add(HttpContextFormConstants.ForceId, action.ResultId.ToString());
                    }

                    action.Form.Add(HttpContextFormConstants.ForceActionCode, action.CustomActionCode);

                    if (options.Contains(EntityTypeCode.BackendAction))
                    {
                        action.Form.Add(HttpContextFormConstants.ForceActionId, action.ChildIds);
                    }
                    break;

                case ActionCode.AddNewVisualEditorPlugin:
                case ActionCode.VisualEditorPluginProperties:
                    if (options.Contains(EntityTypeCode.VisualEditorCommand))
                    {
                        AddListItem(action.Form, HttpContextFormConstants.DataForceCommandIds, action.ChildIds);
                    }

                    break;

                case ActionCode.AddNewWorkflow:
                case ActionCode.WorkflowProperties:
                    if (options.Contains(EntityTypeCode.WorkflowRule))
                    {
                        AddListItem(action.Form, HttpContextFormConstants.DataForceRulesIds, action.ChildIds);
                    }

                    break;
                case ActionCode.RemoveEntityTypePermissionChanges:
                case ActionCode.RemoveActionPermissionChanges:
                case ActionCode.MultipleRemoveChildContentPermissions:
                case ActionCode.RemoveAllChildContentPermissions:
                case ActionCode.RemoveChildContentPermission:
                case ActionCode.MultipleRemoveChildArticlePermissions:
                case ActionCode.RemoveAllChildArticlePermissions:
                case ActionCode.RemoveChildArticlePermission:
                case ActionCode.ChangeEntityTypePermission:
                case ActionCode.ChangeActionPermission:

                    if (action.UserId != 0)
                    {
                        action.Form.Add(HttpContextFormConstants.UserId, action.UserId.ToString());
                    }
                    if (action.GroupId != 0)
                    {
                        action.Form.Add(HttpContextFormConstants.GroupId, action.GroupId.ToString());
                    }
                    break;
            }
            httpRequest.Method = "POST";
            httpRequest.Form = new FormCollection(action.Form);
            httpRequest.Path = action.BackendAction.ControllerActionUrl.Replace("~", string.Empty);
        }

        private static void AddListItem(Dictionary<string, StringValues> collection, string name, string commaList)
        {
            if (!string.IsNullOrEmpty(commaList))
            {
                foreach (var id in commaList.Split(",".ToCharArray()))
                {
                    collection.Add(name, id);
                }
            }
        }

        private static ClaimsPrincipal GetPrincipal(int userId)
        {
            var user = new UserService().ReadProfile(userId);
            var qpUser = new QpUser() { Id = user.Id, Name = user.Name, CustomerCode = QPContext.CurrentCustomerCode, LanguageId = user.LanguageId };
            return AuthenticationHelper.GetClaimsPrincipal(qpUser);
        }

        private static T GetContextData<T>(HttpContext httpContext, string key) => httpContext.Items.ContainsKey(key) ? (T)httpContext.Items[key] : default(T);

        private static Guid GetGuidContextData(HttpContext httpContext, string key) => httpContext.Items.ContainsKey(key) ? Guid.Parse(httpContext.Items[key].ToString()) : Guid.Empty;

        private static Guid[] GetGuidsContextData(HttpContext httpContext, string key) => httpContext.Items.ContainsKey(key) ? httpContext.Items[key].ToString().Split(",".ToCharArray()).Select(Guid.Parse).ToArray() : new Guid[] { };

        private static Dictionary<string, StringValues> GetDynamicFieldValuesFromHttpContext(HttpContext httpContext, string fieldPrefix)
        {
            var result = new Dictionary<string, StringValues>();
            foreach (var ctxKey in httpContext.Items.Keys.OfType<string>().Where(key => key.StartsWith(fieldPrefix)))
            {
                result.AddRange(GetStringValuesFromHttpContext(httpContext, ctxKey));
            }

            return result;
        }

        private static Dictionary<string, StringValues> GetStringValuesFromHttpContext(HttpContext httpContext, string key)
        {
            var result = new Dictionary<string, StringValues>();
            if (httpContext.Items.ContainsKey(key))
            {
                result.Add(key, (StringValues)httpContext.Items[key]);
            }
            return result;
        }
    }
}
