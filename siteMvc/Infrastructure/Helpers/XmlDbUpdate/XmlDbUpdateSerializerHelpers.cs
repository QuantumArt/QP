using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Extensions.Primitives;
using QP8.Infrastructure.Helpers;
using Quantumart.QP8.BLL;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Infrastructure.Constants;
using Quantumart.QP8.WebMvc.Infrastructure.Extensions;
using Quantumart.QP8.WebMvc.Infrastructure.Models;

namespace Quantumart.QP8.WebMvc.Infrastructure.Helpers.XmlDbUpdate
{
    internal static class XmlDbUpdateSerializerHelpers
    {
        internal static XDocument SerializeAction(XmlDbUpdateRecordedAction action, string currentDbVersion, string backendUrl)
        {
            return SerializeAction(action, currentDbVersion, backendUrl, false);
        }

        internal static XDocument SerializeAction(XmlDbUpdateRecordedAction action, string currentDbVersion, string backendUrl, bool withoutRoot)
        {
            var actionAlement = new XElement(XmlDbUpdateXDocumentConstants.ActionElement,
                new XAttribute(XmlDbUpdateXDocumentConstants.ActionCodeAttribute, action.Code),
                new XAttribute(XmlDbUpdateXDocumentConstants.ActionIdsAttribute, string.Join(",", action.Ids)),
                new XAttribute(XmlDbUpdateXDocumentConstants.ActionParentIdAttribute, action.ParentId),
                new XAttribute(XmlDbUpdateXDocumentConstants.ActionLcidAttribute, action.Lcid),
                new XAttribute(XmlDbUpdateXDocumentConstants.ActionExecutedAttribute, action.Executed.ToString(CultureHelpers.GetCultureByLcid(action.Lcid))),
                new XAttribute(XmlDbUpdateXDocumentConstants.ActionExecutedByAttribute, action.ExecutedBy),
                GetEntitySpecificAttributesForPersisting(action),
                GetActionChildElements(action.Form));

            if (!withoutRoot)
            {
                var root = GetOrCreateRoot(backendUrl, currentDbVersion);
                root.Add(actionAlement);
                return root.Document;
            }

            return new XDocument(actionAlement).Document;
        }

        internal static XmlDbUpdateRecordedAction DeserializeAction(XElement action)
        {
            var lcid = GetLcid(action);
            return new XmlDbUpdateRecordedAction
            {
                Code = GetCode(action),
                Ids = GetIds(action),
                ResultId = GetResultIdByCode(action),
                UniqueId = GetUniqueIdByCode(action),
                ResultUniqueId = GetResultUniqueIdByCode(action),
                ParentId = GetParentId(action),
                Lcid = lcid,
                BackwardId = GetBackwardId(action),
                VirtualFieldIds = GetVirtualFieldIds(action),
                ChildId = GetChildIdByCode(action),
                ChildIds = GetChildIdsByCode(action),
                ChildLinkIds = GetChildLinkIdsByCode(action),
                UserId = GetUserId(action),
                GroupId = GetGroupId(action),
                CustomActionCode = GetCustomActionCodeByCode(action),
                Form = GetActionFields(action),
                Executed = GetExecuted(action, lcid),
                ExecutedBy = GetExecutedBy(action)
            };
        }

        internal static void ErasePreviouslyRecordedActions(string backendUrl, string currentDbVersion)
        {
            RemoveRecordsXml();
            var root = GetOrCreateRoot(backendUrl, currentDbVersion);
            var doc = root.Document;
            if (doc != null)
            {
                if (root.HasElements)
                {
                    root.Remove();
                    doc.Add(CreateActionsRoot(backendUrl, currentDbVersion));
                }

                doc.Save(QPContext.GetRecordXmlFilePath());
            }
        }

