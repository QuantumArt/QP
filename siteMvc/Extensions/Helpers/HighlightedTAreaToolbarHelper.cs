using System.Collections.Generic;
using System.Linq;
using System.Web;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.WebMvc.Extensions.Helpers
{
    public static class HighlightedTAreaToolbarHelper
    {
        public static Dictionary<string, string> Functions(int? netLanguageId, string assemblingType, bool presentationOrCodeBehind, bool isContainer)
        {
            var result = new Dictionary<string, string>();
            if (assemblingType == AssemblingType.Asp)
            {
                result.Add("GetContentItemLinkIDs", "GetContentItemLinkIDs(LinkFieldName, ItemID)");
                result.Add("GetLinkIDs", "GetLinkIDs(LinkFieldName)");
                result.Add("GetContentID", "GetContentID(ContentName)");
                result.Add("GetContentUploadURL", "GetContentUploadURL(ContentName)");
                result.Add("GetContentUploadURLByID", "GetContentUploadURLByID(ContentID)");

                result.Add("AddFormToContent", "AddFormToContent content_name,  status_name");
                result.Add("UpdateContentItemField", "UpdateContentItemField content_name, field_name, content_item_id");
                result.Add("UpdateContentItem", "UpdateContentItem");
                result.Add("AddUpdateContentItemLink", "AddUpdateContentItemLink LinkFieldName, ItemID, LinkItemID, TargetLinkItems");
                result.Add("RemoveContentItem", "RemoveContentItem(content_item_id)");
                result.Add("FieldName", "FieldName(content_name, field_name)");

                result.Add("Value", "Value(key)");
                result.Add("NumValue", "NumValue(key)");
                result.Add("AddValue", "AddValue key, value");
                result.Add("StrValue", "StrValue(key)");
                result.Add("DirtyValue", "DirtyValue(key)");

                result.Add("Length", "Length(param)");
                result.Add("IsNumber", "IsNumber(param)");
                result.Add("GetSiteUrl", "GetSiteUrl(site_id)");
                result.Add("GetSiteDirectory", "GetSiteDirectory(site_id)");
                result.Add("GetSiteName", "GetSiteName(site_id)");
                result.Add("SendNotification", "SendNotification notification_on, content_item_id, notification_email");
                result.Add("upload_url", "<%=upload_url%>");
                result.Add("OnScreen", "OnScreen(Value, ItemID)");
                result.Add("OnScreenFlyEdit", "OnScreenFlyEdit(Value, ItemID, FieldName)");
            }

            if (assemblingType == AssemblingType.AspDotNetLikeAsp)
            {
                result.Add("GetContentID", "GetContentID(content_name);");
                result.Add("GetContentUploadUrl", "GetContentUploadUrl(content_name);");
                result.Add("GetContentItemLinkIDs", "GetContentItemLinkIDs(linkFieldName, itemID);");
                result.Add("GetLinkIDs", "GetLinkIDs(LinkFieldName);");

                result.Add("RemoveContentItem", "RemoveContentItem(content_item_id);");
                result.Add("FieldName", "FieldName(content_name, field_name);");
                result.Add("FieldID", "FieldID(content_name, field_name);");
                result.Add("AddFormToContent", "AddFormToContent(content_name, status_name);");
                result.Add("UpdateContentItemField", "UpdateContentItemField(content_name, field_name, content_item_id, with_notification);");

                result.Add("AddValue", "AddValue(key, value);");
                result.Add("DirtyValue", "DirtyValue(key);");
                result.Add("Value", "Value(key);");
                result.Add("NumValue", "NumValue(key);");
                result.Add("StrValue", "StrValue(key);");

                result.Add("ReplaceHTML", "ReplaceHTML(str);");
                result.Add("SendNotification", "SendNotification(notification_on, content_item_id, notification_email);");
                result.Add("GetSiteUrl", "GetSiteUrl();");
            }

            if (assemblingType == AssemblingType.AspDotNet)
            {
                if (netLanguageId == NetLanguage.GetcSharp().Id)
                {
                    if (!presentationOrCodeBehind)
                    {
                        result.Add("GetContentID", "GetContentID(content_name);");
                        result.Add("GetContentUploadUrl", "GetContentUploadUrl(content_name);");
                        result.Add("GetFieldUploadUrl", isContainer ? "GetFieldUploadUrl(field_name);" : "GetFieldUploadUrl(content_name, field_name);");

                        result.Add("GetContentItemLinkIDs", "GetContentItemLinkIDs(linkFieldName, itemID);");
                        result.Add("GetLinkIDs", "GetLinkIDs(LinkFieldName);");

                        result.Add("RemoveContentItem", "RemoveContentItem(content_item_id);");
                        result.Add("FieldName", "FieldName(content_name, field_name);");
                        result.Add("FieldID", "FieldID(content_name, field_name);");
                        result.Add("AddFormToContent", "AddFormToContent(content_name, status_name);");
                        result.Add("UpdateContentItemField", "UpdateContentItemField(content_name, field_name, content_item_id, with_notification);");
                        result.Add("AddUpdateContentItemLink", "AddUpdateContentItemLink(LinkFieldName, ItemID, LinkItems, TargetLinkItems);");

                        result.Add("AddValue", "AddValue(key, value);");
                        result.Add("DirtyValue", "DirtyValue(key);");
                        result.Add("Value", "Value(key);");
                        result.Add("NumValue", "NumValue(key);");
                        result.Add("StrValue", "StrValue(key);");

                        result.Add("ReplaceHTML", "ReplaceHTML(str);");
                        result.Add("SendNotification", "SendNotification(notification_on, content_item_id, notification_email);");
                        result.Add("GetSiteUrl", "GetSiteUrl();");
                        result.Add("upload_url", "upload_url");
                        result.Add("OnScreen", "OnScreen(Value, ItemID);");
                        result.Add("OnScreenFlyEdit", "OnScreenFlyEdit(Value, ItemID, FieldName);");
                    }
                    else
                    {
                        result.Add("GetContentID", "GetContentID(content_name);");
                        result.Add("GetContentUploadUrl", "GetContentUploadUrl(content_name);");
                        result.Add("GetFieldUploadUrl", isContainer ? "GetFieldUploadUrl(field_name);" : "GetFieldUploadUrl(content_name, field_name);");
                        result.Add("GetContentItemLinkIDs", "GetContentItemLinkIDs(linkFieldName, itemID);");
                        result.Add("GetLinkIDs", "GetLinkIDs(LinkFieldName);");
                        result.Add("FieldName", "FieldName(content_name, field_name);");
                        result.Add("FieldID", "FieldID(content_name, field_name);");

                        result.Add("DirtyValue", "DirtyValue(key);");
                        result.Add("Value", "Value(key);");
                        result.Add("NumValue", "NumValue(key);");
                        result.Add("StrValue", "StrValue(key);");

                        result.Add("ReplaceHTML", "ReplaceHTML(str);");
                        result.Add("GetSiteUrl", "GetSiteUrl()");
                        result.Add("upload_url", "upload_url");
                        result.Add("OnScreen", "OnScreen(Value, ItemID);");
                        result.Add("OnScreenFlyEdit", "OnScreenFlyEdit(Value, ItemID, FieldName);");
                    }
                }

                else
                {
                    if (!presentationOrCodeBehind)
                    {
                        result.Add("GetContentID", "GetContentID(content_name);");
                        result.Add("GetContentUploadUrl", "GetContentUploadUrl(content_name);");
                        result.Add("GetFieldUploadUrl", isContainer ? "GetFieldUploadUrl(field_name);" : "GetFieldUploadUrl(content_name, field_name);");
                        result.Add("GetContentItemLinkIDs", "GetContentItemLinkIDs(linkFieldName, itemID);");
                        result.Add("GetLinkIDs", "GetLinkIDs(LinkFieldName);");

                        result.Add("RemoveContentItem", "RemoveContentItem(content_item_id);");
                        result.Add("FieldName", "FieldName(content_name, field_name);");
                        result.Add("FieldID", "FieldID(content_name, field_name);");
                        result.Add("AddFormToContent", "AddFormToContent(content_name, status_name);");
                        result.Add("UpdateContentItemField", "UpdateContentItemField(content_name, field_name, content_item_id, with_notification);");
                        result.Add("AddUpdateContentItemLink", "AddUpdateContentItemLink(LinkFieldName, ItemID, LinkItems, TargetLinkItems);");

                        result.Add("AddValue", "AddValue(key, value);");
                        result.Add("DirtyValue", "DirtyValue(key);");
                        result.Add("Value", "Value(key);");
                        result.Add("NumValue", "NumValue(key);");
                        result.Add("StrValue", "StrValue(key);");

                        result.Add("ReplaceHTML", "ReplaceHTML(str);");
                        result.Add("SendNotification", "SendNotification(notification_on, content_item_id, notification_email);");
                        result.Add("GetSiteUrl", "GetSiteUrl();");
                        result.Add("upload_url", "upload_url");
                        result.Add("OnScreen", "OnScreen(Value, ItemID);");
                        result.Add("OnScreenFlyEdit", "OnScreenFlyEdit(Value, ItemID, FieldName);");
                    }
                    else
                    {
                        result.Add("GetContentID", "GetContentID(content_name);");
                        result.Add("GetContentUploadUrl", "GetContentUploadUrl(content_name);");
                        result.Add("GetFieldUploadUrl", isContainer ? "GetFieldUploadUrl(field_name);" : "GetFieldUploadUrl(content_name, field_name);");
                        result.Add("GetContentItemLinkIDs", "GetContentItemLinkIDs(linkFieldName, itemID);");
                        result.Add("GetLinkIDs", "GetLinkIDs(LinkFieldName);");

                        result.Add("FieldName", "FieldName(content_name, field_name);");
                        result.Add("FieldID", "FieldID(content_name, field_name);");

                        result.Add("DirtyValue", "DirtyValue(key);");
                        result.Add("Value", "Value(key);");
                        result.Add("NumValue", "NumValue(key);");
                        result.Add("StrValue", "StrValue(key);");

                        result.Add("ReplaceHTML", "ReplaceHTML(str);");
                        result.Add("GetSiteUrl", "GetSiteUrl();");
                        result.Add("upload_url", "upload_url");
                        result.Add("OnScreen", "OnScreen(Value, ItemID);");
                        result.Add("OnScreenFlyEdit", "OnScreenFlyEdit(Value, ItemID, FieldName);");
                    }
                }
            }

            return result;
        }

        public static Dictionary<string, string> ContainerInfoProperties(int? netLanguageId, string assemblingType, bool presentationOrCodeBehind)
        {
            var result = new Dictionary<string, string>();
            if (assemblingType == AssemblingType.Asp)
            {
                //Case "containerInfoProperties"
                result.Add("ContentID", "ContainerInfo.ContentID");
                result.Add("ContentName", "ContainerInfo.ContentName");
                result.Add("UploadUrl", "ContainerInfo.UploadUrl");
                result.Add("IsFirst", "ContainerInfo.IsFirst");
                result.Add("IsLast", "ContainerInfo.IsLast");
                result.Add("IsTotalLast", "ContainerInfo.IsTotalLast");
                result.Add("CurrentRecord", "ContainerInfo.CurrentRecord");
                result.Add("TotalRecords", "ContainerInfo.TotalRecords");
                result.Add("RecordsPerPage", "ContainerInfo.RecordsPerPage");
                result.Add("RecordSet", "ContainerInfo.RecordSet");
            }

            if (assemblingType == AssemblingType.AspDotNetLikeAsp)
            {
                //Case "containerInfoProperties"
                result.Add("ContentName", "ContainerInfo.ContentName");
                result.Add("UploadUrl", "ContainerInfo.UploadUrl");
                result.Add("IsFirst", "ContainerInfo.IsFirst");
                result.Add("IsLast", "ContainerInfo.IsLast");
                result.Add("IsTotalLast", "ContainerInfo.IsTotalLast");
                result.Add("CurrentRecord", "ContainerInfo.CurrentRecord");
                result.Add("RecordsPerPage", "ContainerInfo.RecordsPerPage");
            }

            if (assemblingType == AssemblingType.AspDotNet)
            {
                if (netLanguageId == NetLanguage.GetcSharp().Id)
                {
                    if (!presentationOrCodeBehind)
                    {
                        //Case "containerInfoProperties"
                        result.Add("TotalRecords", "TotalRecords");
                        result.Add("AbsoluteTotalRecords", "AbsoluteTotalRecords");
                        result.Add("ContentID", "ContentID");
                        result.Add("ContentName", "ContentName");
                        result.Add("ContentUploadURL", "ContentUploadURL");
                        result.Add("RecordsPerPage", "RecordsPerPage");
                    }
                    else
                    {
                        result.Add("TotalRecords", "TotalRecords");
                        result.Add("AbsoluteTotalRecords", "AbsoluteTotalRecords");
                        result.Add("ContentID", "ContentID");
                        result.Add("ContentName", "ContentName");
                        result.Add("ContentUploadURL", "ContentUploadURL");
                        result.Add("RecordsPerPage", "RecordsPerPage");
                    }
                }
                else
                {
                    if (!presentationOrCodeBehind)
                    {
                        //Case "containerInfoProperties"
                        result.Add("TotalRecords", "TotalRecords");
                        result.Add("AbsoluteTotalRecords", "AbsoluteTotalRecords");
                        result.Add("ContentID", "ContentID");
                        result.Add("ContentName", "ContentName");
                        result.Add("ContentUploadURL", "ContentUploadURL");
                        result.Add("RecordsPerPage", "RecordsPerPage");
                    }
                    else
                    {
                        //Case "containerInfoProperties"
                        result.Add("TotalRecords", "TotalRecords");
                        result.Add("AbsoluteTotalRecords", "AbsoluteTotalRecords");
                        result.Add("ContentID", "ContentID");
                        result.Add("ContentName", "ContentName");
                        result.Add("ContentUploadURL", "ContentUploadURL");
                        result.Add("RecordsPerPage", "RecordsPerPage");
                    }
                }
            }

            return result;
        }

        public static IEnumerable<ListItem> ContainerInfoPropertiesAsListItems(int? netLanguageId, string assemblingType, bool presentationOrCodeBehind)
        {
            var result = ContainerInfoProperties(netLanguageId, assemblingType, presentationOrCodeBehind).Select(x => new ListItem { Text = HttpUtility.HtmlEncode(x.Key), Value = HttpUtility.HtmlEncode(x.Value) }).ToList();
            result.Insert(0, new ListItem { Text = HttpUtility.HtmlEncode(TemplateStrings.SelectProperty), Value = string.Empty });
            return result;
        }

        public static IEnumerable<ListItem> FunctionsAsListItems(int? netLanguageId, string assemblingType, bool presentationOrCodeBehind, bool isContainer)
        {
            var result = Functions(netLanguageId, assemblingType, presentationOrCodeBehind, isContainer).Select(x => new ListItem { Text = HttpUtility.HtmlEncode(x.Key), Value = HttpUtility.HtmlEncode(x.Value) }).ToList();
            result.Insert(0, new ListItem { Text = HttpUtility.HtmlEncode(TemplateStrings.SelectFunction), Value = string.Empty });
            return result;
        }

        internal static IEnumerable<ListItem> GenerateObjectListItems(IEnumerable<BllObject> templateObjects)
        {
            var result = new List<ListItem>();
            if (templateObjects != null)
            {
                result = templateObjects.SelectMany(GoThroughObjFormats).ToList();
            }

            result.Insert(0, new ListItem { Text = TemplateStrings.SelectObject, Value = string.Empty });
            return result;
        }

        internal static IEnumerable<ListItem> GenerateRestObjectListItems(IEnumerable<TemplateObjectFormatDto> templateObjFormatsDto)
        {
            var result = new List<ListItem>();
            if (templateObjFormatsDto != null)
            {
                result = templateObjFormatsDto.Select(x => new ListItem
                {
                    Text = string.IsNullOrEmpty(x.FormatName) ? $"{x.TemplateName}.{x.ObjectName}" : $"{x.TemplateName}.{x.ObjectName}.{x.FormatName}"
                }).ToList();
            }

            result.Insert(0, new ListItem { Text = TemplateStrings.SelectObject, Value = string.Empty });
            return result;
        }

        private static IEnumerable<ListItem> GoThroughObjFormats(BllObject obj)
        {
            return obj.ChildObjectFormats.Select(x =>
            {
                var val = obj.DefaultFormatId == x.Id ? obj.Name : $"{obj.Name}.{x.Name}";
                return new ListItem
                {
                    Text = val,
                    Value = val
                };
            });
        }
    }
}
