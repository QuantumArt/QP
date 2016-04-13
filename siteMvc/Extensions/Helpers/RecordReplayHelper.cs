using Moq;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.Configuration;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Security;
using Quantumart.QP8.Utils;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.Extensions.Helpers.API;
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
using System.Web.Script.Serialization;
using System.Xml.Linq;
#pragma warning disable 169

namespace Quantumart.QP8.WebMvc.Extensions.Helpers
{
    public interface IRecordHelper
    {
        void PersistAction(RecordedAction action, HttpContextBase context);
    }

    public interface IReplayHelper
    {
        void ReplayActionsFromXml(string xmlText, int userId, XDocument fingerPrintSettings);
    }

    public class RecordedAction
    {
        public RecordedAction()
        {
            _backendAction = new InitPropertyValue<BackendAction>(() => BackendActionService.GetByCode(Code));
            ChildActions = new List<RecordedAction>();
        }

        public string[] Ids { get; set; }

        public int ParentId { get; set; }

        public int ResultId { get; set; }

        public int ChildId { get; set; }

        public string ChildIds { get; set; }

        public string ChildLinkIds { get; set; }

        public string VirtualFieldIds { get; set; }

        public int BackwardId { get; set; }

        public int Lcid { get; set; }

        public DateTime Executed { get; set; }

        public string ExecutedBy { get; set; }

        public string Code { get; set; }

        public string CustomActionCode { get; set; }

        public NameValueCollection Form { get; set; }

        public List<RecordedAction> ChildActions { get; set; }

        private readonly InitPropertyValue<BackendAction> _backendAction;

        public BackendAction BackendAction => _backendAction.Value;
    }

    public class RecordReplayHelper: IRecordHelper, IReplayHelper
    {
        public RecordReplayHelper()
        {
            _xmlFileName = new InitPropertyValue<string>(GetXmlFileName);
        }

        private InitPropertyValue<XElement> _storage;
        private readonly InitPropertyValue<string> _xmlFileName;
        private readonly RouteCollection _routes = new RouteCollection();
        private readonly Dictionary<string, Dictionary<int, int>> _idsToReplace = new Dictionary<string, Dictionary<int, int>>();
        private byte[] _fingerPrint;

        #region record
        public string XmlFileName => _xmlFileName.Value;

        private static string GetXmlFileName()
        {
            return $@"{QPConfiguration.TempDirectory}\{QPContext.CurrentCustomerCode}.xml";
        }

        public XElement GetOrCreateRoot(HttpContextBase http)
        {
            XDocument doc;
            try
            {
                doc = XDocument.Load(XmlFileName);
            }
            catch (FileNotFoundException)
            {
                doc = new XDocument
                (
                    NewRoot(http)
                );
            }

            return GetActionsRoot(doc);
        }

        private XElement NewRoot(HttpContextBase http)
        {
            var xml = DbRepository.Get().FpSettingsXml;
            return new XElement(
                "actions",
                GetBackendUrlAttribute(http),
                new XAttribute("dbVersion", new ApplicationInfoHelper().GetCurrentDBVersion()),
                GetFpAttribute(xml),
                GetFpSettingsElement(xml)
            );
        }

        public void Clear(HttpContextBase http, bool overrideFile)
        {
            var root = GetOrCreateRoot(http);
            var doc = root.Document;
            if (doc != null)
            {
                if (root.HasElements && overrideFile)
                {
                    root.Remove();
                    doc.Add(NewRoot(http));
                }
                doc.Save(XmlFileName);
            }

        }

        private XAttribute GetFpAttribute(XDocument settingsXml)
        {
            if (settingsXml == null)
                return null;
            return new XAttribute("fingerPrint", ByteArrayToString(
                new FingerprintService().GetFingerprint(settingsXml)
                ));
        }

        private XAttribute GetBackendUrlAttribute(HttpContextBase http)
        {
            return new XAttribute("backendUrl", QPController.GetBackendUrl(http));
        }

        private XElement GetFpSettingsElement(XDocument settingsXml)
        {
            if (settingsXml == null)
                return null;
            else
                return new XElement("settings", settingsXml.Root);
        }