        private static IEnumerable<XAttribute> GetEntitySpecificAttributesForPersisting(XmlDbUpdateRecordedAction action)
        {
            var result = new Dictionary<string, object>();
            if (XmlDbUpdateQpActionHelpers.IsArticleAndHasUniqueId(action.Code))
            {
                result.Add(XmlDbUpdateXDocumentConstants.ActionUniqueIdAttribute, string.Join(",", action.UniqueId));
            }

            if (XmlDbUpdateQpActionHelpers.IsActionHasResultId(action.Code))
            {
                result.Add(XmlDbUpdateXDocumentConstants.ActionResultIdAttribute, action.ResultId);
                if (XmlDbUpdateQpActionHelpers.IsArticleAndHasUniqueId(action.Code))
                {
                    result.Add(XmlDbUpdateXDocumentConstants.ActionResultUniqueIdAttribute, action.ResultUniqueId);
                }
            }

            switch (action.Code)
            {
                case ActionCode.AddNewVirtualContents:
                case ActionCode.VirtualContentProperties:
                case ActionCode.VirtualFieldProperties:
                    result.Add(XmlDbUpdateXDocumentConstants.ActionNewVirtualFieldIdsAttribute, action.VirtualFieldIds);
                    break;
                case ActionCode.AddNewContent:
                case ActionCode.CreateLikeContent:
                    result.Add(XmlDbUpdateXDocumentConstants.ActionFieldIdsAttribute, action.FieldIds);
                    result.Add(XmlDbUpdateXDocumentConstants.ActionLinkIdsAttribute, action.LinkIds);
                    break;
                case ActionCode.AddNewField:
                case ActionCode.FieldProperties:
                case ActionCode.CreateLikeField:
                    result.Add(XmlDbUpdateXDocumentConstants.ActionNewVirtualFieldIdsAttribute, action.VirtualFieldIds);
                    result.Add(XmlDbUpdateXDocumentConstants.ActionNewLinkIdAttribute, action.NewLinkId);
                    result.Add(XmlDbUpdateXDocumentConstants.ActionNewBackwardIdAttribute, action.BackwardId);
                    result.Add(XmlDbUpdateXDocumentConstants.ActionNewChildFieldIdsAttribute, action.NewChildFieldIds);
                    result.Add(XmlDbUpdateXDocumentConstants.ActionNewChildLinkIdsAttribute, action.NewChildLinkIds);
                    break;
                case ActionCode.AddNewCustomAction:
                case ActionCode.CreateLikeCustomAction:
                    result.Add(XmlDbUpdateXDocumentConstants.ActionActionId, action.ActionId);
                    result.Add(XmlDbUpdateXDocumentConstants.ActionActionCode, action.ActionCode);
                    break;
                case ActionCode.AddNewVisualEditorPlugin:
                case ActionCode.VisualEditorPluginProperties:
                    result.Add(XmlDbUpdateXDocumentConstants.ActionCommandIdsAttribute, action.NewCommandIds);
                    break;
                case ActionCode.AddNewWorkflow:
                case ActionCode.WorkflowProperties:
                    result.Add(XmlDbUpdateXDocumentConstants.ActionRulesIdsAttribute, action.NewRulesIds);
                    break;
                case ActionCode.AddNewNotification:
                case ActionCode.NotificationProperties:
                    result.Add(XmlDbUpdateXDocumentConstants.ActionFormatIdAttribute, action.NotificationFormatId);
                    break;
                case ActionCode.AddNewPageObject:
                case ActionCode.AddNewTemplateObject:
                    result.Add(XmlDbUpdateXDocumentConstants.ActionFormatIdAttribute, action.DefaultFormatId);
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
                        result.Add(XmlDbUpdateXDocumentConstants.UserIdAttribute, action.UserId);
                    }

                    if (action.GroupId != 0)
                    {
                        result.Add(XmlDbUpdateXDocumentConstants.GroupIdAttribute, action.GroupId);
                    }
                    break;
            }

            return result.Where(r => r.Value != null).Select(r => new XAttribute(r.Key, r.Value));
        }

        private static IEnumerable<XElement> GetActionChildElements(Dictionary<string, StringValues> form)
        {
            return form?.Keys.Where(n => n != "__RequestVerificationToken")
                .SelectMany(k => ((string[])form[k]) ?? Enumerable.Empty<string>(), (fieldNameAttributeValue, fieldElementValue) =>
            {
                var fieldElement = new XElement(XmlDbUpdateXDocumentConstants.FieldElement, fieldElementValue);
                fieldElement.SetAttributeValue(XmlDbUpdateXDocumentConstants.FieldNameAttribute, fieldNameAttributeValue);
                return fieldElement;
            }) ?? Enumerable.Empty<XElement>();
        }

