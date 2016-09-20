using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Security;
using Quantumart.QP8.WebMvc.Infrastructure.Extensions;
using Quantumart.QP8.WebMvc.Infrastructure.Models;

namespace Quantumart.QP8.WebMvc.Infrastructure.Services.XmlDbUpdate
{
    public class XmlDbUpdateActionCorrecterService
    {
        private readonly Dictionary<string, Dictionary<int, int>> _idsToReplace = new Dictionary<string, Dictionary<int, int>>();

        internal XmlDbUpdateRecordedAction CorrectAction(XmlDbUpdateRecordedAction entry)
        {
            var code = entry.BackendAction.EntityType.Code;
            var parentCode = entry.BackendAction.EntityType.ParentCode;
            entry.Ids = CorrectListValue(code, entry.Ids).ToArray();
            if (!string.IsNullOrEmpty(parentCode))
            {
                entry.ParentId = CorrectValue(parentCode, entry.ParentId);
            }

            switch (code)
            {
                case EntityTypeCode.Content:
                    CorrectContentForm(entry);
                    break;
                case EntityTypeCode.VirtualContent:
                    CorrectVirtualContentForm(entry);
                    break;
                case EntityTypeCode.Field:
                    CorrectFieldForm(entry.Form);
                    break;
                case EntityTypeCode.Article:
                    CorrectArticleForm(entry.Form, entry.ParentId);
                    break;
                case EntityTypeCode.User:
                    CorrectUserForm(entry);
                    break;
                case EntityTypeCode.UserGroup:
                    CorrectUserGroupForm(entry);
                    break;
                case EntityTypeCode.Site:
                    CorrectSiteForm(entry.Form);
                    break;
                case EntityTypeCode.CustomAction:
                    CorrectCustomActionForm(entry.Form);
                    break;
                case EntityTypeCode.VisualEditorPlugin:
                    CorrectVePluginForm(entry.Form);
                    break;
                case EntityTypeCode.Workflow:
                    CorrectWorkflowForm(entry.Form);
                    break;
                case EntityTypeCode.PageObject:
                    CorrectObjectForm(entry.Form, true);
                    break;
                case EntityTypeCode.TemplateObject:
                    CorrectObjectForm(entry.Form, false);
                    break;
                case EntityTypeCode.Notification:
                    CorrectNotificationForm(entry.Form);
                    break;
            }

            return entry;
        }

        internal XmlDbUpdateRecordedAction CorrectReplaces(XmlDbUpdateRecordedAction action, HttpContextBase context)
        {
            var actionTypeCode = action.BackendAction.ActionType.Code;
            var entityTypeCode = action.BackendAction.EntityType.Code;

            if (new[] { ActionTypeCode.AddNew, ActionTypeCode.Copy }.Contains(actionTypeCode))
            {
                var resultCode = entityTypeCode != EntityTypeCode.VirtualContent ? entityTypeCode : EntityTypeCode.Content;
                var resultId = action.ResultId != 0 ? action.ResultId : int.Parse(action.Ids[0]);
                AddIdToReplace(resultCode, resultId, context, "RESULT_ID");
            }

            switch (action.BackendAction.Code)
            {
                case ActionCode.AddNewVirtualContents:
                case ActionCode.VirtualContentProperties:
                case ActionCode.VirtualFieldProperties:
                    AddIdsToReplace(action.VirtualFieldIds, context, "NEW_VIRTUAL_FIELD_IDS");
                    break;
                case ActionCode.AddNewContent:
                case ActionCode.CreateLikeContent:
                    AddIdsToReplace(action.ChildIds, context, "FIELD_IDS");
                    AddIdsToReplace(action.ChildLinkIds, context, "LINK_IDS");
                    break;
                case ActionCode.AddNewField:
                case ActionCode.FieldProperties:
                case ActionCode.CreateLikeField:
                    AddIdToReplace(EntityTypeCode.ContentLink, action.ChildId, context, "NEW_LINK_ID");
                    AddIdToReplace(EntityTypeCode.Field, action.BackwardId, context, "NEW_BACKWARD_ID");
                    AddIdsToReplace(action.VirtualFieldIds, context, "NEW_VIRTUAL_FIELD_IDS");
                    AddIdsToReplace(action.ChildIds, context, "NEW_CHILD_FIELD_IDS");
                    AddIdsToReplace(action.ChildLinkIds, context, "NEW_CHILD_LINK_IDS");
                    break;
                case ActionCode.AddNewCustomAction:
                    AddIdToReplace(EntityTypeCode.BackendAction, action.ChildId, context, "ACTION_ID");
                    break;
                case ActionCode.AddNewVisualEditorPlugin:
                case ActionCode.VisualEditorPluginProperties:
                    AddIdsToReplace(action.ChildIds, context, "NEW_COMMAND_IDS");
                    break;
                case ActionCode.AddNewWorkflow:
                case ActionCode.WorkflowProperties:
                    AddIdsToReplace(action.ChildIds, context, "NEW_RULES_IDS");
                    break;
                case ActionCode.AddNewNotification:
                case ActionCode.NotificationProperties:
                    AddIdToReplace(EntityTypeCode.TemplateObjectFormat, action.ChildId, context, "NOTIFICATION_FORMAT_ID");
                    break;
                case ActionCode.AddNewPageObject:
                    AddIdToReplace(EntityTypeCode.PageObjectFormat, action.ChildId, context, "DEFAULT_FORMAT_ID");
                    break;
                case ActionCode.AddNewTemplateObject:
                    AddIdToReplace(EntityTypeCode.TemplateObjectFormat, action.ChildId, context, "DEFAULT_FORMAT_ID");
                    break;
            }

            return action;
        }