        public XElement GetActionsRoot(XDocument doc)
        {
            var root = doc.Elements("actions").SingleOrDefault();
            if (root == null)
                throw new InvalidDataException($"File {XmlFileName} has invalid format");

            return root;
        }

        public IEnumerable<XElement> FormToXml(NameValueCollection form)
        {
            if (form == null)
                yield break;
            foreach (var key in form.AllKeys)
            {
                var enumerable = form.GetValues(key);
                if (enumerable != null)
                    foreach (var value in enumerable)
                        yield return new XElement("field",
                            new XAttribute("name", key),
                            new XText(value)
                            );
            }
        }

        private void AppendAttribute(List<XAttribute> result, string name, HttpContextBase context, string key)
        {
            if (context.Items.Contains(key))
            {
                result.Add(new XAttribute(name, context.Items[key].ToString()));
            }
        }

        public IEnumerable<XAttribute> GetEntitySpecificAttributesForPersisting(RecordedAction action, HttpContextBase context)
        {
            var actionTypeCode = action.BackendAction.ActionType.Code;
            var result = new List<XAttribute>();

            if (actionTypeCode == ActionTypeCode.Copy)
            {
                AppendAttribute(result, "resultId", context, "RESULT_ID");
            }

            switch (action.Code)
            {
                case ActionCode.AddNewVirtualContents:
                case ActionCode.VirtualContentProperties:
                case ActionCode.VirtualFieldProperties:
                    AppendAttribute(result, "newVirtualFieldIds", context, "NEW_VIRTUAL_FIELD_IDS");
                    break;
                case ActionCode.AddNewContent:
                case ActionCode.CreateLikeContent:
                    AppendAttribute(result, "fieldIds", context, "FIELD_IDS");
                    AppendAttribute(result, "linkIds", context, "LINK_IDS");
                    break;
                case ActionCode.AddNewField:
                case ActionCode.FieldProperties:
                case ActionCode.CreateLikeField:
                    AppendAttribute(result, "newVirtualFieldIds", context, "NEW_VIRTUAL_FIELD_IDS");
                    AppendAttribute(result, "newLinkId", context, "NEW_LINK_ID");
                    AppendAttribute(result, "newBackwardId", context, "NEW_BACKWARD_ID");
                    AppendAttribute(result, "newChildFieldIds", context, "NEW_CHILD_FIELD_IDS");
                    AppendAttribute(result, "newChildLinkIds", context, "NEW_CHILD_LINK_IDS");
                    break;
                case ActionCode.AddNewCustomAction:
                    AppendAttribute(result, "actionId", context, "ACTION_ID");
                    AppendAttribute(result, "actionCode", context, "ACTION_CODE");
                    break;
                case ActionCode.AddNewVisualEditorPlugin:
                case ActionCode.VisualEditorPluginProperties:
                    AppendAttribute(result, "commandIds", context, "NEW_COMMAND_IDS");
                    break;
                case ActionCode.AddNewWorkflow:
                case ActionCode.WorkflowProperties:
                    AppendAttribute(result, "rulesIds", context, "NEW_RULES_IDS");
                    break;
                case ActionCode.AddNewNotification:
                case ActionCode.NotificationProperties:
                    AppendAttribute(result, "formatId", context, "NOTIFICATION_FORMAT_ID");
                    break;
                case ActionCode.AddNewPageObject:
                case ActionCode.AddNewTemplateObject:
                    AppendAttribute(result, "formatId", context, "DEFAULT_FORMAT_ID");
                    break;
            }
            return result;
        }

        public void PersistAction(RecordedAction action, HttpContextBase context)
        {

            var root = GetOrCreateRoot(context);

            root.Add(
                new XElement("action",
                    new XAttribute("code", action.Code),
                    new XAttribute("ids", string.Join(",", action.Ids)),
                    new XAttribute("parentId", action.ParentId),
                    new XAttribute("lcid", action.Lcid),
                    new XAttribute("executed", action.Executed.ToString(CultureInfo.InvariantCulture)),
                    new XAttribute("executedBy", action.ExecutedBy),
                    GetEntitySpecificAttributesForPersisting(action, context),
                    FormToXml(action.Form)
                )
            );

            root.Document?.Save(XmlFileName);

        }

