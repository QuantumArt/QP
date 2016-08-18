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
using Quantumart.QP8.WebMvc.Infrastructure.Exceptions;
using Quantumart.QP8.WebMvc.Infrastructure.Extensions;
using Quantumart.QP8.WebMvc.Infrastructure.Models;

namespace Quantumart.QP8.WebMvc.Infrastructure.Services.XmlDbUpdate
{
    internal class XmlDbUpdateActionCorrectionService
    {
        internal XmlDbUpdateRecordedAction CorrectAction(XmlDbUpdateRecordedAction entry)
        {
            var code = entry.BackendAction.EntityType.Code;
            entry.Ids = CorrectListValue(entry.Ids).ToArray();
            if (!string.IsNullOrEmpty(entry.BackendAction.EntityType.ParentCode))
            {
                entry.ParentId = entry.ParentId;
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
                    CorrectObjectForm(entry.Form);
                    break;
                case EntityTypeCode.TemplateObject:
                    CorrectObjectForm(entry.Form);
                    break;
                case EntityTypeCode.Notification:
                    CorrectNotificationForm(entry.Form);
                    break;
            }

            return entry;
        }

        internal XmlDbUpdateRecordedAction EmulateHttpContextRequest(XmlDbUpdateRecordedAction xmlAction, string backendUrl, int userId)
        {
            try
            {
                var correctedAction = CorrectAction(xmlAction);
                FakeMvcApplication.PostAction(correctedAction, backendUrl, userId);
                return correctedAction;
            }
            catch (Exception ex)
            {
                var throwEx = new XmlDbUpdateReplayActionException("Error while replaying xml action.", ex);
                throwEx.Data.Add("ActionToReplay", xmlAction.ToJsonLog());
                throw throwEx;
            }
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

        private static void CorrectContentForm(XmlDbUpdateRecordedAction action)
        {
            CorrectFormValue(action.Form, "Data.WorkflowBinding.WorkflowId");
            CorrectFormValue(action.Form, "Data.ParentContentId");
            CorrectFormValue(action.Form, "Data.GroupId");
        }

        private static void CorrectVirtualContentForm(XmlDbUpdateRecordedAction action)
        {
            CorrectFormValue(action.Form, "Data.JoinRootId");
            CorrectFormValue(action.Form, "Data.UnionSourceContentIDs");
            CorrectFormValue(action.Form, "JoinFields", true, true);
            CorrectFormValue(action.Form, "itemValue", true, true);
        }

        private static void CorrectUserGroupForm(XmlDbUpdateRecordedAction action)
        {
            CorrectFormValue(action.Form, "BindedUserIDs", true);
            CorrectFormValue(action.Form, "ParentGroupId");
        }

        private static void CorrectUserForm(XmlDbUpdateRecordedAction action)
        {
            CorrectFormValue(action.Form, "SelectedGroups", true);
            CorrectFormValue(action.Form, "ContentDefaultFilter.SiteId");
            CorrectFormValue(action.Form, "ContentDefaultFilter.ContentId");
            CorrectFormValue(action.Form, "ContentDefaultFilter.ArticleIDs");
        }

        private static void CorrectFieldForm(NameValueCollection form)
        {
            CorrectFormValue(form, "Data.Id");
            CorrectFormValue(form, "InCombinationWith", true);
            CorrectFormValue(form, "Data.RelateToContentId");
            CorrectFormValue(form, "Data.RelationId");
            CorrectFormValue(form, "Data.BackRelationId");
            CorrectFormValue(form, "Data.ClassifierId");
            CorrectFormValue(form, "Data.BaseImageId");
            CorrectFormValue(form, "Data.ListOrderFieldId");
            CorrectFormValue(form, "Data.TreeOrderFieldId");
            CorrectFormValue(form, "Data.ContentLink.LinkId");
            CorrectFormValue(form, "Data.O2MDefaultValue");
            CorrectFormValue(form, "DefaultArticleIds", true);
            CorrectFormValue(form, "ActiveVeCommands", true);
            CorrectFormValue(form, "ActiveVeStyles", true);
            CorrectFormValue(form, "ActiveVeFormats", true);
        }

        private static void CorrectSiteForm(NameValueCollection form)
        {
            CorrectFormValue(form, "ActiveVeCommands", true);
            CorrectFormValue(form, "ActiveVeStyles", true);
            CorrectFormValue(form, "ActiveVeFormats", true);
        }

        private static void CorrectObjectForm(NameValueCollection form)
        {
            CorrectFormValue(form, "Data.ParentObjectId");
            CorrectFormValue(form, "Data.DefaultFormatId");
            CorrectFormValue(form, "Data.Container.ContentId");
            CorrectFormValue(form, "Data.ContentForm.ContentId");
            CorrectFormValue(form, "Data.ContentForm.ThankYouPageId");
            CorrectFormValue(form, "ActiveStatusTypeIds", true);
        }

        private static void CorrectCustomActionForm(NameValueCollection form)
        {
            CorrectFormValue(form, "SelectedSiteIDs", true);
            CorrectFormValue(form, "SelectedContentIDs", true);
            CorrectFormValue(form, "SelectedActions", true);
        }

        private static void CorrectVePluginForm(NameValueCollection form)
        {
            CorrectFormValue(form, "AggregationListItems_VeCommandsDisplay", "Id");
        }

        private static void CorrectWorkflowForm(NameValueCollection form)
        {
            CorrectFormValue(form, "ActiveStatuses", true);
            CorrectFormValue(form, "ActiveContentIds", true);
            CorrectFormValue(form, "Workflows_WorkflowRulesDisplay", "Id");
            CorrectFormValue(form, "Workflows_WorkflowRulesDisplay", "StId");
            CorrectFormValue(form, "Workflows_WorkflowRulesDisplay", "UserId");
            CorrectFormValue(form, "Workflows_WorkflowRulesDisplay", "GroupId");
        }

        private static void CorrectNotificationForm(NameValueCollection form)
        {
            CorrectFormValue(form, "Data.FormatId");
            CorrectFormValue(form, "Data.UserId");
            CorrectFormValue(form, "Data.GroupId");
            CorrectFormValue(form, "Data.EmailFieldId");
            CorrectFormValue(form, "Data.FromBackenduserId");
            CorrectFormValue(form, "Data.NotifyOnStatusTypeId");
        }

        private static void CorrectArticleForm(NameValueCollection form, int contentId)
        {
            var relations = ReplayHelper.GetRelations(contentId);
            var classifiers = ReplayHelper.GetClasifiers(contentId);
            foreach (var key in form.AllKeys.Where(n => n.StartsWith("field_")))
            {
                int fieldId;
                var parsed = int.TryParse(key.Replace("field_", string.Empty), out fieldId);
                if (parsed)
                {
                    if (relations.ContainsKey(fieldId) || classifiers.ContainsKey(fieldId))
                    {
                        var values = form.GetValues(key);
                        form.Remove(key);

                        if (values != null)
                        {
                            foreach (var value in values)
                            {
                                form.Add(key, CorrectCommaListValue(value));
                            }
                        }
                    }
                }
            }
        }

        private static string CorrectValue(string value)
        {
            int result;
            var parsed = int.TryParse(value, out result);
            return parsed ? result.ToString() : value;
        }

        private static void CorrectFormValue(NameValueCollection form, string formKey, bool prefixSearch = false, bool joinModeReplace = false)
        {
            if (!prefixSearch)
            {
                var formValue = form[formKey];
                if (formValue != null)
                {
                    form[formKey] = CorrectValue(formValue);
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
                                    newValue = CorrectValue(value);
                                }
                                else
                                {
                                    newValue = string.Join(".", value.Replace("[", "").Replace("]", "").Split(".".ToCharArray()).Select(CorrectValue));
                                    newValue = "[" + newValue + "]";
                                }

                                form.Add(key, CorrectValue(newValue));
                            }
                        }
                    }
                }
            }
        }

        private static void CorrectFormValue(NameValueCollection form, string formKey, string jsonKey)
        {
            var formValue = form[formKey];
            if (formValue != null)
            {
                var serializer = new JavaScriptSerializer();
                var collectionList = serializer.Deserialize<List<Dictionary<string, string>>>(formValue);
                foreach (var collection in collectionList.Where(collection => collection.ContainsKey(jsonKey)))
                {
                    collection[jsonKey] = CorrectValue(collection[jsonKey]);
                }

                form[formKey] = serializer.Serialize(collectionList);
            }
        }

        private static string CorrectCommaListValue(string value)
        {
            return string.Join(",", CorrectListValue(value.Split(",".ToCharArray())));
        }

        private static IEnumerable<string> CorrectListValue(IEnumerable<string> values)
        {
            foreach (var value in values)
            {
                int parsedValue;
                var parsed = int.TryParse(value, out parsedValue);
                yield return parsed ? parsedValue.ToString() : value;
            }
        }
    }
}