        internal static XmlDbUpdateRecordedAction CreateActionFromHttpContext(HttpContextBase httpContext, string actionCode, bool ignoreForm)
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

        private static T GetContextData<T>(HttpContextBase httpContext, string key)
        {
            return httpContext.Items.Contains(key) ? (T)httpContext.Items[key] : default(T);
        }

        private void CorrectContentForm(XmlDbUpdateRecordedAction action)
        {
            CorrectFormValue(EntityTypeCode.Workflow, action.Form, "Data.WorkflowBinding.WorkflowId");
            CorrectFormValue(EntityTypeCode.Content, action.Form, "Data.ParentContentId");
            CorrectFormValue(EntityTypeCode.ContentGroup, action.Form, "Data.GroupId");
        }

        private void CorrectVirtualContentForm(XmlDbUpdateRecordedAction action)
        {
            CorrectFormValue(EntityTypeCode.Content, action.Form, "Data.JoinRootId");
            CorrectFormValue(EntityTypeCode.Content, action.Form, "Data.UnionSourceContentIDs");
            CorrectFormValue(EntityTypeCode.Field, action.Form, "JoinFields", true, true);
            CorrectFormValue(EntityTypeCode.Field, action.Form, "itemValue", true, true);
        }

        private void CorrectUserGroupForm(XmlDbUpdateRecordedAction action)
        {
            CorrectFormValue(EntityTypeCode.User, action.Form, "BindedUserIDs", true);
            CorrectFormValue(EntityTypeCode.UserGroup, action.Form, "ParentGroupId");
        }

        private void CorrectUserForm(XmlDbUpdateRecordedAction action)
        {
            CorrectFormValue(EntityTypeCode.UserGroup, action.Form, "SelectedGroups", true);
            CorrectFormValue(EntityTypeCode.Site, action.Form, "ContentDefaultFilter.SiteId");
            CorrectFormValue(EntityTypeCode.Content, action.Form, "ContentDefaultFilter.ContentId");
            CorrectFormValue(EntityTypeCode.Article, action.Form, "ContentDefaultFilter.ArticleIDs");
        }

        private void CorrectFieldForm(NameValueCollection form)
        {
            CorrectFormValue(EntityTypeCode.Field, form, "Data.Id");
            CorrectFormValue(EntityTypeCode.Field, form, "InCombinationWith", true);
            CorrectFormValue(EntityTypeCode.Content, form, "Data.RelateToContentId");
            CorrectFormValue(EntityTypeCode.Field, form, "Data.RelationId");
            CorrectFormValue(EntityTypeCode.Field, form, "Data.BackRelationId");
            CorrectFormValue(EntityTypeCode.Field, form, "Data.ClassifierId");
            CorrectFormValue(EntityTypeCode.Field, form, "Data.BaseImageId");
            CorrectFormValue(EntityTypeCode.Field, form, "Data.ListOrderFieldId");
            CorrectFormValue(EntityTypeCode.Field, form, "Data.TreeOrderFieldId");
            CorrectFormValue(EntityTypeCode.ContentLink, form, "Data.ContentLink.LinkId");
            CorrectFormValue(EntityTypeCode.Article, form, "Data.O2MDefaultValue");
            CorrectFormValue(EntityTypeCode.Article, form, "DefaultArticleIds", true);
            CorrectFormValue(EntityTypeCode.VisualEditorCommand, form, "ActiveVeCommands", true);
            CorrectFormValue(EntityTypeCode.VisualEditorStyle, form, "ActiveVeStyles", true);
            CorrectFormValue(EntityTypeCode.VisualEditorStyle, form, "ActiveVeFormats", true);
        }