        #endregion

        #region replay

        private void AddIdToReplace(string code, int id, HttpContextBase context, string key)
        {
            if (context.Items.Contains(key))
                AddIdToReplace(code, id, int.Parse(context.Items[key].ToString()));
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
                    _idsToReplace[code].Add(id, newId);
            }
        }

        private void CorrectAction(RecordedAction action)
        {
            var code = action.BackendAction.EntityType.Code;
            var parentCode = action.BackendAction.EntityType.ParentCode;
            action.Ids = CorrectListValue(code, action.Ids).ToArray();
            if (!string.IsNullOrEmpty(parentCode))
                action.ParentId = CorrectValue(parentCode, action.ParentId);
            switch (code)
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
        }

        private void CorrectContentForm(RecordedAction action)
        {
            CorrectFormValue(EntityTypeCode.Workflow, action.Form, "Data.WorkflowBinding.WorkflowId");
            CorrectFormValue(EntityTypeCode.Content, action.Form, "Data.ParentContentId");
            CorrectFormValue(EntityTypeCode.ContentGroup, action.Form, "Data.GroupId");
        }

        private void CorrectVirtualContentForm(RecordedAction action)
        {
            CorrectFormValue(EntityTypeCode.Content, action.Form, "Data.JoinRootId");
            CorrectFormValue(EntityTypeCode.Content, action.Form, "Data.UnionSourceContentIDs");
            CorrectFormValue(EntityTypeCode.Field, action.Form, "JoinFields", true, true);
            CorrectFormValue(EntityTypeCode.Field, action.Form, "itemValue", true, true);
        }

        private void CorrectUserGroupForm(RecordedAction action)
        {
            CorrectFormValue(EntityTypeCode.User, action.Form, "BindedUserIDs", true);
            CorrectFormValue(EntityTypeCode.UserGroup, action.Form, "ParentGroupId");
        }

