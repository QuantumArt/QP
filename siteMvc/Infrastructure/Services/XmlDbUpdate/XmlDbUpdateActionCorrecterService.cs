using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Services.XmlDbUpdate;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Infrastructure.Extensions;
using Quantumart.QP8.WebMvc.Infrastructure.Models;
using Quantumart.QP8.WebMvc.ViewModels.Article;

namespace Quantumart.QP8.WebMvc.Infrastructure.Services.XmlDbUpdate
{
    public class XmlDbUpdateActionCorrecterService
    {
        private readonly Dictionary<string, Dictionary<int, int>> _idsToReplace = new Dictionary<string, Dictionary<int, int>>();
        private readonly Dictionary<string, Dictionary<Guid, Guid>> _uniqueIdsToReplace = new Dictionary<string, Dictionary<Guid, Guid>>();

        private readonly IXmlDbUpdateActionService _dbActionService;

        public XmlDbUpdateActionCorrecterService(IXmlDbUpdateActionService dbActionService)
        {
            _dbActionService = dbActionService;
        }

        private XmlDbUpdateRecordedAction CorrectUniqueGuidValue(string code, XmlDbUpdateRecordedAction entry)
        {
            if (entry.BackendAction.Code == ActionCode.CreateLikeArticle)
            {
                entry.UniqueId = CorrectUniqueIdValue(code, entry.UniqueId);
            }
            else
            {
                return CorrectFormGuidFieldValue(entry, code, vm => vm.Data.UniqueId, CorrectUniqueIdValue);
            }

            return entry;
        }

        private static XmlDbUpdateRecordedAction CorrectFormGuidFieldValue(XmlDbUpdateRecordedAction entry, string code, Expression<Func<ArticleViewModel, Guid?>> fieldExpression, Func<string, Guid, Guid> convertFn)
        {
            var uniqueIdFieldName = ExpressionHelper.GetExpressionText(fieldExpression);
            entry.UniqueId = convertFn(code, Guid.Parse(entry.Form[uniqueIdFieldName]));
            entry.Form[uniqueIdFieldName] = entry.UniqueId.ToString();
            return entry;
        }

