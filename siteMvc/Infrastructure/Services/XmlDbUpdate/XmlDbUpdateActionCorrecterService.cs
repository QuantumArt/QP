using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Script.Serialization;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Interfaces.Services;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Infrastructure.Extensions;
using Quantumart.QP8.WebMvc.Infrastructure.Helpers.XmlDbUpdate;
using Quantumart.QP8.WebMvc.Infrastructure.Models;
using Quantumart.QP8.WebMvc.Infrastructure.Services.XmlDbUpdate.Interfaces;

namespace Quantumart.QP8.WebMvc.Infrastructure.Services.XmlDbUpdate
{
    public class XmlDbUpdateActionCorrecterService : IXmlDbUpdateActionCorrecterService
    {
        private readonly Dictionary<string, Dictionary<int, int>> _idsToReplace = new Dictionary<string, Dictionary<int, int>>();
        private readonly Dictionary<string, Dictionary<Guid, Guid>> _uniqueIdsToReplace = new Dictionary<string, Dictionary<Guid, Guid>>();
        private readonly IArticleService _dbActionService;

        public XmlDbUpdateActionCorrecterService(IArticleService dbActionService)
        {
            _dbActionService = dbActionService;
        }

        public XmlDbUpdateRecordedAction PreActionCorrections(XmlDbUpdateRecordedAction action, bool useGuidSubstitution)
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
                    CorrectFieldForm(action, useGuidSubstitution);
                    break;
                case EntityTypeCode.Article:
                    CorrectArticleForm(action, useGuidSubstitution);
                    break;
                case EntityTypeCode.User:
                    CorrectUserForm(action, useGuidSubstitution);
                    break;
                case EntityTypeCode.UserGroup:
                    CorrectUserGroupForm(action);
                    break;
                case EntityTypeCode.Site:
                    CorrectSiteForm(action);
                    break;
                case EntityTypeCode.CustomAction:
                    CorrectCustomActionForm(action);
                    break;
                case EntityTypeCode.VisualEditorPlugin:
                    CorrectVePluginForm(action);
                    break;
                case EntityTypeCode.Workflow:
                    CorrectWorkflowForm(action);
                    break;
                case EntityTypeCode.PageObject:
                    CorrectObjectForm(action, true);
                    break;
                case EntityTypeCode.TemplateObject:
                    CorrectObjectForm(action, false);
                    break;
                case EntityTypeCode.Notification:
                    CorrectNotificationForm(action);
                    break;
            }

