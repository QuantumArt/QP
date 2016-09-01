using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Infrastructure.Constants.XmlDbUpdate;
using Quantumart.QP8.WebMvc.Infrastructure.Extensions;
using Quantumart.QP8.WebMvc.Infrastructure.Models;

namespace Quantumart.QP8.WebMvc.Infrastructure.Helpers.XmlDbUpdate
{
    internal static class XmlDbUpdateSerializerHelpers
    {
        internal static XDocument SerializeAction(XmlDbUpdateRecordedAction action, string backendUrl)
        {
            var root = GetOrCreateRoot(backendUrl);
            root.Add(new XElement(XmlDbUpdateXDocumentConstants.ActionElement,
                new XAttribute(XmlDbUpdateXDocumentConstants.ActionCodeAttribute, action.Code),
                new XAttribute(XmlDbUpdateXDocumentConstants.ActionIdsAttribute, string.Join(",", action.Ids)),
                new XAttribute(XmlDbUpdateXDocumentConstants.ActionParentIdAttribute, action.ParentId),
                new XAttribute(XmlDbUpdateXDocumentConstants.ActionLcidAttribute, action.Lcid),
                new XAttribute(XmlDbUpdateXDocumentConstants.ActionExecutedAttribute, action.Executed.ToString(CultureHelpers.GetCultureInfoByLcid(action.Lcid))),
                new XAttribute(XmlDbUpdateXDocumentConstants.ActionExecutedByAttribute, action.ExecutedBy),
                GetEntitySpecificAttributesForPersisting(action),
                GetActionChildElements(action.Form))
            );

            return root.Document;
        }

        internal static XmlDbUpdateRecordedAction DeserializeAction(XElement action)
        {
            var lcid = GetLcid(action);
            return new XmlDbUpdateRecordedAction
            {
                Code = GetCode(action),
                Ids = GetIds(action),
                ParentId = GetParentId(action),
                Lcid = lcid,
                BackwardId = GetBackwardId(action),
                VirtualFieldIds = GetVirtualFieldIds(action),
                ChildId = GetChildIdByCode(action),
                ChildIds = GetChildIdsByCode(action),
                ChildLinkIds = GetChildLinkIdsByCode(action),
                ResultId = GetResultIdByCode(action),
                CustomActionCode = GetCustomActionCodeByCode(action),
                Form = GetActionFields(action),
                Executed = GetExecuted(action, lcid),
                ExecutedBy = GetExecutedBy(action)
            };
        }

        internal static void ErasePreviouslyRecordedActions(string backendUrl)
        {
            var root = GetOrCreateRoot(backendUrl);
            var doc = root.Document;
            if (doc != null)
            {
                if (root.HasElements)
                {
                    root.Remove();
                    doc.Add(CreateActionsRoot(backendUrl));
                }

                doc.Save(XmlDbUpdateXDocumentConstants.XmlFilePath);
            }
        }

        private static IEnumerable<XAttribute> GetEntitySpecificAttributesForPersisting(XmlDbUpdateRecordedAction action)
        {
            var result = new Dictionary<string, object>();
            if (action.BackendAction.ActionType.Code == ActionTypeCode.Copy)
            {
                result.Add(XmlDbUpdateXDocumentConstants.ActionResultIdAttribute, action.ResultId);
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
            }

            return result.Where(r => r.Value != null).Select(r => new XAttribute(r.Key, r.Value));
        }

        private static IEnumerable<XElement> GetActionChildElements(NameValueCollection nvc)
        {
            return nvc?.AllKeys.SelectMany(nvc.GetValues, (fieldNameAttributeValue, fieldElementValue) =>
            {
                var fieldElement = new XElement(XmlDbUpdateXDocumentConstants.FieldElement, fieldElementValue);
                fieldElement.SetAttributeValue(XmlDbUpdateXDocumentConstants.FieldNameAttribute, fieldNameAttributeValue);
                return fieldElement;
            }) ?? Enumerable.Empty<XElement>();
        }

        private static XElement CreateActionsRoot(string backendUrl)
        {
            return new XElement(
                XmlDbUpdateXDocumentConstants.RootElement,
                GetBackendUrlAttribute(backendUrl),
                new XAttribute(XmlDbUpdateXDocumentConstants.RootDbVersionAttribute,
                new ApplicationInfoHelper().GetCurrentDbVersion())
            );
        }