        internal XmlDbUpdateRecordedAction CorrectAction(XmlDbUpdateRecordedAction entry, bool useGuidSubstitution)
        {
            entry = CorrectUniqueGuidValue(EntityTypeCode.Article, entry);
            if (useGuidSubstitution)
            {
                entry = SubstituteIdsFromGuids(entry);
            }

            entry.Ids = CorrectIdsValue(entry.BackendAction.EntityType.Code, entry.Ids).ToArray();
            if (!string.IsNullOrEmpty(entry.BackendAction.EntityType.ParentCode))
            {
                entry.ParentId = CorrectIdValue(entry.BackendAction.EntityType.ParentCode, entry.ParentId);
            }

            switch (entry.BackendAction.EntityType.Code)
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

        private XmlDbUpdateRecordedAction SubstituteIdsFromGuids(XmlDbUpdateRecordedAction entry)
        {
            if (entry.BackendAction.EntityType.Code == EntityTypeCode.Article)
            {
                var id = GetArticleIdByGuid(entry.UniqueId);
                entry.Ids = id.HasValue ? new[] { id.ToString() } : entry.Ids;
                if (IsActionNewOrCopy(entry))
                {
                    entry.ResultId = GetArticleIdByGuidOrDefault(entry.ResultUniqueId) ?? entry.ResultId;
                }
            }

            return entry;
        }

        internal XmlDbUpdateRecordedAction CorrectReplaces(XmlDbUpdateRecordedAction action, HttpContextBase httpContext)
        {
            // TODO: MOVE CONSTANTS TO FILE
            AddResultId(action, httpContext);
            AddResultUniqueId(action, httpContext);
            switch (action.BackendAction.Code)
            {
                case ActionCode.AddNewVirtualContents:
                case ActionCode.VirtualContentProperties:
                case ActionCode.VirtualFieldProperties:
                    AddIdsToReplace(action.VirtualFieldIds, httpContext, "NEW_VIRTUAL_FIELD_IDS");
                    break;
                case ActionCode.AddNewContent:
                case ActionCode.CreateLikeContent:
                    AddIdsToReplace(action.ChildIds, httpContext, "FIELD_IDS");
                    AddIdsToReplace(action.ChildLinkIds, httpContext, "LINK_IDS");
                    break;
                case ActionCode.AddNewField:
                case ActionCode.FieldProperties:
                case ActionCode.CreateLikeField:
                    AddIdToReplace(EntityTypeCode.ContentLink, action.ChildId, httpContext, "NEW_LINK_ID");
                    AddIdToReplace(EntityTypeCode.Field, action.BackwardId, httpContext, "NEW_BACKWARD_ID");
                    AddIdsToReplace(action.VirtualFieldIds, httpContext, "NEW_VIRTUAL_FIELD_IDS");
                    AddIdsToReplace(action.ChildIds, httpContext, "NEW_CHILD_FIELD_IDS");
                    AddIdsToReplace(action.ChildLinkIds, httpContext, "NEW_CHILD_LINK_IDS");
                    break;
                case ActionCode.AddNewCustomAction:
                    AddIdToReplace(EntityTypeCode.BackendAction, action.ChildId, httpContext, "ACTION_ID");
                    break;
                case ActionCode.AddNewVisualEditorPlugin:
                case ActionCode.VisualEditorPluginProperties:
                    AddIdsToReplace(action.ChildIds, httpContext, "NEW_COMMAND_IDS");
                    break;
                case ActionCode.AddNewWorkflow:
                case ActionCode.WorkflowProperties:
                    AddIdsToReplace(action.ChildIds, httpContext, "NEW_RULES_IDS");
                    break;
                case ActionCode.AddNewNotification:
                case ActionCode.NotificationProperties:
                    AddIdToReplace(EntityTypeCode.TemplateObjectFormat, action.ChildId, httpContext, "NOTIFICATION_FORMAT_ID");
                    break;
                case ActionCode.AddNewPageObject:
                    AddIdToReplace(EntityTypeCode.PageObjectFormat, action.ChildId, httpContext, "DEFAULT_FORMAT_ID");
                    break;
                case ActionCode.AddNewTemplateObject:
                    AddIdToReplace(EntityTypeCode.TemplateObjectFormat, action.ChildId, httpContext, "DEFAULT_FORMAT_ID");
                    break;
            }

            return action;
        }

        private void AddResultId(XmlDbUpdateRecordedAction action, HttpContextBase httpContext)
        {
            if (IsActionNewOrCopy(action))
            {
                var entityTypeCode = action.BackendAction.EntityType.Code != EntityTypeCode.VirtualContent
                    ? action.BackendAction.EntityType.Code
                    : EntityTypeCode.Content;

                var resultId = action.ResultId != 0 ? action.ResultId : int.Parse(action.Ids[0]);
                AddIdToReplace(entityTypeCode, resultId, httpContext, "RESULT_ID");
            }
        }

        private void AddResultUniqueId(XmlDbUpdateRecordedAction action, HttpContextBase httpContext)
        {
            if (IsActionNewOrCopy(action))
            {
                var entityTypeCode = action.BackendAction.EntityType.Code != EntityTypeCode.VirtualContent
                    ? action.BackendAction.EntityType.Code
                    : EntityTypeCode.Content;

                var resultUniqueId = action.ResultUniqueId != default(Guid) ? action.ResultUniqueId : action.UniqueId;
                AddUniqueIdToReplace(entityTypeCode, resultUniqueId, httpContext, "RESULT_GUID");
            }
        }

        private static bool IsActionNewOrCopy(XmlDbUpdateRecordedAction action)
        {
            return new[] { ActionTypeCode.AddNew, ActionTypeCode.Copy }.Contains(action.BackendAction.ActionType.Code);
        }

        private int? GetArticleIdByGuid(Guid guid)
        {
            return guid == default(Guid) ? (int?)null : _dbActionService.GetArticleIdByGuid(guid);
        }

        private int? GetArticleIdByGuidOrDefault(Guid guid)
        {
            return guid == default(Guid) ? null : _dbActionService.GetArticleIdByGuidOrDefault(guid);
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
                    var newFieldId = CorrectIdValue(EntityTypeCode.Field, result);
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

        private void CorrectFormValue(string code, NameValueCollection form, string formKey, bool prefixSearch = false, bool joinModeReplace = false)
        {
            if (!prefixSearch)
            {
                var formValue = form[formKey];
                if (formValue != null)
                {
                    form[formKey] = CorrectIdValue(code, formValue);
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
                                    newValue = CorrectIdValue(code, value);
                                }
                                else
                                {
                                    newValue = string.Join(".", value.Replace("[", string.Empty).Replace("]", string.Empty).Split(".".ToCharArray()).Select(n => CorrectIdValue(code, n)));
                                    newValue = "[" + newValue + "]";
                                }

                                form.Add(key, CorrectIdValue(code, newValue));
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
                    collection[jsonKey] = CorrectIdValue(code, collection[jsonKey]);
                }

                form[formKey] = serializer.Serialize(collectionList);
            }
        }

        private string CorrectCommaListValue(string code, string value)
        {
            return string.Join(",", CorrectIdsValue(code, value.Split(",".ToCharArray())));
        }

        private IEnumerable<string> CorrectIdsValue(string code, IEnumerable<string> values)
        {
            foreach (var value in values)
            {
                int parsedValue;
                yield return int.TryParse(value, out parsedValue) ? CorrectIdValue(code, parsedValue).ToString() : value;
            }
        }

        private string CorrectIdValue(string code, string value)
        {
            int result;
            return int.TryParse(value, out result) ? CorrectIdValue(code, result).ToString() : value;
        }

        private int CorrectIdValue(string code, int value)
        {
            if (_idsToReplace.ContainsKey(code) && _idsToReplace[code].ContainsKey(value))
            {
                return _idsToReplace[code][value];
            }

            return value;
        }

        private Guid CorrectUniqueIdValue(string code, Guid value)
        {
            if (_uniqueIdsToReplace.ContainsKey(code) && _uniqueIdsToReplace[code].ContainsKey(value))
            {
                return _uniqueIdsToReplace[code][value];
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

        private void AddUniqueIdToReplace(string code, Guid uniqueId, HttpContextBase context, string key)
        {
            if (context.Items.Contains(key))
            {
                AddUniqueIdToReplace(code, uniqueId, Guid.Parse(context.Items[key].ToString()));
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

        private void AddUniqueIdToReplace(string code, Guid uniqueId, Guid newUniqueId)
        {
            if (uniqueId != newUniqueId)
            {
                if (!_uniqueIdsToReplace.ContainsKey(code))
                {
                    _uniqueIdsToReplace.Add(code, new Dictionary<Guid, Guid>());
                }

                if (uniqueId != default(Guid))
                {
                    _uniqueIdsToReplace[code].Add(uniqueId, newUniqueId);
                }
            }
        }

        private void AddIdsToReplace(string oldIdsCommaString, HttpContextBase context, string key)
        {
            if (context.Items.Contains(key))
            {
                IReadOnlyList<int> oldIds = oldIdsCommaString.ToIntArray();
                IReadOnlyList<int> newIds = context.Items[key].ToString().ToIntArray();
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
                    AddIdToReplace(EntityTypeCode.Field, oldIds[i], newIds[i]);
                }
            }
        }
    }
}