        private void CorrectSiteForm(NameValueCollection form)
        {
            CorrectFormValue(EntityTypeCode.VisualEditorCommand, form, "ActiveVeCommands", true);
            CorrectFormValue(EntityTypeCode.VisualEditorStyle, form, "ActiveVeStyles", true);
            CorrectFormValue(EntityTypeCode.VisualEditorStyle, form, "ActiveVeFormats", true);
        }

        private void CorrectObjectForm(NameValueCollection form, bool isPageObject)
        {
            CorrectFormValue(EntityTypeCode.TemplateObject, form, "Data.ParentObjectId");
            CorrectFormValue(isPageObject ? EntityTypeCode.PageObjectFormat : EntityTypeCode.TemplateObjectFormat, form, "Data.DefaultFormatId");
            CorrectFormValue(EntityTypeCode.Content, form, "Data.Container.ContentId");
            CorrectFormValue(EntityTypeCode.Content, form, "Data.ContentForm.ContentId");
            CorrectFormValue(EntityTypeCode.Page, form, "Data.ContentForm.ThankYouPageId");
            CorrectFormValue(EntityTypeCode.StatusType, form, "ActiveStatusTypeIds", true);
        }

        private void CorrectCustomActionForm(NameValueCollection form)
        {
            CorrectFormValue(EntityTypeCode.Site, form, "SelectedSiteIDs", true);
            CorrectFormValue(EntityTypeCode.Content, form, "SelectedContentIDs", true);
            CorrectFormValue(EntityTypeCode.BackendAction, form, "SelectedActions", true);
        }

        private void CorrectVePluginForm(NameValueCollection form)
        {
            CorrectFormValue(EntityTypeCode.VisualEditorCommand, form, "AggregationListItems_VeCommandsDisplay", "Id");
        }

        private void CorrectWorkflowForm(NameValueCollection form)
        {
            CorrectFormValue(EntityTypeCode.StatusType, form, "ActiveStatuses", true);
            CorrectFormValue(EntityTypeCode.Content, form, "ActiveContentIds", true);
            CorrectFormValue(EntityTypeCode.WorkflowRule, form, "Workflows_WorkflowRulesDisplay", "Id");
            CorrectFormValue(EntityTypeCode.StatusType, form, "Workflows_WorkflowRulesDisplay", "StId");
            CorrectFormValue(EntityTypeCode.User, form, "Workflows_WorkflowRulesDisplay", "UserId");
            CorrectFormValue(EntityTypeCode.UserGroup, form, "Workflows_WorkflowRulesDisplay", "GroupId");
        }

        private void CorrectNotificationForm(NameValueCollection form)
        {
            CorrectFormValue(EntityTypeCode.TemplateObjectFormat, form, "Data.FormatId");
            CorrectFormValue(EntityTypeCode.User, form, "Data.UserId");
            CorrectFormValue(EntityTypeCode.UserGroup, form, "Data.GroupId");
            CorrectFormValue(EntityTypeCode.Field, form, "Data.EmailFieldId");
            CorrectFormValue(EntityTypeCode.User, form, "Data.FromBackenduserId");
            CorrectFormValue(EntityTypeCode.StatusType, form, "Data.NotifyOnStatusTypeId");
        }

        private void CorrectArticleForm(NameValueCollection form, int contentId)
        {
            var relations = ReplayHelper.GetRelations(contentId);
            var classifiers = ReplayHelper.GetClasifiers(contentId);
            foreach (var key in form.AllKeys.Where(n => n.StartsWith("field_")))
            {
                int result;
                var parsed = int.TryParse(key.Replace("field_", string.Empty), out result);
                if (parsed)
                {
                    var newFieldId = CorrectValue(EntityTypeCode.Field, result);
                    if (newFieldId != result)
                    {
                        var newKey = "field_" + newFieldId;
                        var enumerable = form.GetValues(key);
                        if (enumerable != null)
                        {
                            foreach (var value in enumerable)
                            {
                                var newValue = value;
                                if (relations.ContainsKey(newFieldId) || classifiers.ContainsKey(newFieldId))
                                {
                                    var code = relations.ContainsKey(newFieldId) ? EntityTypeCode.Article : EntityTypeCode.Content;
                                    newValue = CorrectCommaListValue(code, value);
                                }

                                form.Add(newKey, newValue);
                            }
                        }

                        form.Remove(key);
                    }
                    else
                    {
                        if (relations.ContainsKey(newFieldId) || classifiers.ContainsKey(newFieldId))
                        {
                            var values = form.GetValues(key);
                            form.Remove(key);

                            if (values != null)
                            {
                                foreach (var value in values)
                                {
                                    var code = relations.ContainsKey(newFieldId) ? EntityTypeCode.Article : EntityTypeCode.Content;
                                    form.Add(key, CorrectCommaListValue(code, value));
                                }
                            }
                        }
                    }
                }
            }
        }

