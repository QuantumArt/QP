using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Services.XmlDbUpdate;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Infrastructure.Extensions;
using Quantumart.QP8.WebMvc.Infrastructure.Helpers.XmlDbUpdate;
using Quantumart.QP8.WebMvc.Infrastructure.Models;

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

        internal XmlDbUpdateRecordedAction PreActionCorrections(XmlDbUpdateRecordedAction action, bool useGuidSubstitution)
        {
            action = CorrectActionIds(action, useGuidSubstitution);
            switch (action.BackendAction.EntityType.Code)
            {
                case EntityTypeCode.Content:
                    CorrectContentForm(action);
                    break;
                case EntityTypeCode.VirtualContent:
                    CorrectVirtualContentForm(action);
                    break;
                case EntityTypeCode.Field:
                    CorrectFieldForm(action.Form);
                    break;
                case EntityTypeCode.Article:
                    CorrectArticleForm(action.Form, action.ParentId);
                    break;
                case EntityTypeCode.User:
                    CorrectUserForm(action);
                    break;
                case EntityTypeCode.UserGroup:
                    CorrectUserGroupForm(action);
                    break;
                case EntityTypeCode.Site:
                    CorrectSiteForm(action.Form);
                    break;
                case EntityTypeCode.CustomAction:
                    CorrectCustomActionForm(action.Form);
                    break;
                case EntityTypeCode.VisualEditorPlugin:
                    CorrectVePluginForm(action.Form);
                    break;
                case EntityTypeCode.Workflow:
                    CorrectWorkflowForm(action.Form);
                    break;
                case EntityTypeCode.PageObject:
                    CorrectObjectForm(action.Form, true);
                    break;
                case EntityTypeCode.TemplateObject:
                    CorrectObjectForm(action.Form, false);
                    break;
                case EntityTypeCode.Notification:
                    CorrectNotificationForm(action.Form);
                    break;
            }

            return action;
        }

        internal XmlDbUpdateRecordedAction PostActionCorrections(XmlDbUpdateRecordedAction action, HttpContextBase httpContext)
        {
            AddResultIds(action, httpContext);
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

        private XmlDbUpdateRecordedAction CorrectActionIds(XmlDbUpdateRecordedAction action, bool useGuidSubstitution)
        {
            if (XmlDbUpdateQpActionHelpers.IsArticleAndHasUniqueId(action.Code))
            {
                action = CorrectEntryUniqueIdsValue(action);
                if (useGuidSubstitution)
                {
                    action = SubstituteArticleIdsFromGuids(action);
                }
                else if (XmlDbUpdateQpActionHelpers.IsNewArticle(action.Code))
                {
                    action.UniqueId = new[] { Guid.NewGuid() };
                    var uniqueIdFieldName = XmlDbUpdateQpActionHelpers.GetFieldName(vm => vm.Data.UniqueId);
                    action.Form[uniqueIdFieldName] = action.UniqueId.Single().ToString();
                }
            }

            var entityTypeCode = action.BackendAction.EntityType.Code == EntityTypeCode.ArchiveArticle
                ? EntityTypeCode.Article
                : action.BackendAction.EntityType.Code;

            action.Ids = CorrectIdsValue(entityTypeCode, action.Ids).ToArray();
            if (!string.IsNullOrEmpty(action.BackendAction.EntityType.ParentCode))
            {
                action.ParentId = CorrectIdValue(action.BackendAction.EntityType.ParentCode, action.ParentId);
            }

            return action;
        }

        private XmlDbUpdateRecordedAction CorrectEntryUniqueIdsValue(XmlDbUpdateRecordedAction action)
        {
            if (XmlDbUpdateQpActionHelpers.IsArticleAndStoreUniqueIdInForm(action.Code))
            {
                action = CorrectFormUniqueId(action);
            }

            action.UniqueId = action.UniqueId.Select(g => CorrectAttributeUniqueId(EntityTypeCode.Article, g)).ToArray();
            return action;
        }

        private IEnumerable<string> CorrectIdsValue(string entityTypeCode, IEnumerable<string> ids)
        {
            return ids.Select(id => CorrectIdValue(entityTypeCode, id));
        }

        private string CorrectIdValue(string entityTypeCode, string value)
        {
            // TODO: REMOVE UNUSED CODE
            int result;
            return int.TryParse(value, out result) ? CorrectIdValue(entityTypeCode, result).ToString() : value;
        }

        private int CorrectIdValue(string entityTypeCode, int value)
        {
            if (_idsToReplace.ContainsKey(entityTypeCode) && _idsToReplace[entityTypeCode].ContainsKey(value))
            {
                return _idsToReplace[entityTypeCode][value];
            }

            return value;
        }

        private XmlDbUpdateRecordedAction CorrectFormUniqueId(XmlDbUpdateRecordedAction action)
        {
            var uniqueIdFieldName = XmlDbUpdateQpActionHelpers.GetFieldName(vm => vm.Data.UniqueId);
            action.Form[uniqueIdFieldName] = CorrectAttributeUniqueId(EntityTypeCode.Article, Guid.Parse(action.Form[uniqueIdFieldName])).ToString();
            return action;
        }

        private Guid CorrectAttributeUniqueId(string entityTypeCode, Guid value)
        {
            if (_uniqueIdsToReplace.ContainsKey(entityTypeCode) && _uniqueIdsToReplace[entityTypeCode].ContainsKey(value))
            {
                return _uniqueIdsToReplace[entityTypeCode][value];
            }

            return value;
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
                                    var entityTypeCode = relations.ContainsKey(newFieldId) ? EntityTypeCode.Article : EntityTypeCode.Content;
                                    newValue = CorrectCommaListValue(entityTypeCode, value);
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
                                    var entityTypeCode = relations.ContainsKey(newFieldId) ? EntityTypeCode.Article : EntityTypeCode.Content;
                                    form.Add(key, CorrectCommaListValue(entityTypeCode, value));
                                }
                            }
                        }
                    }
                }
            }
        }

        private void CorrectFormValue(string entityTypeCode, NameValueCollection form, string formKey, string jsonKey)
        {
            var formValue = form[formKey];
            if (formValue != null)
            {
                var serializer = new JavaScriptSerializer();
                var collectionList = serializer.Deserialize<List<Dictionary<string, string>>>(formValue);
                foreach (var collection in collectionList.Where(collection => collection.ContainsKey(jsonKey)))
                {
                    collection[jsonKey] = CorrectIdValue(entityTypeCode, collection[jsonKey]);
                }

                form[formKey] = serializer.Serialize(collectionList);
            }
        }

        private void CorrectFormValue(string entityTypeCode, NameValueCollection form, string formKey, bool prefixSearch = false, bool joinModeReplace = false)
        {
            if (!prefixSearch)
            {
                var formValue = form[formKey];
                if (formValue != null)
                {
                    form[formKey] = CorrectIdValue(entityTypeCode, formValue);
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
                                    newValue = CorrectIdValue(entityTypeCode, value);
                                }
                                else
                                {
                                    newValue = string.Join(".", value.Replace("[", string.Empty).Replace("]", string.Empty).Split(".".ToCharArray()).Select(n => CorrectIdValue(entityTypeCode, n)));
                                    newValue = "[" + newValue + "]";
                                }

                                form.Add(key, CorrectIdValue(entityTypeCode, newValue));
                            }
                        }
                    }
                }
            }
        }

        private string CorrectCommaListValue(string entityTypeCode, string value)
        {
            return string.Join(",", CorrectIdsValue(entityTypeCode, value.Split(",".ToCharArray())));
        }

        private XmlDbUpdateRecordedAction SubstituteArticleIdsFromGuids(XmlDbUpdateRecordedAction action)
        {
            Ensure.Equal(action.UniqueId.Length, action.Ids.Length, "Amount of uniqueIds and ids should be equal");
            if (XmlDbUpdateQpActionHelpers.IsActionHasResultId(action.Code))
            {
                action.ResultId = GetArticleResultIdByGuidOrDefault(action);
            }

            if (!XmlDbUpdateQpActionHelpers.IsNewArticle(action.Code))
            {
                action.Ids = action.UniqueId
                    .Select(_dbActionService.GetArticleIdByGuid)
                    .Select(g => g.ToString())
                    .ToArray();
            }

            return action;
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
                    throw new ArgumentException("Array lengths are not equal");
                }

                for (var i = 0; i < oldIds.Count; i++)
                {
                    AddIdToReplace(EntityTypeCode.Field, oldIds[i], newIds[i]);
                }
            }
        }

        private void AddIdToReplace(string entityTypeCode, int id, int newId)
        {
            if (id != newId)
            {
                if (!_idsToReplace.ContainsKey(entityTypeCode))
                {
                    _idsToReplace.Add(entityTypeCode, new Dictionary<int, int>());
                }

                if (id != default(int))
                {
                    if (_idsToReplace[entityTypeCode].ContainsKey(id))
                    {
                        _idsToReplace[entityTypeCode][id] = newId;
                    }
                    else
                    {
                        _idsToReplace[entityTypeCode].Add(id, newId);
                    }
                }
            }
        }

        private void AddIdToReplace(string entityTypeCode, int id, HttpContextBase context, string key)
        {
            if (context.Items.Contains(key))
            {
                AddIdToReplace(entityTypeCode, id, int.Parse(context.Items[key].ToString()));
            }
        }

        private void AddResultIds(XmlDbUpdateRecordedAction action, HttpContextBase httpContext)
        {
            if (XmlDbUpdateQpActionHelpers.IsActionHasResultId(action.Code))
            {
                var entityTypeCode = action.BackendAction.EntityType.Code != EntityTypeCode.VirtualContent ? action.BackendAction.EntityType.Code : EntityTypeCode.Content;
                var resultId = action.ResultId != default(int) ? action.ResultId : int.Parse(action.Ids.First());
                AddIdToReplace(entityTypeCode, resultId, httpContext, "RESULT_ID");

                var resultUniqueId = action.ResultUniqueId != Guid.Empty ? action.ResultUniqueId : action.UniqueId.First();
                AddUniqueIdToReplace(entityTypeCode, resultUniqueId, httpContext, "RESULT_GUID");
            }
        }

        private void AddUniqueIdToReplace(string entityTypeCode, Guid uniqueId, Guid newUniqueId)
        {
            if (uniqueId != newUniqueId)
            {
                if (!_uniqueIdsToReplace.ContainsKey(entityTypeCode))
                {
                    _uniqueIdsToReplace.Add(entityTypeCode, new Dictionary<Guid, Guid>());
                }

                if (uniqueId != Guid.Empty)
                {
                    if (_uniqueIdsToReplace[entityTypeCode].ContainsKey(uniqueId))
                    {
                        _uniqueIdsToReplace[entityTypeCode][uniqueId] = newUniqueId;
                    }
                    else
                    {
                        _uniqueIdsToReplace[entityTypeCode].Add(uniqueId, newUniqueId);
                    }
                }
            }
        }

        private void AddUniqueIdToReplace(string entityTypeCode, Guid uniqueId, HttpContextBase context, string key)
        {
            if (context.Items.Contains(key))
            {
                AddUniqueIdToReplace(entityTypeCode, uniqueId, Guid.Parse(context.Items[key].ToString()));
            }
        }

        private int GetArticleResultIdByGuidOrDefault(XmlDbUpdateRecordedAction action)
        {
            var articleIdByGuid = action.ResultUniqueId == Guid.Empty ? null : _dbActionService.GetArticleIdByGuidOrDefault(action.ResultUniqueId);
            return articleIdByGuid ?? action.ResultId;
        }
    }
}
