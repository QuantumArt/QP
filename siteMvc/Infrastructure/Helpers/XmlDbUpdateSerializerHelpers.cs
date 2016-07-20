using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Infrastructure.Constants.XmlDbUpdate;
using Quantumart.QP8.WebMvc.Infrastructure.Extensions;
using Quantumart.QP8.WebMvc.Infrastructure.Models;

namespace Quantumart.QP8.WebMvc.Infrastructure.Helpers
{
    internal class XmlDbUpdateSerializerHelpers
    {
        internal static XDocument SerializeAction(XmlDbUpdateRecordedAction action, string backendUrl)
        {
            var root = GetOrCreateRoot(backendUrl);
            root.Add(new XElement(XmlDbUpdateXDocumentConstants.ActionElement,
                new XAttribute(XmlDbUpdateXDocumentConstants.ActionCodeAttribute, action.Code),
                new XAttribute(XmlDbUpdateXDocumentConstants.ActionIdsAttribute, string.Join(",", action.Ids)),
                new XAttribute(XmlDbUpdateXDocumentConstants.ActionParentIdAttribute, action.ParentId),
                new XAttribute(XmlDbUpdateXDocumentConstants.ActionLcidAttribute, action.Lcid),
                new XAttribute(XmlDbUpdateXDocumentConstants.ActionExecutedAttribute, action.Executed.ToString(CultureInfo.InvariantCulture)),
                new XAttribute(XmlDbUpdateXDocumentConstants.ActionExecutedByAttribute, action.ExecutedBy),
                GetEntitySpecificAttributesForPersisting(action),
                GetActionChildElements(action.Form))
            );

            return root.Document;
        }

        internal static XmlDbUpdateRecordedAction DeserializeAction(XElement action)
        {
            return new XmlDbUpdateRecordedAction
            {
                Code = GetCode(action),
                Ids = GetIds(action),
                ParentId = GetParentId(action),
                Lcid = GetLcid(action),
                BackwardId = GetBackwardId(action),
                VirtualFieldIds = GetVirtualFieldIds(action),
                ChildId = GetChildIdByCode(action),
                ChildIds = GetChildIdsByCode(action),
                ChildLinkIds = GetChildLinkIdsByCode(action),
                ResultId = GetResultIdByCode(action),
                CustomActionCode = GetCustomActionCodeByCode(action),
                Form = GetActionFields(action),
                Executed = GetExecuted(action),
                ExecutedBy = GetExecutedBy(action)
            };
        }

        internal static void ErasePreviouslyRecordedActions(string backendUrl, bool overrideFile)
        {
            var root = GetOrCreateRoot(backendUrl);
            var doc = root.Document;
            if (doc != null)
            {
                if (root.HasElements && overrideFile)
                {
                    root.Remove();
                    doc.Add(CreateActionsRoot(backendUrl));
                }

                doc.Save(XmlDbUpdateXDocumentConstants.XmlFilePath);
            }
        }