        private string CorrectValue(string code, string value)
        {
            int result;
            var parsed = int.TryParse(value, out result);
            return parsed ? CorrectValue(code, result).ToString() : value;
        }

        private void CorrectFormValue(string code, NameValueCollection form, string formKey, bool prefixSearch = false, bool joinModeReplace = false)
        {
            if (!prefixSearch)
            {
                var formValue = form[formKey];
                if (formValue != null)
                {
                    form[formKey] = CorrectValue(code, formValue);
                }
            }
            else
            {
                foreach (var key in form.AllKeys.Where(n => n.StartsWith(formKey)))
                {
                    if (!joinModeReplace || !key.EndsWith("Index"))
                    {
                        var values = form.GetValues(key);
                        form.Remove(key);

                        if (values != null)
                        {
                            foreach (var value in values)
                            {
                                string newValue;
                                if (!joinModeReplace)
                                {
                                    newValue = CorrectValue(code, value);
                                }
                                else
                                {
                                    newValue = string.Join(".", value.Replace("[", string.Empty).Replace("]", string.Empty).Split(".".ToCharArray()).Select(n => CorrectValue(code, n)));
                                    newValue = "[" + newValue + "]";
                                }

                                form.Add(key, CorrectValue(code, newValue));
                            }
                        }
                    }
                }
            }
        }

        private void CorrectFormValue(string code, NameValueCollection form, string formKey, string jsonKey)
        {
            var formValue = form[formKey];
            if (formValue != null)
            {
                var serializer = new JavaScriptSerializer();
                var collectionList = serializer.Deserialize<List<Dictionary<string, string>>>(formValue);
                foreach (var collection in collectionList.Where(collection => collection.ContainsKey(jsonKey)))
                {
                    collection[jsonKey] = CorrectValue(code, collection[jsonKey]);
                }

                form[formKey] = serializer.Serialize(collectionList);
            }
        }

        private string CorrectCommaListValue(string code, string value)
        {
            return string.Join(",", CorrectListValue(code, value.Split(",".ToCharArray())));
        }

        private IEnumerable<string> CorrectListValue(string code, IEnumerable<string> values)
        {
            foreach (var value in values)
            {
                int parsedValue;
                var parsed = int.TryParse(value, out parsedValue);
                yield return parsed ? CorrectValue(code, parsedValue).ToString() : value;
            }
        }

        private int CorrectValue(string code, int value)
        {
            if (_idsToReplace.ContainsKey(code) && _idsToReplace[code].ContainsKey(value))
            {
                return _idsToReplace[code][value];
            }

            return value;
        }

        private void AddIdToReplace(string code, int id, HttpContextBase context, string key)
        {
            if (context.Items.Contains(key))
            {
                AddIdToReplace(code, id, int.Parse(context.Items[key].ToString()));
            }
        }

        private void AddIdToReplace(string code, int id, int newId)
        {
            if (id != newId)
            {
                if (!_idsToReplace.ContainsKey(code))
                {
                    _idsToReplace.Add(code, new Dictionary<int, int>());
                }

                if (id != 0)
                {
                    _idsToReplace[code].Add(id, newId);
                }
            }
        }

        private void AddIdsToReplace(string oldIdsCommaString, HttpContextBase context, string key)
        {
            if (context.Items.Contains(key))
            {
                AddIdsToReplace(EntityTypeCode.Field, oldIdsCommaString.ToIntArray(), context.Items[key].ToString().ToIntArray());
            }
        }

        private void AddIdsToReplace(string code, IReadOnlyList<int> oldIds, IReadOnlyList<int> newIds)
        {
            if (oldIds == null || newIds == null)
            {
                return;
            }

            if (oldIds.Count != newIds.Count)
            {
                throw new ArgumentException("Array leghths are not equal");
            }

            for (var i = 0; i < oldIds.Count; i++)
            {
                AddIdToReplace(code, oldIds[i], newIds[i]);
            }
        }
    }
}