        private static XAttribute GetBackendUrlAttribute(string backendUrl)
        {
            return new XAttribute(XmlDbUpdateXDocumentConstants.RootBackendUrlAttribute, backendUrl);
        }

        private static XElement GetOrCreateRoot(string backendUrl)
        {
            var doc = File.Exists(XmlDbUpdateXDocumentConstants.XmlFilePath) ? XDocument.Load(XmlDbUpdateXDocumentConstants.XmlFilePath) : new XDocument(CreateActionsRoot(backendUrl));
            return doc.Elements(XmlDbUpdateXDocumentConstants.RootElement).Single();
        }

        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        private static string GetCode(XElement action)
        {
            // ReSharper disable once PossibleNullReferenceException
            return action.Attribute(XmlDbUpdateXDocumentConstants.ActionCodeAttribute).Value;
        }

        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        private static string[] GetIds(XElement action)
        {
            // ReSharper disable once PossibleNullReferenceException
            return action.Attribute(XmlDbUpdateXDocumentConstants.ActionIdsAttribute).Value.Split(",".ToCharArray());
        }

        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        private static int GetParentId(XElement action)
        {
            // ReSharper disable once PossibleNullReferenceException
            return int.Parse(action.Attribute(XmlDbUpdateXDocumentConstants.ActionParentIdAttribute).Value);
        }

        private static int GetLcid(XElement action)
        {
            return action.Attribute(XmlDbUpdateXDocumentConstants.ActionLcidAttribute).GetValueOrDefault<int>();
        }

        private static int GetBackwardId(XElement action)
        {
            return action.Attribute(XmlDbUpdateXDocumentConstants.ActionNewBackwardIdAttribute).GetValueOrDefault<int>();
        }

        private static string GetVirtualFieldIds(XElement action)
        {
            return action.Attribute(XmlDbUpdateXDocumentConstants.ActionNewVirtualFieldIdsAttribute).GetValueOrDefault<string>();
        }

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

        private static int GetResultIdByCode(XElement action)
        {
            switch (GetCode(action))
            {
                case ActionCode.CreateLikeContent:
                case ActionCode.CreateLikeField:
                case ActionCode.CreateLikeArticle:
                    return action.Attribute(XmlDbUpdateXDocumentConstants.ActionResultIdAttribute).GetValueOrDefault<int>();
                default:
                    return default(int);
            }
        }

        private static string GetCustomActionCodeByCode(XElement action)
        {
            switch (GetCode(action))
            {
                case ActionCode.AddNewCustomAction:
                    return action.Attribute(XmlDbUpdateXDocumentConstants.ActionActionCode).GetValueOrDefault<string>();
                default:
                    return default(string);
            }
        }

        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        private static NameValueCollection GetActionFields(XContainer root)
        {
            return root.Elements().Aggregate(new NameValueCollection(), (seed, curr) =>
            {
                // ReSharper disable once PossibleNullReferenceException
                seed.Add(curr.Attribute(XmlDbUpdateXDocumentConstants.FieldNameAttribute).Value, curr.Value);
                return seed;
            });
        }

        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        private static DateTime GetExecuted(XElement action, int lcid)
        {
            try
            {
                return Convert.ToDateTime(action.Attribute(XmlDbUpdateXDocumentConstants.ActionExecutedAttribute).Value, CultureHelpers.GetCultureInfoByLcid(lcid));
            }
            catch
            {
                //TODO: DELETE THIS!!! TEMP!!! DELETE THIS!!! TEMP!!! DELETE THIS!!! TEMP!!! DELETE THIS!!! TEMP!!! And remove unusing references then.
                return Convert.ToDateTime(action.Attribute(XmlDbUpdateXDocumentConstants.ActionExecutedAttribute).Value, CultureInfo.InvariantCulture);
            }
        }

        private static string GetExecutedBy(XElement action)
        {
            return action.Attribute(XmlDbUpdateXDocumentConstants.ActionExecutedByAttribute).GetValueOrDefault<string>();
        }
    }
}