        private static XElement CreateActionsRoot(string backendUrl, string currentDbVersion) => new XElement(
            XmlDbUpdateXDocumentConstants.RootElement,
            GetBackendUrlAttribute(backendUrl),
            new XAttribute(XmlDbUpdateXDocumentConstants.RootDbVersionAttribute,
                currentDbVersion)
        );

        private static XAttribute GetBackendUrlAttribute(string backendUrl) => new XAttribute(XmlDbUpdateXDocumentConstants.RootBackendUrlAttribute, backendUrl);

        private static XElement GetOrCreateRoot(string backendUrl, string currentDbVersion)
        {
            var doc = File.Exists(QPContext.GetRecordXmlFilePath()) ? XDocument.Load(QPContext.GetRecordXmlFilePath()) : new XDocument(CreateActionsRoot(backendUrl, currentDbVersion));
            return doc.Elements(XmlDbUpdateXDocumentConstants.RootElement).Single();
        }

        private static void RemoveRecordsXml()
        {
            var path = QPContext.GetRecordXmlFilePath();
            if (File.Exists(path))
            {
                File.SetAttributes(path, FileAttributes.Normal);
                File.Delete(path);
            }
        }

        private static string GetCode(XElement action) => action.Attribute(XmlDbUpdateXDocumentConstants.ActionCodeAttribute)?.Value;

        private static string[] GetIds(XElement action) => action.Attribute(XmlDbUpdateXDocumentConstants.ActionIdsAttribute)?.Value.Split(",".ToCharArray());

        private static int GetParentId(XElement action) => int.Parse(action.Attribute(XmlDbUpdateXDocumentConstants.ActionParentIdAttribute)?.Value ?? throw new InvalidOperationException());

        private static int GetLcid(XElement action) => action.Attribute(XmlDbUpdateXDocumentConstants.ActionLcidAttribute).GetValueOrDefault<int>();

        private static int GetBackwardId(XElement action) => action.Attribute(XmlDbUpdateXDocumentConstants.ActionNewBackwardIdAttribute).GetValueOrDefault<int>();

        private static int GetUserId(XElement action) => action.Attribute(XmlDbUpdateXDocumentConstants.UserIdAttribute).GetValueOrDefault<int>();

        private static int GetGroupId(XElement action) => action.Attribute(XmlDbUpdateXDocumentConstants.GroupIdAttribute).GetValueOrDefault<int>();

        private static string GetVirtualFieldIds(XElement action) => action.Attribute(XmlDbUpdateXDocumentConstants.ActionNewVirtualFieldIdsAttribute).GetValueOrDefault<string>();

        private static int GetChildIdByCode(XElement action)
        {
            switch (GetCode(action))
            {
                case ActionCode.AddNewField:
                case ActionCode.FieldProperties:
                case ActionCode.CreateLikeField:
                    return action.Attribute(XmlDbUpdateXDocumentConstants.ActionNewLinkIdAttribute).GetValueOrDefault<int>();
                case ActionCode.AddNewNotification:
                case ActionCode.NotificationProperties:
                case ActionCode.AddNewTemplateObject:
                case ActionCode.AddNewPageObject:
                    return action.Attribute(XmlDbUpdateXDocumentConstants.ActionFormatIdAttribute).GetValueOrDefault<int>();
                case ActionCode.AddNewCustomAction:
                    return action.Attribute(XmlDbUpdateXDocumentConstants.ActionActionId).GetValueOrDefault<int>();
                default:
                    return default(int);
            }
        }

