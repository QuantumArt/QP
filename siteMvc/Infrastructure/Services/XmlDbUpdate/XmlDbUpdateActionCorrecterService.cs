using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using QP8.Infrastructure;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services.ArticleServices;
using Quantumart.QP8.BLL.Services.ContentServices;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Constants.Mvc;
using Quantumart.QP8.WebMvc.Infrastructure.Constants;
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
        private readonly IContentService _dbContentService;
        private readonly ModelExpressionProvider _modelExpressionProvider;

        public XmlDbUpdateActionCorrecterService(IArticleService dbActionService, IContentService dbContentService, ModelExpressionProvider modelExpressionProvider)
        {
            _dbActionService = dbActionService;
            _dbContentService = dbContentService;
            _modelExpressionProvider = modelExpressionProvider;
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

        public XmlDbUpdateRecordedAction PostActionCorrections(XmlDbUpdateRecordedAction action, HttpContext httpContext)
        {
            AddResultIds(action, httpContext);
            switch (action.BackendAction.Code)
            {
                case ActionCode.AddNewVirtualContents:
                case ActionCode.VirtualContentProperties:
                case ActionCode.VirtualFieldProperties:
                    AddIdsToReplace(action.VirtualFieldIds, httpContext, HttpContextItems.NewVirtualFieldIds);
                    break;
                case ActionCode.AddNewContent:
                case ActionCode.CreateLikeContent:
                    AddIdsToReplace(action.ChildIds, httpContext, HttpContextItems.FieldIds);
                    AddIdsToReplace(action.ChildLinkIds, httpContext, HttpContextItems.LinkIds);
                    break;
                case ActionCode.AddNewField:
                case ActionCode.FieldProperties:
                case ActionCode.CreateLikeField:
                    AddIdToReplace(EntityTypeCode.ContentLink, action.ChildId, httpContext, HttpContextItems.NewLinkId);
                    AddIdToReplace(EntityTypeCode.Field, action.BackwardId, httpContext, HttpContextItems.NewBackwardId);
                    AddIdsToReplace(action.VirtualFieldIds, httpContext, HttpContextItems.NewVirtualFieldIds);
                    AddIdsToReplace(action.ChildIds, httpContext, HttpContextItems.NewChildFieldIds);
                    AddIdsToReplace(action.ChildLinkIds, httpContext, HttpContextItems.NewChildLinkIds);
                    break;
                case ActionCode.AddNewCustomAction:
                    AddIdToReplace(EntityTypeCode.BackendAction, action.ChildId, httpContext, HttpContextItems.ActionId);
                    break;
                case ActionCode.AddNewVisualEditorPlugin:
                case ActionCode.VisualEditorPluginProperties:
                    AddIdsToReplace(action.ChildIds, httpContext, HttpContextItems.NewCommandIds);
                    break;
                case ActionCode.AddNewWorkflow:
                case ActionCode.WorkflowProperties:
                    AddIdsToReplace(action.ChildIds, httpContext, HttpContextItems.NewRulesIds);
                    break;
                case ActionCode.AddNewNotification:
                case ActionCode.NotificationProperties:
                    AddIdToReplace(EntityTypeCode.TemplateObjectFormat, action.ChildId, httpContext, HttpContextItems.NotificationFormatId);
                    break;
                case ActionCode.AddNewPageObject:
                    AddIdToReplace(EntityTypeCode.PageObjectFormat, action.ChildId, httpContext, HttpContextItems.DefaultFormatId);
                    break;
                case ActionCode.AddNewTemplateObject:
                    AddIdToReplace(EntityTypeCode.TemplateObjectFormat, action.ChildId, httpContext, HttpContextItems.DefaultFormatId);
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
            }

            var entityTypeCode = action.BackendAction.EntityType.Code == EntityTypeCode.ArchiveArticle
                ? EntityTypeCode.Article
                : action.BackendAction.EntityType.Code;

            entityTypeCode = action.BackendAction.EntityType.Code == EntityTypeCode.VirtualContent
                ? EntityTypeCode.Content
                : entityTypeCode;

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

        private string CorrectIdValue(string entityTypeCode, string value) => int.TryParse(value, out var result) ? CorrectIdValue(entityTypeCode, result).ToString() : value;

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
            var uniqueIdFieldName = XmlDbUpdateQpActionHelpers.GetFieldName(
                _modelExpressionProvider, vm => vm.Data.UniqueId
            );
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
            CorrectFormValue(EntityTypeCode.Workflow, action.Form, HttpContextFormConstants.DataWorkflowBindingWorkflowId);
            CorrectFormValue(EntityTypeCode.Content, action.Form, HttpContextFormConstants.DataParentContentId);
            CorrectFormValue(EntityTypeCode.ContentGroup, action.Form, HttpContextFormConstants.DataGroupId);
            CorrectXamlValidation(action.Form, int.Parse(action.Ids[0]));
        }

        private void CorrectVirtualContentForm(XmlDbUpdateRecordedAction action)
        {
            CorrectFormValue(EntityTypeCode.Content, action.Form, HttpContextFormConstants.DataJoinRootId);
            CorrectFormValue(EntityTypeCode.Content, action.Form, HttpContextFormConstants.DataUnionSourceContentIds);
            CorrectFormValue(EntityTypeCode.Field, action.Form, HttpContextFormConstants.JoinFields, true, true);
            CorrectFormValue(EntityTypeCode.Field, action.Form, HttpContextFormConstants.ItemValue, true, true);
        }

        private void CorrectUserGroupForm(XmlDbUpdateRecordedAction action)
        {
            CorrectFormValue(EntityTypeCode.User, action.Form, HttpContextFormConstants.BindedUserIds, true);
            CorrectFormValue(EntityTypeCode.UserGroup, action.Form, HttpContextFormConstants.ParentGroupId);
        }

        private void CorrectUserForm(XmlDbUpdateRecordedAction action, bool useGuidSubstitution)
        {
            CorrectFormValue(EntityTypeCode.UserGroup, action.Form, HttpContextFormConstants.SelectedGroups, true);
            CorrectFormValue(EntityTypeCode.Site, action.Form, HttpContextFormConstants.ContentDefaultFilterSiteId);
            CorrectFormValue(EntityTypeCode.Content, action.Form, HttpContextFormConstants.ContentDefaultFilterContentId);

            CorrectAndSubstituteArticleIdFormValues(action.Form, HttpContextFormConstants.ContentDefaultFilterArticleIds, HttpContextFormConstants.ContentDefaultFilterArticleUniqueIds, useGuidSubstitution);
            CorrectFormValue(EntityTypeCode.Article, action.Form, HttpContextFormConstants.ContentDefaultFilterArticleIds);
        }

        private void CorrectFieldForm(XmlDbUpdateRecordedAction action, bool useGuidSubstitution)
        {
            CorrectFormValue(EntityTypeCode.Field, action.Form, HttpContextFormConstants.DataId);
            CorrectFormValue(EntityTypeCode.Field, action.Form, HttpContextFormConstants.InCombinationWith, true);
            CorrectFormValue(EntityTypeCode.Content, action.Form, HttpContextFormConstants.DataRelateToContentId);
            CorrectFormValue(EntityTypeCode.Field, action.Form, HttpContextFormConstants.DataRelationId);
            CorrectFormValue(EntityTypeCode.Field, action.Form, HttpContextFormConstants.DataBackRelationId);
            CorrectFormValue(EntityTypeCode.Field, action.Form, HttpContextFormConstants.DataClassifierId);
            CorrectFormValue(EntityTypeCode.Field, action.Form, HttpContextFormConstants.DataBaseImageId);
            CorrectFormValue(EntityTypeCode.Field, action.Form, HttpContextFormConstants.DataListOrderFieldId);
            CorrectFormValue(EntityTypeCode.Field, action.Form, HttpContextFormConstants.DataTreeOrderFieldId);
            CorrectFormValue(EntityTypeCode.ContentLink, action.Form, HttpContextFormConstants.DataContentLinkLinkId);

            CorrectAndSubstituteArticleIdFormValues(action.Form, HttpContextFormConstants.DataO2MDefaultValue, HttpContextFormConstants.DataO2MUniqueIdDefaultValue, useGuidSubstitution);
            CorrectFormValue(EntityTypeCode.Article, action.Form, HttpContextFormConstants.DataO2MDefaultValue);

            CorrectAndSubstituteArticleIdFormValues(action.Form, HttpContextFormConstants.DefaultArticleIds, HttpContextFormConstants.DefaultArticleUniqueIds, useGuidSubstitution);
            CorrectFormValue(EntityTypeCode.Article, action.Form, HttpContextFormConstants.DefaultArticleIds, true);

            CorrectFormValue(EntityTypeCode.VisualEditorCommand, action.Form, HttpContextFormConstants.ActiveVeCommands, true);
            CorrectFormValue(EntityTypeCode.VisualEditorStyle, action.Form, HttpContextFormConstants.ActiveVeStyles, true);
            CorrectFormValue(EntityTypeCode.VisualEditorStyle, action.Form, HttpContextFormConstants.ActiveVeFormats, true);
        }

        private void CorrectSiteForm(XmlDbUpdateRecordedAction action)
        {
            CorrectFormValue(EntityTypeCode.VisualEditorCommand, action.Form, HttpContextFormConstants.ActiveVeCommands, true);
            CorrectFormValue(EntityTypeCode.VisualEditorStyle, action.Form, HttpContextFormConstants.ActiveVeStyles, true);
            CorrectFormValue(EntityTypeCode.VisualEditorStyle, action.Form, HttpContextFormConstants.ActiveVeFormats, true);
        }

        private void CorrectObjectForm(XmlDbUpdateRecordedAction action, bool isPageObject)
        {
            CorrectFormValue(EntityTypeCode.TemplateObject, action.Form, HttpContextFormConstants.DataParentObjectId);
            CorrectFormValue(isPageObject ? EntityTypeCode.PageObjectFormat : EntityTypeCode.TemplateObjectFormat, action.Form, HttpContextFormConstants.DataDefaultFormatId);
            CorrectFormValue(EntityTypeCode.Content, action.Form, HttpContextFormConstants.DataContainerContentId);
            CorrectFormValue(EntityTypeCode.Content, action.Form, HttpContextFormConstants.DataContentFormContentId);
            CorrectFormValue(EntityTypeCode.Page, action.Form, HttpContextFormConstants.DataContentFormThankYouPageId);
            CorrectFormValue(EntityTypeCode.StatusType, action.Form, HttpContextFormConstants.ActiveStatusTypeIds, true);
        }

        private void CorrectCustomActionForm(XmlDbUpdateRecordedAction action)
        {
            CorrectFormValue(EntityTypeCode.Site, action.Form, HttpContextFormConstants.SelectedSiteIDs, true);
            CorrectFormValue(EntityTypeCode.Content, action.Form, HttpContextFormConstants.SelectedContentIDs, true);
            CorrectFormValue(EntityTypeCode.BackendAction, action.Form, HttpContextFormConstants.SelectedActions, true);
        }

        private void CorrectVePluginForm(XmlDbUpdateRecordedAction action)
        {
            CorrectFormValue(EntityTypeCode.VisualEditorCommand, action.Form, HttpContextFormConstants.AggregationListItemsVeCommandsDisplay, "Id");
        }

        private void CorrectWorkflowForm(XmlDbUpdateRecordedAction action)
        {
            CorrectFormValue(EntityTypeCode.StatusType, action.Form, HttpContextFormConstants.ActiveStatuses, true);
            CorrectFormValue(EntityTypeCode.Content, action.Form, HttpContextFormConstants.ActiveContentIds, true);
            CorrectFormValue(EntityTypeCode.WorkflowRule, action.Form, HttpContextFormConstants.WorkflowsWorkflowRulesDisplay, "Id");
            CorrectFormValue(EntityTypeCode.StatusType, action.Form, HttpContextFormConstants.WorkflowsWorkflowRulesDisplay, "StId");
            CorrectFormValue(EntityTypeCode.User, action.Form, HttpContextFormConstants.WorkflowsWorkflowRulesDisplay, "UserId");
            CorrectFormValue(EntityTypeCode.UserGroup, action.Form, HttpContextFormConstants.WorkflowsWorkflowRulesDisplay, "GroupId");
        }

        private void CorrectNotificationForm(XmlDbUpdateRecordedAction action)
        {
            CorrectFormValue(EntityTypeCode.TemplateObjectFormat, action.Form, HttpContextFormConstants.DataFormatId);
            CorrectFormValue(EntityTypeCode.User, action.Form, HttpContextFormConstants.DataUserId);
            CorrectFormValue(EntityTypeCode.UserGroup, action.Form, HttpContextFormConstants.DataGroupId);
            CorrectFormValue(EntityTypeCode.Field, action.Form, HttpContextFormConstants.DataEmailFieldId);
            CorrectFormValue(EntityTypeCode.User, action.Form, HttpContextFormConstants.DataFromBackenduserId);
            CorrectFormValue(EntityTypeCode.StatusType, action.Form, HttpContextFormConstants.DataNotifyOnStatusTypeId);
        }

        private void CorrectArticleForm(XmlDbUpdateRecordedAction action, bool useGuidSubstitution)
        {
            CorrectDynamicArticleFormGuidValues(action.Form, useGuidSubstitution);
            CorrectDynamicArticleFormIdValues(action.Form, action.ParentId);
        }

        private void CorrectDynamicArticleFormIdValues(Dictionary<string, StringValues> form, int contentId)
        {
            var fieldRegexp = new Regex(@"^field_\d+$", RegexOptions.Compiled);
            var fieldNames = form.Keys.Where(field => fieldRegexp.IsMatch(field)).ToArray();
            var correctedContentId = CorrectIdValue(EntityTypeCode.Content, contentId);
            var content = _dbContentService.Get(correctedContentId);
            var fields = (content.AggregatedContents.Any()
                ? content.Fields.Union(content.AggregatedContents.SelectMany(s => s.Fields))
                : content.Fields).ToDictionary(n => n.Id, n => n);

            foreach (var fieldName in fieldNames)
            {
                var fieldId = fieldName.Replace("field_", string.Empty);
                if (!int.TryParse(fieldId, out var parsedFieldId) || !form.ContainsKey(fieldName))
                {
                    continue;
                }
                var correctedFieldId = CorrectIdValue(EntityTypeCode.Field, parsedFieldId);

                var fieldValues = ((string)form[fieldName]).Split(',').ToList();
                form.Remove(fieldName);

                if (fieldValues.Any())
                {
                    if (fields.TryGetValue(correctedFieldId, out var field))
                    {
                        switch (field.ExactType)
                        {
                            case FieldExactTypes.O2MRelation:
                            case FieldExactTypes.M2MRelation:
                            case FieldExactTypes.M2ORelation:
                                fieldValues = CorrectIdsValue(EntityTypeCode.Article, fieldValues).ToList();
                                break;
                            case FieldExactTypes.Classifier:
                                fieldValues = CorrectIdsValue(EntityTypeCode.Content, fieldValues).ToList();
                                break;
                        }

                        if (content.NetName == "QPDiscriminator" && field.LinqPropertyName == "PreferredContentId")
                        {
                            fieldValues = CorrectIdsValue(EntityTypeCode.Content, fieldValues).ToList();
                        }
                    }
                    form[$"field_{correctedFieldId}"] = new StringValues(fieldValues.ToArray());
                }
            }
        }

        private void CorrectDynamicArticleFormGuidValues(Dictionary<string, StringValues> form, bool useGuidSubstitution)
        {
            var uniqueFieldRegexp = new Regex(@"^field_uniqueid_\d+$", RegexOptions.Compiled);
            var fieldNames = form.Keys.Where(field => uniqueFieldRegexp.IsMatch(field)).ToArray();
            foreach (var fieldName in fieldNames)
            {
                if (!int.TryParse(fieldName.Replace("field_uniqueid_", string.Empty), out var parsedFieldId))
                {
                    continue;
                }

                CorrectAndSubstituteArticleIdFormValues(form, $"field_{parsedFieldId}", $"field_uniqueid_{parsedFieldId}", useGuidSubstitution);
                form.Remove($"field_uniqueid_{parsedFieldId}");
            }
        }

        private void CorrectXamlValidation(Dictionary<string, StringValues> form, int contentId)
        {
            if (contentId == 0)
            {
                return;
            }
            const string key = "Data.XamlValidation";
            if (form.TryGetValue(key, out var value))
            {
                var xaml = (string)value;
                if (!string.IsNullOrWhiteSpace(xaml))
                {
                    var correctedContentId = CorrectIdValue(EntityTypeCode.Content, contentId);
                    var content = _dbContentService.Get(correctedContentId);
                    if (content == null)
                    {
                        return;
                    }

                    var fields = content.AggregatedContents.Any()
                        ? content.Fields.Union(content.AggregatedContents.SelectMany(s => s.Fields))
                        : content.Fields;

                    var fieldMapping = fields
                        .Select(n => new { Key = n.Id, Value = CorrectIdValue(EntityTypeCode.Field, n.Id) })
                        .Where(n => n.Key != n.Value)
                        .ToList();

                    foreach (var map in fieldMapping)
                    {
                        xaml = xaml.Replace($"field_{map.Key}", $"field_{map.Value}");
                    }

                    form[key] = new StringValues(xaml);
                }
            }
        }


        private void CorrectFormValue(string entityTypeCode, Dictionary<string, StringValues> form, string formKey, string jsonKey)
        {
            if (!form.TryGetValue(formKey, out var formValue))
            {
                return;
            }

            var collectionList = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(formValue);
            foreach (var collection in collectionList.Where(collection => collection.ContainsKey(jsonKey)))
            {
                collection[jsonKey] = CorrectIdValue(entityTypeCode, collection[jsonKey]);
            }

            form[formKey] = JsonConvert.SerializeObject(collectionList);
        }

        private void CorrectFormValue(string entityTypeCode, Dictionary<string, StringValues> form, string formKey, bool prefixSearch = false, bool joinModeReplace = false)
        {
            if (!prefixSearch)
            {
                if (!form.TryGetValue(formKey, out var formValue))
                {
                    return;
                }

                if (!string.IsNullOrEmpty(formValue))
                {
                    form[formKey] = CorrectIdValue(entityTypeCode, formValue);
                }
            }
            else
            {
                var keys = form.Keys.Where(n => n.StartsWith(formKey)).ToArray();
                foreach (var key in keys)
                {
                    if (joinModeReplace && key.EndsWith("Index") || !form.ContainsKey(key))
                    {
                        continue;
                    }

                    var values = (string[])form[key];
                    form.Remove(key);

                    var newValues = new List<string>(values.Length);
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

                        newValues.Add(CorrectIdValue(entityTypeCode, newValue));
                    }

                    form.Add(key, new StringValues(newValues.ToArray()));
                }
            }
        }

        private void CorrectUniqueIdFormValues(Dictionary<string, StringValues> form, string formUniqueIdsKey)
        {
            if (!form.TryGetValue(formUniqueIdsKey, out var formValue))
            {
                return;
            }
            var formUniqueIds = ((string)formValue).Split(',');
            if (formUniqueIds != null)
            {
                var correctedUniqueIds = formUniqueIds
                    .Select(g => CorrectUniqueIdValue(EntityTypeCode.Article, Guid.Parse(g)))
                    .Select(n => n.ToString()).ToArray();
                form[formUniqueIdsKey] = new StringValues(correctedUniqueIds);
            }
        }

        private void CorrectAndSubstituteArticleIdFormValues(Dictionary<string, StringValues> form, string formIdsKey, string formUniqueIdsKey, bool useGuidSubstitution)
        {
            CorrectUniqueIdFormValues(form, formUniqueIdsKey);
            if (useGuidSubstitution)
            {
                SubstituteArticleIdFormValuesFromGuids(form, formIdsKey, formUniqueIdsKey);
            }
        }

        private void SubstituteArticleIdFormValuesFromGuids(Dictionary<string, StringValues> form, string formIdKey, string formUniqueIdKey)
        {
            if (!form.TryGetValue(formUniqueIdKey, out var formValue))
            {
                return;
            }
            var formUniqueIds = ((string)formValue).Split(',');
            if (formUniqueIds != null)
            {
                var substitutedIds = formUniqueIds
                    .Select(_dbActionService.GetArticleIdByGuid)
                    .Select(id => id.ToString())
                    .ToArray();

                form[formIdKey] = new StringValues(substitutedIds);
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
                action.Ids = _dbActionService.GetArticleIdsByGuids(action.UniqueId)
                    .Select(g => g.ToString())
                    .ToArray();
            }

            return action;
        }

        private void AddIdsToReplace(string oldIdsCommaString, HttpContext context, string key)
        {
            if (context.Items.ContainsKey(key))
            {
                IReadOnlyList<int> oldIds = oldIdsCommaString?.ToIntArray();
                IReadOnlyList<int> newIds = context.Items[key]?.ToString().ToIntArray();
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

        private void AddIdToReplace(string entityTypeCode, int id, HttpContext context, string key)
        {
            if (context.Items.ContainsKey(key))
            {
                AddIdToReplace(entityTypeCode, id, int.Parse(context.Items[key].ToString()));
            }
        }

        private void AddResultIds(XmlDbUpdateRecordedAction action, HttpContext httpContext)
        {
            if (XmlDbUpdateQpActionHelpers.IsActionHasResultId(action.Code))
            {
                var entityTypeCode = action.BackendAction.EntityType.Code != EntityTypeCode.VirtualContent ? action.BackendAction.EntityType.Code : EntityTypeCode.Content;
                var resultId = action.ResultId != default(int) ? action.ResultId : int.Parse(action.Ids.First());
                AddIdToReplace(entityTypeCode, resultId, httpContext, HttpContextItems.ResultId);

                var resultUniqueId = action.ResultUniqueId != Guid.Empty ? action.ResultUniqueId : action.UniqueId.First();
                AddUniqueIdToReplace(entityTypeCode, resultUniqueId, httpContext, HttpContextItems.ResultGuid);
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

        private void AddUniqueIdToReplace(string entityTypeCode, Guid uniqueId, HttpContext context, string key)
        {
            if (context.Items.ContainsKey(key))
            {
                AddUniqueIdToReplace(entityTypeCode, uniqueId, Guid.Parse(context.Items[key].ToString()));
            }
        }

        private int GetArticleResultIdByGuidOrDefault(XmlDbUpdateRecordedAction action)
        {
            var articleIdByGuid = action.ResultUniqueId == Guid.Empty ? 0 : _dbActionService.GetArticleIdByGuidOrDefault(action.ResultUniqueId);
            return articleIdByGuid == 0 ? action.ResultId : articleIdByGuid;
        }
    }
}