            return action;
        }

        public XmlDbUpdateRecordedAction PostActionCorrections(XmlDbUpdateRecordedAction action, HttpContextBase httpContext)
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
            // Guid substitution step-by-step:
            // 1. UniqueId Form Correction
            // 2. UniqueId Attribute Correction
            // 3. Result Guid -> Id Substitution
            // 4. Main Guids -> Ids Substitution
            // 5. Main Ids Correction
            // 6. Parent Id Correction

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

            action.UniqueId = action.UniqueId.Select(g => CorrectUniqueIdValue(EntityTypeCode.Article, g)).ToArray();
            return action;
        }

        private IEnumerable<string> CorrectIdsValue(string entityTypeCode, IEnumerable<string> ids)
        {
            return ids.Select(id => CorrectIdValue(entityTypeCode, id));
        }

        private string CorrectIdValue(string entityTypeCode, string value)
        {
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
            CorrectUniqueIdFormValues(action.Form, uniqueIdFieldName);
            return action;
        }

        private Guid CorrectUniqueIdValue(string entityTypeCode, Guid value)
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

        private void CorrectUserForm(XmlDbUpdateRecordedAction action, bool useGuidSubstitution)
        {
            CorrectFormValue(EntityTypeCode.UserGroup, action.Form, "SelectedGroups", true);
            CorrectFormValue(EntityTypeCode.Site, action.Form, "ContentDefaultFilter.SiteId");
            CorrectFormValue(EntityTypeCode.Content, action.Form, "ContentDefaultFilter.ContentId");

            CorrectAndSubstituteArticleIdFormValues(action.Form, "ContentDefaultFilter.ArticleIDs", "ContentDefaultFilter.ArticleUniqueIDs", useGuidSubstitution);
            CorrectFormValue(EntityTypeCode.Article, action.Form, "ContentDefaultFilter.ArticleIDs");
        }

        private void CorrectFieldForm(XmlDbUpdateRecordedAction action, bool useGuidSubstitution)
        {
            CorrectFormValue(EntityTypeCode.Field, action.Form, "Data.Id");
            CorrectFormValue(EntityTypeCode.Field, action.Form, "InCombinationWith", true);
            CorrectFormValue(EntityTypeCode.Content, action.Form, "Data.RelateToContentId");
            CorrectFormValue(EntityTypeCode.Field, action.Form, "Data.RelationId");
            CorrectFormValue(EntityTypeCode.Field, action.Form, "Data.BackRelationId");
            CorrectFormValue(EntityTypeCode.Field, action.Form, "Data.ClassifierId");
            CorrectFormValue(EntityTypeCode.Field, action.Form, "Data.BaseImageId");
            CorrectFormValue(EntityTypeCode.Field, action.Form, "Data.ListOrderFieldId");
            CorrectFormValue(EntityTypeCode.Field, action.Form, "Data.TreeOrderFieldId");
            CorrectFormValue(EntityTypeCode.ContentLink, action.Form, "Data.ContentLink.LinkId");

            CorrectAndSubstituteArticleIdFormValues(action.Form, "Data.O2MDefaultValue", "Data.O2MUniqueIdDefaultValue", useGuidSubstitution);
            CorrectFormValue(EntityTypeCode.Article, action.Form, "Data.O2MDefaultValue");

            CorrectAndSubstituteArticleIdFormValues(action.Form, "DefaultArticleIds", "DefaultArticleUniqueIds", useGuidSubstitution);
            CorrectFormValue(EntityTypeCode.Article, action.Form, "DefaultArticleIds", true);

            CorrectFormValue(EntityTypeCode.VisualEditorCommand, action.Form, "ActiveVeCommands", true);
            CorrectFormValue(EntityTypeCode.VisualEditorStyle, action.Form, "ActiveVeStyles", true);
            CorrectFormValue(EntityTypeCode.VisualEditorStyle, action.Form, "ActiveVeFormats", true);
        }

        private void CorrectSiteForm(XmlDbUpdateRecordedAction action)
        {
            CorrectFormValue(EntityTypeCode.VisualEditorCommand, action.Form, "ActiveVeCommands", true);
            CorrectFormValue(EntityTypeCode.VisualEditorStyle, action.Form, "ActiveVeStyles", true);
            CorrectFormValue(EntityTypeCode.VisualEditorStyle, action.Form, "ActiveVeFormats", true);
        }

        private void CorrectObjectForm(XmlDbUpdateRecordedAction action, bool isPageObject)
        {
            CorrectFormValue(EntityTypeCode.TemplateObject, action.Form, "Data.ParentObjectId");
            CorrectFormValue(isPageObject ? EntityTypeCode.PageObjectFormat : EntityTypeCode.TemplateObjectFormat, action.Form, "Data.DefaultFormatId");
            CorrectFormValue(EntityTypeCode.Content, action.Form, "Data.Container.ContentId");
            CorrectFormValue(EntityTypeCode.Content, action.Form, "Data.ContentForm.ContentId");
            CorrectFormValue(EntityTypeCode.Page, action.Form, "Data.ContentForm.ThankYouPageId");
            CorrectFormValue(EntityTypeCode.StatusType, action.Form, "ActiveStatusTypeIds", true);
        }

        private void CorrectCustomActionForm(XmlDbUpdateRecordedAction action)
        {
            CorrectFormValue(EntityTypeCode.Site, action.Form, "SelectedSiteIDs", true);
            CorrectFormValue(EntityTypeCode.Content, action.Form, "SelectedContentIDs", true);
            CorrectFormValue(EntityTypeCode.BackendAction, action.Form, "SelectedActions", true);
        }

        private void CorrectVePluginForm(XmlDbUpdateRecordedAction action)
        {
            CorrectFormValue(EntityTypeCode.VisualEditorCommand, action.Form, "AggregationListItemsVeCommandsDisplay", "Id");
        }

        private void CorrectWorkflowForm(XmlDbUpdateRecordedAction action)
        {
            CorrectFormValue(EntityTypeCode.StatusType, action.Form, "ActiveStatuses", true);
            CorrectFormValue(EntityTypeCode.Content, action.Form, "ActiveContentIds", true);
            CorrectFormValue(EntityTypeCode.WorkflowRule, action.Form, "WorkflowsWorkflowRulesDisplay", "Id");
            CorrectFormValue(EntityTypeCode.StatusType, action.Form, "WorkflowsWorkflowRulesDisplay", "StId");
            CorrectFormValue(EntityTypeCode.User, action.Form, "WorkflowsWorkflowRulesDisplay", "UserId");
            CorrectFormValue(EntityTypeCode.UserGroup, action.Form, "WorkflowsWorkflowRulesDisplay", "GroupId");
        }

        private void CorrectNotificationForm(XmlDbUpdateRecordedAction action)
        {
            CorrectFormValue(EntityTypeCode.TemplateObjectFormat, action.Form, "Data.FormatId");
            CorrectFormValue(EntityTypeCode.User, action.Form, "Data.UserId");
            CorrectFormValue(EntityTypeCode.UserGroup, action.Form, "Data.GroupId");
            CorrectFormValue(EntityTypeCode.Field, action.Form, "Data.EmailFieldId");
            CorrectFormValue(EntityTypeCode.User, action.Form, "Data.FromBackenduserId");
            CorrectFormValue(EntityTypeCode.StatusType, action.Form, "Data.NotifyOnStatusTypeId");
        }

        private void CorrectArticleForm(XmlDbUpdateRecordedAction action, bool useGuidSubstitution)
        {
            CorrectDynamicArticleFormGuidValues(action.Form, useGuidSubstitution);
            CorrectDynamicArticleFormIdValues(action.Form, action.ParentId);
        }

        private void CorrectDynamicArticleFormIdValues(NameValueCollection form, int contentId)
        {
            var relations = ReplayHelper.GetRelations(contentId);
            var classifiers = ReplayHelper.GetClasifiers(contentId);
            foreach (var fieldName in form.AllKeys.Where(field => Regex.IsMatch(field, @"^field_\d+$")))
            {
                int parsedFieldId;
                if (!int.TryParse(fieldName.Replace("field_", string.Empty), out parsedFieldId))
                {
                    continue;
                }

                var correctedFieldId = CorrectIdValue(EntityTypeCode.Field, parsedFieldId);
                if (relations.ContainsKey(correctedFieldId) || classifiers.ContainsKey(correctedFieldId))
                {
                    var entityTypeCode = relations.ContainsKey(correctedFieldId) ? EntityTypeCode.Article : EntityTypeCode.Content;
                    var formIds = form[fieldName]?.Split(',');
                    if (formIds != null)
                    {
                        var correctedIds = CorrectIdsValue(entityTypeCode, formIds);
                        form[$"field_{correctedFieldId}"] = string.Join(",", correctedIds);
                    }

                    if (correctedFieldId != parsedFieldId)
                    {
                        form.Remove($"field_{parsedFieldId}");
                    }
                }
            }
        }

        private void CorrectDynamicArticleFormGuidValues(NameValueCollection form, bool useGuidSubstitution)
        {
            foreach (var fieldName in form.AllKeys.Where(field => Regex.IsMatch(field, @"^field_uniqueid_\d+$")))
            {
                int parsedFieldId;
                if (!int.TryParse(fieldName.Replace("field_uniqueid_", string.Empty), out parsedFieldId))
                {
                    continue;
                }

                CorrectAndSubstituteArticleIdFormValues(form, $"field_{parsedFieldId}", $"field_uniqueid_{parsedFieldId}", useGuidSubstitution);
                form.Remove($"field_uniqueid_{parsedFieldId}");
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

        private void CorrectUniqueIdFormValues(NameValueCollection form, string formUniqueIdsKey)
        {
            var formUniqueIds = form[formUniqueIdsKey]?.Split(',');
            if (formUniqueIds != null)
            {
                var correctedUniqueIds = formUniqueIds.Select(g => CorrectUniqueIdValue(EntityTypeCode.Article, Guid.Parse(g)));
                form[formUniqueIdsKey] = string.Join(",", correctedUniqueIds);
            }
        }

        private void CorrectAndSubstituteArticleIdFormValues(NameValueCollection form, string formIdsKey, string formUniqueIdsKey, bool useGuidSubstitution)
        {
            CorrectUniqueIdFormValues(form, formUniqueIdsKey);
            if (useGuidSubstitution)
            {
                SubstituteArticleIdFormValuesFromGuids(form, formIdsKey, formUniqueIdsKey);
            }
        }

        private void SubstituteArticleIdFormValuesFromGuids(NameValueCollection form, string formIdKey, string formUniqueIdKey)
        {
            var formUniqueIds = form[formUniqueIdKey]?.Split(',');
            if (formUniqueIds != null)
            {
                var substitutedIds = formUniqueIds
                    .Select(_dbActionService.GetArticleIdByGuid)
                    .Select(id => id.ToString())
                    .ToArray();

                form[formIdKey] = string.Join(",", substitutedIds);
            }
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