        private static IEnumerable<XAttribute> GetEntitySpecificAttributesForPersisting(XmlDbUpdateRecordedAction action)
        {
            if (action.BackendAction.ActionType.Code == ActionTypeCode.Copy)
            {
                yield return new XAttribute(XmlDbUpdateXDocumentConstants.ActionResultIdAttribute, action.ResultId);
            }

            switch (action.Code)
            {
                case ActionCode.AddNewVirtualContents:
                case ActionCode.VirtualContentProperties:
                case ActionCode.VirtualFieldProperties:
                    yield return new XAttribute(XmlDbUpdateXDocumentConstants.ActionNewVirtualFieldIdsAttribute, action.VirtualFieldIds);
                    break;
                case ActionCode.AddNewContent:
                case ActionCode.CreateLikeContent:
                    yield return new XAttribute(XmlDbUpdateXDocumentConstants.ActionFieldIdsAttribute, action.FieldIds);
                    yield return new XAttribute(XmlDbUpdateXDocumentConstants.ActionLinkIdsAttribute, action.LinkIds);
                    break;
                case ActionCode.AddNewField:
                case ActionCode.FieldProperties:
                case ActionCode.CreateLikeField:
                    yield return new XAttribute(XmlDbUpdateXDocumentConstants.ActionNewVirtualFieldIdsAttribute, action.VirtualFieldIds);
                    yield return new XAttribute(XmlDbUpdateXDocumentConstants.ActionNewLinkIdAttribute, action.NewLinkId);
                    yield return new XAttribute(XmlDbUpdateXDocumentConstants.ActionNewBackwardIdAttribute, action.BackwardId);
                    yield return new XAttribute(XmlDbUpdateXDocumentConstants.ActionNewChildFieldIdsAttribute, action.NewChildFieldIds);
                    yield return new XAttribute(XmlDbUpdateXDocumentConstants.ActionNewChildLinkIdsAttribute, action.NewChildLinkIds);
                    break;
                case ActionCode.AddNewCustomAction:
                    yield return new XAttribute(XmlDbUpdateXDocumentConstants.ActionActionId, action.ActionId);
                    yield return new XAttribute(XmlDbUpdateXDocumentConstants.ActionActionCode, action.ActionCode);
                    break;
                case ActionCode.AddNewVisualEditorPlugin:
                case ActionCode.VisualEditorPluginProperties:
                    yield return new XAttribute(XmlDbUpdateXDocumentConstants.ActionCommandIdsAttribute, action.NewCommandIds);
                    break;
                case ActionCode.AddNewWorkflow:
                case ActionCode.WorkflowProperties:
                    yield return new XAttribute(XmlDbUpdateXDocumentConstants.ActionRulesIdsAttribute, action.NewRulesIds);
                    break;
                case ActionCode.AddNewNotification:
                case ActionCode.NotificationProperties:
                    yield return new XAttribute(XmlDbUpdateXDocumentConstants.ActionFormatIdAttribute, action.NotificationFormatId);
                    break;
                case ActionCode.AddNewPageObject:
                case ActionCode.AddNewTemplateObject:
                    yield return new XAttribute(XmlDbUpdateXDocumentConstants.ActionFormatIdAttribute, action.DefaultFormatId);
                    break;
            }
        }

        private static IEnumerable<XElement> GetActionChildElements(NameValueCollection nvc)
        {
            return nvc.AllKeys.SelectMany(nvc.GetValues, (fieldNameAttributeValue, fieldElementValue) =>
            {
                var fieldElement = new XElement(XmlDbUpdateXDocumentConstants.FieldElement, fieldElementValue);
                fieldElement.SetAttributeValue(XmlDbUpdateXDocumentConstants.FieldNameAttribute, fieldNameAttributeValue);
                return fieldElement;
            });
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
            XDocument doc;
            try
            {
                doc = XDocument.Load(XmlDbUpdateXDocumentConstants.XmlFilePath);
            }
            catch (FileNotFoundException)
            {
                doc = new XDocument(CreateActionsRoot(backendUrl));
            }

            return doc.Elements(XmlDbUpdateXDocumentConstants.RootElement).Single();
        }

        private static string GetCode(XElement action)
        {
            return action.Attribute(XmlDbUpdateXDocumentConstants.ActionCodeAttribute).Value;
        }

        private static string[] GetIds(XElement action)
        {
            return action.Attribute(XmlDbUpdateXDocumentConstants.ActionIdsAttribute).Value.Split(",".ToCharArray());
        }

        private static int GetParentId(XElement action)
        {
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

        private static NameValueCollection GetActionFields(XContainer root)
        {
            return root.Elements().Aggregate(new NameValueCollection(), (seed, curr) =>
            {
                seed.Add(curr.Attribute(XmlDbUpdateXDocumentConstants.FieldNameAttribute).Value, curr.Value);
                return seed;
            });
        }

        private static DateTime GetExecuted(XElement action)
        {
            return Convert.ToDateTime(action.Attribute(XmlDbUpdateXDocumentConstants.ActionExecutedAttribute).Value);
        }

        private static string GetExecutedBy(XElement action)
        {
            return action.Attribute(XmlDbUpdateXDocumentConstants.ActionExecutedByAttribute).GetValueOrDefault<string>();
        }
    }
}