        private void CorrectUserForm(RecordedAction action)
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
            CorrectFormValue((isPageObject) ? EntityTypeCode.PageObjectFormat : EntityTypeCode.TemplateObjectFormat, form, "Data.DefaultFormatId");
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
            var fields = ReplayHelper.GetRelations(contentId);
            foreach (var key in form.AllKeys.Where(n => n.StartsWith("field_")))
            {
                int result;
                var parsed = int.TryParse(key.Replace("field_", ""), out result);
                if (parsed)
                {
                    var newFieldId = CorrectValue(EntityTypeCode.Field, result);
                    if (newFieldId != result)
                    {
                        var newKey = "field_" + newFieldId;
                        var enumerable = form.GetValues(key);
                        if (enumerable != null)
                            foreach (var value in enumerable)
                            {
                                var newValue = (fields.ContainsKey(newFieldId)) ? CorrectCommaListValue(EntityTypeCode.Article, value) : value;
                                form.Add(newKey, newValue);
                            }
                        form.Remove(key);
                    }
                    else
                    {
                        if (fields.ContainsKey(newFieldId))
                        {
                            var values = form.GetValues(key);
                            form.Remove(key);
                            if (values != null)
                                foreach (var value in values)
                                {
                                    form.Add(key, CorrectCommaListValue(EntityTypeCode.Article, value));
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
            return (parsed) ? CorrectValue(code, result).ToString() : value;
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
                            foreach (var value in values)
                            {
                                string newValue;
                                if (!joinModeReplace)
                                {
                                    newValue = CorrectValue(code, value);
                                }
                                else
                                {
                                    newValue = string.Join(".", value
                                        .Replace("[", "")
                                        .Replace("]", "")
                                        .Split(".".ToCharArray())
                                        .Select(n => CorrectValue(code, n))
                                        );
                                    newValue = "[" + newValue + "]";
                                }
                                form.Add(key, CorrectValue(code, newValue));
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
                foreach (var collection in collectionList)
                {
                    if (collection.ContainsKey(jsonKey))
                    {
                        collection[jsonKey] = CorrectValue(code, collection[jsonKey]);
                    }
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
                var finalValue = (parsed) ? CorrectValue(code, parsedValue).ToString() : value;
                yield return finalValue;
            }
        }


        private int CorrectValue(string code, int value)
        {
            if (_idsToReplace.ContainsKey(code) && _idsToReplace[code].ContainsKey(value))
            {
                return _idsToReplace[code][value];
            }
            else
            {
                return value;
            }
        }

        private void CorrectReplaces(RecordedAction action, HttpContextBase context)
        {
            var actionTypeCode = action.BackendAction.ActionType.Code;
            var entityTypeCode = action.BackendAction.EntityType.Code;

            if (new[] { ActionTypeCode.AddNew, ActionTypeCode.Copy }.Contains(actionTypeCode))
            {
                var resultCode = (entityTypeCode != EntityTypeCode.VirtualContent) ? entityTypeCode : EntityTypeCode.Content;
                var resultId = (action.ResultId != 0) ? action.ResultId : int.Parse(action.Ids[0]);
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

        }

        public static int[] ToIntArray(string input)
        {
            if (string.IsNullOrEmpty(input))
                return null;
            else
                return input.Split(",".ToCharArray()).Select(int.Parse).ToArray();
        }

        private void AddIdsToReplace(string oldIdsCommaString, HttpContextBase context, string key)
        {
            if (context.Items.Contains(key))
                AddIdsToReplace(EntityTypeCode.Field, ToIntArray(oldIdsCommaString), ToIntArray(context.Items[key].ToString()));
        }

        private void AddIdsToReplace(string code, int[] oldIds, int[] newIds)
        {
            if (oldIds == null || newIds == null)
                return;
            if (oldIds.Length != newIds.Length)
                throw new ArgumentException("Array leghths are not equal");
            for (var i = 0; i < oldIds.Length; i++)
            {
                AddIdToReplace(code, oldIds[i], newIds[i]);
            }
        }

        public NameValueCollection FormFromXml(XElement root)
        {
            var result = new NameValueCollection();
            foreach (var elem in root.Elements())
            {
                result.Add(elem.Attribute("name").Value, elem.Value);
            }
            return result;
        }


        public void ReplayActionsFromXml(string xmlText, int userId, XDocument fingerPrintSettings)
        {
            var doc = XDocument.Parse(xmlText);
            if (!CheckFp(doc, fingerPrintSettings))
                throw new InvalidOperationException("Fingerprints doesn't match");
            if (!CheckDbVersion(doc))
                throw new InvalidOperationException("DB versions doesn't match");
            if (RecordActions())
                throw new InvalidOperationException("Replaying actions cannot be proceeded on the database which has recording option on");

            SetUp();
            foreach (var action in LoadAllActionsToReplay(doc))
            {
                try
                {
                    ReplayForm(action, userId, doc.Root?.Attribute("backendUrl"));
                }
                catch(Exception ex)
                {
                    throw new ReplayXmlException(action, ex);
                }
            }
            TearDown();
        }

        private bool RecordActions()
        {
            return new ApplicationInfoHelper().RecordActions();
        }

        private bool CheckDbVersion(XDocument doc)
        {
            var el = GetActionsRoot(doc);
            return el.Attribute("dbVersion") == null || el.Attribute("dbVersion").Value == new ApplicationInfoHelper().GetCurrentDBVersion();
        }

        private bool CheckFp(XDocument doc, XDocument fpSettings)
        {
            if (fpSettings == null)
            {
                var elem = doc.Descendants("fingerprint").SingleOrDefault();
                if (elem != null)
                    fpSettings = new XDocument(elem);
            }

            var fp = doc.Root?.Attribute("fingerPrint");
            if (fp == null || fpSettings == null)
                return true;
            else
                return fp.Value == GetFingerPrint(fpSettings);
        }

        private void TearDown()
        {
            MvcApplication.UnregisterModelBinders();
            MvcApplication.UnregisterModelValidatorProviders();
            MvcApplication.UnregisterRoutes();
        }

        private void SetUp()
        {
            MvcApplication.RegisterModelBinders();
            MvcApplication.RegisterModelValidatorProviders();
            MvcApplication.RegisterMappings();
            CheatBuildManager();
            AreaRegistration.RegisterAllAreas();
            MvcApplication.RegisterUnity();
            MvcApplication.RegisterRoutes(_routes);
        }

        private void CheatBuildManager()
        {
            var memberInfo = typeof(BuildManager).GetField("_theBuildManager", BindingFlags.NonPublic | BindingFlags.Static);
            if (memberInfo == null) return;
            var manager = memberInfo.GetValue(null);
            typeof(BuildManager).GetProperty("PreStartInitStage", BindingFlags.NonPublic | BindingFlags.Static).SetValue(null, 2, null);
            var fieldInfo = typeof(BuildManager).GetField("_topLevelFilesCompiledStarted", BindingFlags.NonPublic | BindingFlags.Instance);
            fieldInfo?.SetValue(manager, true);
            var field = typeof(BuildManager).GetField("_topLevelReferencedAssemblies", BindingFlags.NonPublic | BindingFlags.Instance);
            field?.SetValue(manager, new List<Assembly> { typeof(MvcApplication).Assembly });
        }

        private IEnumerable<RecordedAction> LoadAllActionsToReplay(XDocument doc)
        {

            var root = GetActionsRoot(doc);
            return root.Elements("action").Select(n =>
                new RecordedAction
                {
                    Code = n.Attribute("code").Value,
                    Lcid = GetLcid(n),
                    Ids = n.Attribute("ids").Value.Split(",".ToCharArray()),
                    ParentId = int.Parse(n.Attribute("parentId").Value),
                    Form = FormFromXml(n),
                    ChildId = GetChildId(n),
                    ChildIds = GetChildIds(n),
                    ChildLinkIds = GetChildLinkIds(n),
                    BackwardId = GetBackwardId(n),
                    ResultId = GetResultId(n),
                    ChildActions = GetChildActions(),
                    VirtualFieldIds = GetVirtualFieldIds(n),
                    CustomActionCode = GetCustomActionCode(n)
                }

            ).ToArray();
        }

        private string GetCustomActionCode(XElement n)
        {
            var code = n.Attribute("code").Value;
            if (code == ActionCode.AddNewCustomAction && n.Attribute("actionCode") != null)
                return n.Attribute("actionCode").Value;
            else
                return string.Empty;
        }

        private int GetLcid(XElement n)
        {
            return GetIntAttribute(n, "lcid");
        }

        private string GetVirtualFieldIds(XElement n)
        {
            return GetStringAttribute(n, "newVirtualFieldIds");
        }

        private int GetResultId(XElement n)
        {
            var code = n.Attribute("code").Value;
            if (new[] { ActionCode.CreateLikeContent, ActionCode.CreateLikeField, ActionCode.CreateLikeArticle }.Contains(code))
            {
                return GetIntAttribute(n, "resultId");
            }
            else
            {
                return 0;
            }
        }

        private int GetChildId(XElement n)
        {
            var code = n.Attribute("code").Value;
            if (new[] { ActionCode.AddNewField, ActionCode.FieldProperties, ActionCode.CreateLikeField }.Contains(code))
            {
                return GetIntAttribute(n, "newLinkId");
            }
            else if (new[] { ActionCode.AddNewNotification, ActionCode.NotificationProperties, ActionCode.AddNewTemplateObject, ActionCode.AddNewPageObject }.Contains(code))
            {
                return GetIntAttribute(n, "formatId");
            }
            else if (code == ActionCode.AddNewCustomAction)
            {
                return GetIntAttribute(n, "actionId");
            }
            else
                return 0;
        }

        private string GetChildIds(XElement n)
        {
            var code = n.Attribute("code").Value;
            if (new[] { ActionCode.CreateLikeContent, ActionCode.AddNewContent }.Contains(code))
            {
                return n.Attribute("fieldIds").Value;
            }
            else if (new[] { ActionCode.FieldProperties, ActionCode.AddNewField, ActionCode.CreateLikeField }.Contains(code))
            {
                return GetStringAttribute(n, "newChildFieldIds");
            }
            else if (new[] { ActionCode.AddNewVisualEditorPlugin, ActionCode.VisualEditorPluginProperties }.Contains(code))
            {
                return GetStringAttribute(n, "commandIds");
            }
            else if (new[] { ActionCode.AddNewWorkflow, ActionCode.WorkflowProperties }.Contains(code))
            {
                return GetStringAttribute(n, "rulesIds");
            }
            else
                return null;
        }

        private string GetChildLinkIds(XElement n)
        {
            var code = n.Attribute("code").Value;
            if (new[] { ActionCode.CreateLikeContent, ActionCode.AddNewContent }.Contains(code))
            {
                return GetStringAttribute(n, "linkIds");
            }
            else if (new[] { ActionCode.FieldProperties, ActionCode.AddNewField, ActionCode.CreateLikeField }.Contains(code))
            {
                return GetStringAttribute(n, "newChildLinkIds");
            }
            else
                return null;
        }

        private int GetBackwardId(XElement n)
        {
            return GetIntAttribute(n, "newBackwardId");
        }

        private string GetStringAttribute(XElement n, string name)
        {
            var attr = n.Attribute(name);
            return attr?.Value;
        }

        private int GetIntAttribute(XElement n, string name)
        {
            var attr = n.Attribute(name);
            return (attr != null) ? int.Parse(attr.Value) : 0;
        }

        private List<RecordedAction> GetChildActions()
        {
            var result = new List<RecordedAction>();
            return result;
        }

        private void ReplayForm(RecordedAction action, int userId, XAttribute urlAttribute)
        {
            IControllerFactory factory = new DefaultControllerFactory();//ControllerBuilder.Current.GetControllerFactory();

            var urlParts = action.BackendAction.ControllerActionUrl.Split(@"/".ToCharArray()).Where(n => !string.IsNullOrEmpty(n) && n != "~").ToArray();

            CorrectAction(action);

            var data = GetRouteData(action, urlParts);

            var httpContext = GetHttpContextBase(action, userId, urlAttribute);

            var context = new RequestContext(httpContext, data);

            var controller = factory.CreateController(context, urlParts[0]);
            if (controller == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Controller '{0}' is not found ", urlParts[0]));
            }
            var baseController = (ControllerBase)controller;
            var ci = (action.Lcid == 0) ? CultureInfo.InvariantCulture : new CultureInfo(action.Lcid);
            Thread.CurrentThread.CurrentCulture = ci;
            baseController.ValueProvider = new ValueProviderCollection(new[] {
                new DictionaryValueProvider<object>(data.Values, ci),
                new FormCollection(httpContext.Request.Form).ToValueProvider(),
                new NameValueCollectionValueProvider(httpContext.Request.QueryString, ci),
            });
            baseController.ControllerContext = new ControllerContext(httpContext, data, baseController);

            PreAction(action, context);
            controller.Execute(context);
            PostAction(action, context);

            foreach (var childAction in action.ChildActions)
            {
                ReplayForm(childAction, userId, urlAttribute);
            }

        }

        private void PreAction(RecordedAction action, RequestContext context)
        {

        }

        private void PostAction(RecordedAction action, RequestContext context)
        {
            CorrectReplaces(action, context.HttpContext);
        }

        private static HttpContextBase GetHttpContextBase(RecordedAction action, int userId, XAttribute urlAttribute)
        {

            var httpContext = new Mock<HttpContextBase>();
            httpContext.Setup(c => c.Request).Returns(GetHttpRequestMock(action));
            httpContext.Setup(c => c.Session).Returns(new HttpSessionMock());
            httpContext.Setup(c => c.Response).Returns(new Mock<HttpResponseBase>().Object);
            httpContext.Setup(c => c.Response.Cookies).Returns(new HttpCookieCollection());

            var principal = GetQpPrincipal(userId);
            httpContext.Setup(c => c.User).Returns(principal);
            httpContext.Setup(c => c.Items).Returns(new Hashtable());
            httpContext.Setup(c => c.Cache).Returns(HttpRuntime.Cache);

            var currentRequest = new HttpRequest("foo", "http://tempuri.org/foo", "");
            HttpContext.Current = new HttpContext(currentRequest, new HttpResponse(new StringWriter()))
            {
                User = principal
            };

            var result = httpContext.Object;
            AppendItems(result, urlAttribute);
            return result;
        }

        private static void AppendItems(HttpContextBase result, XAttribute urlAttribute)
        {
            result.Items.Add("IS_REPLAY", true);
            if (urlAttribute != null)
                result.Items.Add("BACKEND_URL", urlAttribute.Value);
        }

        private static QPPrincipal GetQpPrincipal(int userId)
        {
            var user = new UserService().ReadProfile(userId);
            var identity = new QPIdentity(user.Id, user.Name, QPContext.CurrentCustomerCode, "QP", true, 1, "neutral", false);
            var principal = new QPPrincipal(identity, new string[] { });
            return principal;
        }

        private static HttpRequestMock GetHttpRequestMock(RecordedAction action)
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
                        AddListItem(httpRequest.Form, "Data.ForceFieldIds", action.ChildIds);
                    if (options.Contains(EntityTypeCode.ContentLink))
                        AddListItem(httpRequest.Form, "Data.ForceLinkIds", action.ChildLinkIds);
                    break;
                case ActionCode.CreateLikeContent:
                    if (options.Contains(EntityTypeCode.Content))
                        httpRequest.Form.Add("forceId", action.ResultId.ToString());
                    if (options.Contains(EntityTypeCode.Field))
                        httpRequest.Form.Add("forceFieldIds", action.ChildIds);
                    if (options.Contains(EntityTypeCode.ContentLink))
                        httpRequest.Form.Add("forceLinkIds", action.ChildLinkIds);
                    break;
                case ActionCode.CreateLikeField:
                    if (options.Contains(EntityTypeCode.Field))
                        httpRequest.Form.Add("forceId", action.ResultId.ToString());
                    if (options.Contains(EntityTypeCode.Field))
                        httpRequest.Form.Add("forceVirtualFieldIds", action.VirtualFieldIds);
                    if (options.Contains(EntityTypeCode.Field))
                        httpRequest.Form.Add("forceChildFieldIds", action.ChildIds);
                    if (options.Contains(EntityTypeCode.ContentLink))
                        httpRequest.Form.Add("forceLinkId", action.ChildId.ToString());
                    if (options.Contains(EntityTypeCode.ContentLink))
                        httpRequest.Form.Add("forceChildLinkIds", action.ChildLinkIds);
                    break;
                case ActionCode.AddNewVirtualContents:
                case ActionCode.VirtualContentProperties:
                case ActionCode.VirtualFieldProperties:
                    if (options.Contains(EntityTypeCode.Field))
                        AddListItem(httpRequest.Form, "Data.ForceVirtualFieldIds", action.VirtualFieldIds);
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
                        httpRequest.Form.Add("Data.ForceActionId", action.ChildId.ToString());
                    break;
                case ActionCode.AddNewVisualEditorPlugin:
                case ActionCode.VisualEditorPluginProperties:
                    if (options.Contains(EntityTypeCode.VisualEditorCommand))
                        AddListItem(httpRequest.Form, "Data.ForceCommandIds", action.ChildIds);
                    break;
                case ActionCode.AddNewWorkflow:
                case ActionCode.WorkflowProperties:
                    if (options.Contains(EntityTypeCode.WorkflowRule))
                        AddListItem(httpRequest.Form, "Data.ForceRulesIds", action.ChildIds);
                    break;
            }

            httpRequest.SetPath(action.BackendAction.ControllerActionUrl.Replace("~", ""));
            return httpRequest;
        }

        private static void AddListItem(NameValueCollection collection, string name, string commaList)
        {
            if (!string.IsNullOrEmpty(commaList))
                foreach (var id in commaList.Split(",".ToCharArray()))
                {
                    collection.Add(name, id);
                }
        }

        private RouteData GetRouteData(RecordedAction action, string[] urlParts)
        {
            var data = new RouteData();
            data.Values["controller"] = urlParts[0];
            data.Values["action"] = urlParts[1];
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


        #endregion

        #region fingerprint

        private static string ByteArrayToString(byte[] ba)
        {
            var hex = BitConverter.ToString(ba);
            return hex.Replace("-", "");
        }

        public string GetFingerPrint(XDocument settings)
        {
            return ByteArrayToString(new FingerprintService().GetFingerprint(settings));
        }

        #endregion
    }
}