        private static string GetChildIdsByCode(XElement action)
        {
            switch (GetCode(action))
            {
                case ActionCode.CreateLikeContent:
                case ActionCode.AddNewContent:
                    return action.Attribute(XmlDbUpdateXDocumentConstants.ActionFieldIdsAttribute).GetValueOrDefault<string>();
                case ActionCode.CreateLikeCustomAction:
                    return action.Attribute(XmlDbUpdateXDocumentConstants.ActionActionId).GetValueOrDefault<string>();
                case ActionCode.FieldProperties:
                case ActionCode.AddNewField:
                case ActionCode.CreateLikeField:
                    return action.Attribute(XmlDbUpdateXDocumentConstants.ActionNewChildFieldIdsAttribute).GetValueOrDefault<string>();
                case ActionCode.AddNewVisualEditorPlugin:
                case ActionCode.VisualEditorPluginProperties:
                    return action.Attribute(XmlDbUpdateXDocumentConstants.ActionCommandIdsAttribute).GetValueOrDefault<string>();
                case ActionCode.AddNewWorkflow:
                case ActionCode.WorkflowProperties:
                    return action.Attribute(XmlDbUpdateXDocumentConstants.ActionRulesIdsAttribute).GetValueOrDefault<string>();
                default:
                    return default(string);
            }
        }

        private static string GetChildLinkIdsByCode(XElement action)
        {
            switch (GetCode(action))
            {
                case ActionCode.CreateLikeContent:
                case ActionCode.AddNewContent:
                    return action.Attribute(XmlDbUpdateXDocumentConstants.ActionLinkIdsAttribute).GetValueOrDefault<string>();
                case ActionCode.FieldProperties:
                case ActionCode.AddNewField:
                case ActionCode.CreateLikeField:
                    return action.Attribute(XmlDbUpdateXDocumentConstants.ActionNewChildLinkIdsAttribute).GetValueOrDefault<string>();
                default:
                    return default(string);
            }
        }

        private static int GetResultIdByCode(XElement action) => XmlDbUpdateQpActionHelpers.IsActionHasResultId(GetCode(action))
            ? action.Attribute(XmlDbUpdateXDocumentConstants.ActionResultIdAttribute).GetValueOrDefault<int>()
            : default(int);

        private static Guid[] GetUniqueIdByCode(XElement action)
        {
            var actionCode = GetCode(action);
            var uniqueIdValue = XmlDbUpdateQpActionHelpers.IsArticleAndHasUniqueId(actionCode)
                ? action.Attribute(XmlDbUpdateXDocumentConstants.ActionUniqueIdAttribute)?.Value ?? Guid.Empty.ToString()
                : Guid.Empty.ToString();

            return uniqueIdValue.Split(",".ToCharArray()).Select(Guid.Parse).ToArray();
        }

        private static Guid GetResultUniqueIdByCode(XElement action) => XmlDbUpdateQpActionHelpers.IsArticleAndHasResultUniqueId(GetCode(action))
            ? Guid.Parse(action.Attribute(XmlDbUpdateXDocumentConstants.ActionResultUniqueIdAttribute)?.Value ?? Guid.Empty.ToString())
            : Guid.Empty;

        private static string GetCustomActionCodeByCode(XElement action)
        {
            switch (GetCode(action))
            {
                case ActionCode.CreateLikeCustomAction:
                case ActionCode.AddNewCustomAction:
                    return action.Attribute(XmlDbUpdateXDocumentConstants.ActionActionCode).GetValueOrDefault<string>();
                default:
                    return default(string);
            }
        }

        private static Dictionary<string, StringValues> GetActionFields(XContainer root)
        {
            var result = new Dictionary<string, StringValues>();
            foreach (var elem in root.Elements())
            {
                var key = elem.Attribute(XmlDbUpdateXDocumentConstants.FieldNameAttribute)?.Value;
                if (!string.IsNullOrEmpty(key))
                {
                    if (result.ContainsKey(key))
                    {
                        result[key] = StringValues.Concat(result[key], elem.Value);
                    }
                    else
                    {
                        result[key] = elem.Value;
                    }
                }
            }

            return result;
        }

        private static DateTime GetExecuted(XElement action, int lcid) =>
            Convert.ToDateTime(action.Attribute(XmlDbUpdateXDocumentConstants.ActionExecutedAttribute)?.Value, CultureHelpers.GetCultureByLcid(lcid));

        private static string GetExecutedBy(XElement action) =>
            action.Attribute(XmlDbUpdateXDocumentConstants.ActionExecutedByAttribute).GetValueOrDefault<string>();
    }
}
