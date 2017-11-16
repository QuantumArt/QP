using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Quantumart.QP8.Constants;
using Quantumart.QP8.DAL;

namespace Quantumart.QP8.BLL.Repository
{
    public interface IFingerprintRepository
    {
        void IterateSite(FingerprintEntityTypeSettings querySetting, Action<IEnumerable<object>> rowIterator);

        void IterateContent(FingerprintEntityTypeSettings querySetting, Action<IEnumerable<object>> rowIterator);

        void IterateContentLink(FingerprintEntityTypeSettings querySetting, Action<IEnumerable<object>> rowIterator);

        void IterateField(FingerprintEntityTypeSettings querySetting, Action<IEnumerable<object>> rowIterator);

        void IterateArticle(FingerprintEntityTypeSettings querySetting, Action<IEnumerable<object>> rowIterator);

        void IterateNotification(FingerprintEntityTypeSettings querySetting, Action<IEnumerable<object>> rowIterator);

        void IterateWorkflow(FingerprintEntityTypeSettings querySetting, Action<IEnumerable<object>> rowIterator);

        void IterateStatusType(FingerprintEntityTypeSettings querySetting, Action<IEnumerable<object>> rowIterator);

        void IterateCustomAction(FingerprintEntityTypeSettings querySetting, Action<IEnumerable<object>> rowIterator);

        void IterateVePlugin(FingerprintEntityTypeSettings querySetting, Action<IEnumerable<object>> rowIterator);

        void IterateVeStyle(FingerprintEntityTypeSettings querySetting, Action<IEnumerable<object>> rowIterator);

        void IterateUser(FingerprintEntityTypeSettings querySetting, Action<IEnumerable<object>> rowIterator);

        void IterateUserGroup(FingerprintEntityTypeSettings querySetting, Action<IEnumerable<object>> rowIterator);

        void IterateSiteFolder(FingerprintEntityTypeSettings querySetting, Action<IEnumerable<object>> rowIterator);

        void IterateContentFolder(FingerprintEntityTypeSettings querySetting, Action<IEnumerable<object>> rowIterator);

        void IterateSitePermission(FingerprintEntityTypeSettings querySetting, Action<IEnumerable<object>> rowIterator);

        void IterateContentPermission(FingerprintEntityTypeSettings querySetting, Action<IEnumerable<object>> rowIterator);

        void IterateArticlePermission(FingerprintEntityTypeSettings querySetting, Action<IEnumerable<object>> rowIterator);

        void IterateWorkflowPermission(FingerprintEntityTypeSettings querySetting, Action<IEnumerable<object>> rowIterator);

        void IterateSiteFolderPermission(FingerprintEntityTypeSettings querySetting, Action<IEnumerable<object>> rowIterator);

        void IterateEntityTypePermission(FingerprintEntityTypeSettings querySetting, Action<IEnumerable<object>> rowIterator);

        void IterateActionPermission(FingerprintEntityTypeSettings querySetting, Action<IEnumerable<object>> rowIterator);

        void IterateTemplate(FingerprintEntityTypeSettings querySetting, Action<IEnumerable<object>> rowIterator);

        void IterateTemplateObject(FingerprintEntityTypeSettings querySetting, Action<IEnumerable<object>> rowIterator);

        void IterateTemplateObjectFormat(FingerprintEntityTypeSettings querySetting, Action<IEnumerable<object>> rowIterator);

        void IteratePage(FingerprintEntityTypeSettings querySetting, Action<IEnumerable<object>> rowIterator);

        void IteratePageObject(FingerprintEntityTypeSettings querySetting, Action<IEnumerable<object>> rowIterator);

        void IteratePageObjectFormat(FingerprintEntityTypeSettings querySetting, Action<IEnumerable<object>> rowIterator);

        Dictionary<string, EntityType> GetEntityTypesCodeKeyDictionaty();
    }

    /// <summary>
    /// Настройки для EntityType
    /// </summary>
    public class FingerprintEntityTypeSettings
    {
        public FingerprintEntityTypeSettings()
        {
            IncludedIDs = Enumerable.Empty<int>();
            IncludedParentIDs = Enumerable.Empty<int>();
            ExceptedIDs = Enumerable.Empty<int>();
            ExceptedParentIDs = Enumerable.Empty<int>();
            ConsiderCurentIdentity = false;
        }

        public string EntityTypeCode { get; set; }

        // Не учитываемые ID
        public IEnumerable<int> ExceptedIDs { get; set; }

        // Учитываемые ID
        public IEnumerable<int> IncludedIDs { get; set; }

        // Не учитываемые ParentID
        public IEnumerable<int> ExceptedParentIDs { get; set; }

        // Не учитываемые ParentID
        public IEnumerable<int> IncludedParentIDs { get; set; }

        // Иерархическое ограничение
        public FingerprintAncestorRestrictionTree AncestorRestrictionTree { get; set; }

        // Учитывать ли текущее состояние Identity
        public bool ConsiderCurentIdentity { get; set; }

        public bool HasDirectRestriction => ExceptedIDs.Any() || IncludedIDs.Any() || ExceptedParentIDs.Any() || IncludedParentIDs.Any();

        public bool HasAncestorRestriction => AncestorRestrictionTree != null;
    }

    // Иерархическое ограничение - цепочка предков EntityType до концевого предка + настройки ограничения для концевого предка
    // предок - концевой, если имеет ограничения пo ID/ParentID в конфиге
    // предок - промежуточный, если не имеет ограничений пo ID/ParentID в конфиге
    public class FingerprintAncestorRestrictionTree
    {
        public EntityType EntityType { get; set; }

        public FingerprintAncestorRestrictionTree Parent { get; set; }

        public FingerprintEntityTypeSettings Settings { get; set; }
    }

    public class FingerprintRepository : IFingerprintRepository
    {
        #region IFingerprintRepository Members

        public void IterateSite(FingerprintEntityTypeSettings querySetting, Action<IEnumerable<object>> rowIterator)
        {
            Debug.Assert(querySetting != null);

            IEnumerable<string> queryTemplates = new[]
            {
                "SELECT * FROM [SITE] {0} ORDER BY [SITE].SITE_ID",

                @"SELECT [SITE].SITE_ID, VE_COMMAND_SITE_BIND.* FROM [SITE]
				JOIN VE_COMMAND_SITE_BIND ON [SITE].SITE_ID = VE_COMMAND_SITE_BIND.SITE_ID
				{0}
				ORDER BY [SITE].SITE_ID",

                @"SELECT [SITE].SITE_ID, VE_STYLE_SITE_BIND.* FROM [SITE]
				JOIN VE_STYLE_SITE_BIND ON [SITE].SITE_ID = VE_STYLE_SITE_BIND.SITE_ID
				{0}
				ORDER BY [SITE].SITE_ID"
            };

            var queries = AddFilterStatement("[SITE].SITE_ID", null, querySetting, queryTemplates);
            queries = AddCurrentIdentityStatement("[SITE]", querySetting, queries);
            foreach (var q in queries)
            {
                IterateQuery(q, rowIterator);
            }
        }

        public void IterateContentLink(FingerprintEntityTypeSettings querySetting, Action<IEnumerable<object>> rowIterator)
        {
            IEnumerable<string> queryTemplates = new[]
            {
                "SELECT * FROM [SITE_CONTENT_LINK] {0} ORDER BY [SITE_CONTENT_LINK].LINK_ID"
            };

            var queries = AddFilterStatement("[SITE_CONTENT_LINK].LINK_ID", "[SITE_CONTENT_LINK].SITE_ID", querySetting, queryTemplates);
            queries = AddCurrentIdentityStatement("[CONTENT_TO_CONTENT]", querySetting, queries);
            foreach (var q in queries)
            {
                IterateQuery(q, rowIterator);
            }
        }

        public void IterateContent(FingerprintEntityTypeSettings querySetting, Action<IEnumerable<object>> rowIterator)
        {
            IEnumerable<string> queryTemplates = new[]
            {
                "SELECT  * FROM [CONTENT] {0} ORDER BY [CONTENT].CONTENT_ID",

                @"SELECT [CONTENT].CONTENT_ID,  content_group.* FROM [CONTENT]
				JOIN content_group ON [CONTENT].content_group_id = content_group.content_group_id
				{0}
				ORDER BY [CONTENT].CONTENT_ID",

                @"select [CONTENT].CONTENT_ID, content_constraint.*, content_constraint_rule.*
				FROM [CONTENT]
				JOIN content_constraint ON [CONTENT].CONTENT_ID = content_constraint.content_id
				LEFT JOIN content_constraint_rule ON content_constraint.constraint_id = content_constraint_rule.constraint_id
				{0}
				ORDER BY [CONTENT].CONTENT_ID",

                @"SELECT [CONTENT].CONTENT_ID,  union_contents.* FROM [CONTENT]
				JOIN union_contents ON union_contents.union_content_id = CONTENT.CONTENT_ID OR union_contents.virtual_content_id = CONTENT.CONTENT_ID
				{0}
				ORDER BY [CONTENT].CONTENT_ID",

                @"SELECT [CONTENT].CONTENT_ID,  user_query_contents.* FROM [CONTENT]
				JOIN user_query_contents ON user_query_contents.real_content_id = CONTENT.CONTENT_ID OR user_query_contents.virtual_content_id = CONTENT.CONTENT_ID
				{0}
				ORDER BY [CONTENT].CONTENT_ID",

                @"SELECT content_workflow_bind.* FROM [CONTENT]
				JOIN content_workflow_bind ON content_workflow_bind.CONTENT_ID = CONTENT.CONTENT_ID
				{0}
				ORDER BY [CONTENT].CONTENT_ID"
            };

            var queries = AddFilterStatement("[CONTENT].CONTENT_ID", "[CONTENT].SITE_ID", querySetting, queryTemplates);
            queries = AddCurrentIdentityStatement("[CONTENT]", querySetting, queries);
            foreach (var q in queries)
            {
                IterateQuery(q, rowIterator);
            }
        }

        public void IterateField(FingerprintEntityTypeSettings querySetting, Action<IEnumerable<object>> rowIterator)
        {
            IEnumerable<string> queryTemplates = new[]
            {
                @"select * from CONTENT_ATTRIBUTE_TYPE as CONTENT_ATTRIBUTE {0} ORDER BY CONTENT_ATTRIBUTE.ATTRIBUTE_ID",

                @"SELECT CONTENT_ATTRIBUTE.ATTRIBUTE_ID, content_to_content.* FROM CONTENT_ATTRIBUTE
				JOIN content_to_content ON CONTENT_ATTRIBUTE.link_id = content_to_content.link_id
				{0}
				order by CONTENT_ATTRIBUTE.ATTRIBUTE_ID",

                @"select CONTENT_ATTRIBUTE_TYPE.* from CONTENT_ATTRIBUTE
				JOIN CONTENT_ATTRIBUTE_TYPE ON CONTENT_ATTRIBUTE_TYPE.ATTRIBUTE_ID = CONTENT_ATTRIBUTE.ATTRIBUTE_ID
				{0}
				order by CONTENT_ATTRIBUTE.ATTRIBUTE_ID",

                @"select DYNAMIC_IMAGE_ATTRIBUTE.* from CONTENT_ATTRIBUTE
				JOIN DYNAMIC_IMAGE_ATTRIBUTE ON DYNAMIC_IMAGE_ATTRIBUTE.ATTRIBUTE_ID = CONTENT_ATTRIBUTE.ATTRIBUTE_ID
				{0}
				order by CONTENT_ATTRIBUTE.ATTRIBUTE_ID",

                @"select UNION_ATTRS.* from CONTENT_ATTRIBUTE
				JOIN UNION_ATTRS ON UNION_ATTRS.UNION_ATTR_ID = CONTENT_ATTRIBUTE.ATTRIBUTE_ID OR UNION_ATTRS.virtual_attr_id = CONTENT_ATTRIBUTE.ATTRIBUTE_ID
				{0}
				order by CONTENT_ATTRIBUTE.ATTRIBUTE_ID",

                @"select USER_QUERY_ATTRS.* from CONTENT_ATTRIBUTE
				JOIN USER_QUERY_ATTRS ON USER_QUERY_ATTRS.USER_QUERY_ATTR_ID = CONTENT_ATTRIBUTE.ATTRIBUTE_ID OR USER_QUERY_ATTRS.VIRTUAL_CONTENT_ID = CONTENT_ATTRIBUTE.ATTRIBUTE_ID
				{0}
				order by CONTENT_ATTRIBUTE.ATTRIBUTE_ID",

                @"select VE_COMMAND_FIELD_BIND.* from CONTENT_ATTRIBUTE
				JOIN VE_COMMAND_FIELD_BIND ON VE_COMMAND_FIELD_BIND.FIELD_ID = CONTENT_ATTRIBUTE.ATTRIBUTE_ID
				{0}
				order by CONTENT_ATTRIBUTE.ATTRIBUTE_ID",

                @"select VE_STYLE_FIELD_BIND.* from CONTENT_ATTRIBUTE
				JOIN VE_STYLE_FIELD_BIND ON VE_STYLE_FIELD_BIND.FIELD_ID = CONTENT_ATTRIBUTE.ATTRIBUTE_ID
				{0}
				order by CONTENT_ATTRIBUTE.ATTRIBUTE_ID"
            };

            var queries = AddFilterStatement("[CONTENT_ATTRIBUTE].[ATTRIBUTE_ID]", "[CONTENT_ATTRIBUTE].[CONTENT_ID]", querySetting, queryTemplates);
            queries = AddCurrentIdentityStatement("[CONTENT_ATTRIBUTE]", querySetting, queries);
            foreach (var q in queries)
            {
                IterateQuery(q, rowIterator);
            }
        }

        public void IterateArticle(FingerprintEntityTypeSettings querySetting, Action<IEnumerable<object>> rowIterator)
        {
            IEnumerable<string> queryTemplates = new[]
            {
                @"select * from CONTENT_ITEM {0} order by CONTENT_ITEM_ID",

                @"select CONTENT_DATA.* from CONTENT_ITEM
				JOIN CONTENT_DATA ON CONTENT_DATA.CONTENT_ITEM_ID = CONTENT_ITEM.CONTENT_ITEM_ID
				{0}
				order by CONTENT_ITEM.CONTENT_ITEM_ID",

                @"select CONTENT_ITEM.CONTENT_ITEM_ID, ITEM_TO_ITEM.* from CONTENT_ITEM
				JOIN ITEM_TO_ITEM ON CONTENT_ITEM.CONTENT_ITEM_ID = item_to_item.l_item_id OR CONTENT_ITEM.CONTENT_ITEM_ID = item_to_item.r_item_id
				{0}
				order by CONTENT_ITEM.CONTENT_ITEM_ID",

                @"select CONTENT_ITEM.CONTENT_ITEM_ID, ITEM_LINK_ASYNC.* from CONTENT_ITEM
				JOIN ITEM_LINK_ASYNC ON CONTENT_ITEM.CONTENT_ITEM_ID = ITEM_LINK_ASYNC.item_id OR CONTENT_ITEM.CONTENT_ITEM_ID = ITEM_LINK_ASYNC.linked_item_id
				{0}
				order by CONTENT_ITEM.CONTENT_ITEM_ID",

                @"select CONTENT_ITEM.CONTENT_ITEM_ID, CONTENT_ITEM_VERSION.* FROM CONTENT_ITEM
				JOIN CONTENT_ITEM_VERSION ON CONTENT_ITEM.CONTENT_ITEM_ID = CONTENT_ITEM_VERSION.CONTENT_ITEM_ID
				{0}
				order by CONTENT_ITEM.CONTENT_ITEM_ID",

                @"select CONTENT_ITEM.CONTENT_ITEM_ID, VERSION_CONTENT_DATA.* FROM CONTENT_ITEM
				JOIN CONTENT_ITEM_VERSION ON CONTENT_ITEM.CONTENT_ITEM_ID = CONTENT_ITEM_VERSION.CONTENT_ITEM_ID
				JOIN VERSION_CONTENT_DATA ON CONTENT_ITEM_VERSION.CONTENT_ITEM_VERSION_ID = VERSION_CONTENT_DATA.CONTENT_ITEM_VERSION_ID
				{0}
				order by CONTENT_ITEM.CONTENT_ITEM_ID",

                @"select CONTENT_ITEM.CONTENT_ITEM_ID, ITEM_TO_ITEM_VERSION.* FROM CONTENT_ITEM
				JOIN CONTENT_ITEM_VERSION ON CONTENT_ITEM.CONTENT_ITEM_ID = CONTENT_ITEM_VERSION.CONTENT_ITEM_ID
				JOIN ITEM_TO_ITEM_VERSION ON CONTENT_ITEM_VERSION.CONTENT_ITEM_VERSION_ID = item_to_item_version.content_item_version_id
				{0}
				order by CONTENT_ITEM.CONTENT_ITEM_ID",

                @"select CONTENT_ITEM.CONTENT_ITEM_ID, CONTENT_ITEM_SCHEDULE.*
				FROM CONTENT_ITEM
				JOIN CONTENT_ITEM_SCHEDULE ON CONTENT_ITEM.CONTENT_ITEM_ID = CONTENT_ITEM_SCHEDULE.CONTENT_ITEM_ID
				{0}
				order by CONTENT_ITEM.CONTENT_ITEM_ID",

                @"select WAITING_FOR_APPROVAL.*
				FROM CONTENT_ITEM
				JOIN WAITING_FOR_APPROVAL ON CONTENT_ITEM.CONTENT_ITEM_ID = WAITING_FOR_APPROVAL.CONTENT_ITEM_ID
				{0}
				order by CONTENT_ITEM.CONTENT_ITEM_ID"
            };

            var queries = AddFilterStatement("CONTENT_ITEM.CONTENT_ITEM_ID", "CONTENT_ITEM.CONTENT_ID", querySetting, queryTemplates);
            queries = AddCurrentIdentityStatement("[CONTENT_ITEM]", querySetting, queries);
            foreach (var q in queries)
            {
                IterateQuery(q, rowIterator);
            }
        }

        public void IterateNotification(FingerprintEntityTypeSettings querySetting, Action<IEnumerable<object>> rowIterator)
        {
            IEnumerable<string> queryTemplates = new[]
            {
                @"SELECT * FROM NOTIFICATIONS {0} ORDER BY NOTIFICATIONS.NOTIFICATION_ID",

                @"SELECT notifications_sent.* FROM NOTIFICATIONS
				JOIN notifications_sent ON NOTIFICATIONS.NOTIFICATION_ID = notifications_sent.notification_id
				{0}
				ORDER BY NOTIFICATIONS.NOTIFICATION_ID"
            };

            var queries = AddFilterStatement("NOTIFICATIONS.NOTIFICATION_ID", "NOTIFICATIONS.CONTENT_ID", querySetting, queryTemplates);
            queries = AddCurrentIdentityStatement("[NOTIFICATIONS]", querySetting, queries);
            foreach (var q in queries)
            {
                IterateQuery(q, rowIterator);
            }
        }

        public void IterateWorkflow(FingerprintEntityTypeSettings querySetting, Action<IEnumerable<object>> rowIterator)
        {
            IEnumerable<string> queryTemplates = new[]
            {
                @"SELECT * FROM WORKFLOW
				  LEFT JOIN WORKFLOW_RULES ON WORKFLOW.WORKFLOW_ID = WORKFLOW_RULES.WORKFLOW_ID
				  {0}
				  ORDER BY WORKFLOW.WORKFLOW_ID"
            };

            var queries = AddFilterStatement("workflow.WORKFLOW_ID", "workflow.SITE_ID", querySetting, queryTemplates);
            queries = AddCurrentIdentityStatement("[WORKFLOW]", querySetting, queries);
            foreach (var q in queries)
            {
                IterateQuery(q, rowIterator);
            }
        }

        public void IterateStatusType(FingerprintEntityTypeSettings querySetting, Action<IEnumerable<object>> rowIterator)
        {
            IEnumerable<string> queryTemplates = new[]
            {
                @"SELECT * FROM STATUS_TYPE {0} ORDER BY STATUS_TYPE.STATUS_TYPE_ID"
            };

            var queries = AddFilterStatement("STATUS_TYPE.STATUS_TYPE_ID", "STATUS_TYPE.SITE_ID", querySetting, queryTemplates);
            queries = AddCurrentIdentityStatement("[STATUS_TYPE]", querySetting, queries);
            foreach (var q in queries)
            {
                IterateQuery(q, rowIterator);
            }
        }

        public void IterateCustomAction(FingerprintEntityTypeSettings querySetting, Action<IEnumerable<object>> rowIterator)
        {
            IEnumerable<string> queryTemplates = new[]
            {
                @"SELECT * FROM CUSTOM_ACTION
				{0}
				ORDER BY CUSTOM_ACTION.ID",

                @"SELECT CUSTOM_ACTION.ID, CONTEXT_MENU_ITEM.* FROM CUSTOM_ACTION
				JOIN CONTEXT_MENU_ITEM ON CUSTOM_ACTION.ACTION_ID = CONTEXT_MENU_ITEM.ACTION_ID
				{0}
				ORDER BY CUSTOM_ACTION.ID",

                @"SELECT CUSTOM_ACTION.ID, ACTION_TOOLBAR_BUTTON.* FROM CUSTOM_ACTION
				JOIN ACTION_TOOLBAR_BUTTON ON CUSTOM_ACTION.ACTION_ID = ACTION_TOOLBAR_BUTTON.ACTION_ID
				{0}
				ORDER BY CUSTOM_ACTION.ID",

                @"SELECT ACTION_CONTENT_BIND.* FROM CUSTOM_ACTION
				JOIN ACTION_CONTENT_BIND ON CUSTOM_ACTION.ID = ACTION_CONTENT_BIND.CUSTOM_ACTION_ID
				{0}
				ORDER BY CUSTOM_ACTION.ID",

                @"SELECT ACTION_SITE_BIND.* FROM CUSTOM_ACTION
				JOIN ACTION_SITE_BIND ON CUSTOM_ACTION.ID = ACTION_SITE_BIND.CUSTOM_ACTION_ID
				{0}
				ORDER BY CUSTOM_ACTION.ID"
            };

            var queries = AddFilterStatement("CUSTOM_ACTION.ID", null, querySetting, queryTemplates);
            queries = AddCurrentIdentityStatement("[CUSTOM_ACTION]", querySetting, queries);
            foreach (var q in queries)
            {
                IterateQuery(q, rowIterator);
            }
        }

        public void IterateVePlugin(FingerprintEntityTypeSettings querySetting, Action<IEnumerable<object>> rowIterator)
        {
            IEnumerable<string> queryTemplates = new[]
            {
                @"SELECT * FROM VE_PLUGIN
				{0}
				ORDER BY VE_PLUGIN.ID",

                @"SELECT VE_COMMAND.* FROM VE_PLUGIN
				JOIN VE_COMMAND ON VE_PLUGIN.ID = PLUGIN_ID
				{0}
				ORDER BY VE_PLUGIN.ID"
            };

            var queries = AddFilterStatement("VE_PLUGIN.ID", null, querySetting, queryTemplates);
            queries = AddCurrentIdentityStatement("[VE_PLUGIN]", querySetting, queries);
            foreach (var q in queries)
            {
                IterateQuery(q, rowIterator);
            }
        }

        public void IterateVeStyle(FingerprintEntityTypeSettings querySetting, Action<IEnumerable<object>> rowIterator)
        {
            IEnumerable<string> queryTemplates = new[]
            {
                @"select * from VE_STYLE {0} order BY VE_STYLE.ID"
            };

            var queries = AddFilterStatement("VE_STYLE.ID", null, querySetting, queryTemplates);
            queries = AddCurrentIdentityStatement("[VE_STYLE]", querySetting, queries);
            foreach (var q in queries)
            {
                IterateQuery(q, rowIterator);
            }
        }

        public void IterateUser(FingerprintEntityTypeSettings querySetting, Action<IEnumerable<object>> rowIterator)
        {
            IEnumerable<string> queryTemplates = new[]
            {
                @"SELECT * FROM [USERS] {0} ORDER BY [USERS].[USER_ID]",
                @"SELECT USER_GROUP_BIND.* FROM [USERS]
				JOIN USER_GROUP_BIND ON USER_GROUP_BIND.[USER_ID] = USERS.[USER_ID]
				{0}
				ORDER BY [USERS].[USER_ID]",

                @"SELECT USER_DEFAULT_FILTER.* FROM [USERS]
				JOIN USER_DEFAULT_FILTER ON USER_DEFAULT_FILTER.[USER_ID] = USERS.[USER_ID]
				{0}
				ORDER BY [USERS].[USER_ID]"
            };

            var queries = AddFilterStatement("[USERS].[USER_ID]", null, querySetting, queryTemplates);
            queries = AddCurrentIdentityStatement("[USERS]", querySetting, queries);
            foreach (var q in queries)
            {
                IterateQuery(q, rowIterator);
            }
        }

        public void IterateUserGroup(FingerprintEntityTypeSettings querySetting, Action<IEnumerable<object>> rowIterator)
        {
            IEnumerable<string> queryTemplates = new[]
            {
                @"SELECT * FROM USER_GROUP {0} ORDER BY USER_GROUP.GROUP_ID"
            };

            var queries = AddFilterStatement("USER_GROUP.GROUP_ID", null, querySetting, queryTemplates);
            queries = AddCurrentIdentityStatement("[USER_GROUP]", querySetting, queries);
            foreach (var q in queries)
            {
                IterateQuery(q, rowIterator);
            }
        }

        public void IterateSiteFolder(FingerprintEntityTypeSettings querySetting, Action<IEnumerable<object>> rowIterator)
        {
            IEnumerable<string> queryTemplates = new[]
            {
                @"SELECT * FROM FOLDER {0} ORDER BY FOLDER.FOLDER_ID"
            };
            var queries = AddFilterStatement("FOLDER.FOLDER_ID", "FOLDER.SITE_ID", querySetting, queryTemplates);
            queries = AddCurrentIdentityStatement("[FOLDER]", querySetting, queries);
            foreach (var q in queries)
            {
                IterateQuery(q, rowIterator);
            }
        }

        public void IterateContentFolder(FingerprintEntityTypeSettings querySetting, Action<IEnumerable<object>> rowIterator)
        {
            IEnumerable<string> queryTemplates = new[]
            {
                @"SELECT * FROM CONTENT_FOLDER {0} ORDER BY CONTENT_FOLDER.FOLDER_ID"
            };

            var queries = AddFilterStatement("CONTENT_FOLDER.FOLDER_ID", "CONTENT_FOLDER.CONTENT_ID", querySetting, queryTemplates);
            queries = AddCurrentIdentityStatement("[CONTENT_FOLDER]", querySetting, queries);
            foreach (var q in queries)
            {
                IterateQuery(q, rowIterator);
            }
        }

        public void IterateSitePermission(FingerprintEntityTypeSettings querySetting, Action<IEnumerable<object>> rowIterator)
        {
            IEnumerable<string> queryTemplates = new[]
            {
                @"SELECT * FROM SITE_ACCESS {0} ORDER BY SITE_ACCESS_ID"
            };
            var queries = AddFilterStatement("SITE_ACCESS_ID", "SITE_ID", querySetting, queryTemplates);
            queries = AddCurrentIdentityStatement("[SITE_ACCESS]", querySetting, queries);
            foreach (var q in queries)
            {
                IterateQuery(q, rowIterator);
            }
        }

        public void IterateContentPermission(FingerprintEntityTypeSettings querySetting, Action<IEnumerable<object>> rowIterator)
        {
            IEnumerable<string> queryTemplates = new[]
            {
                @"SELECT * FROM CONTENT_ACCESS {0} ORDER BY CONTENT_ACCESS_ID"
            };

            var queries = AddFilterStatement("CONTENT_ACCESS_ID", "CONTENT_ID", querySetting, queryTemplates);
            queries = AddCurrentIdentityStatement("[CONTENT_ACCESS]", querySetting, queries);
            foreach (var q in queries)
            {
                IterateQuery(q, rowIterator);
            }
        }

        public void IterateArticlePermission(FingerprintEntityTypeSettings querySetting, Action<IEnumerable<object>> rowIterator)
        {
            IEnumerable<string> queryTemplates = new[]
            {
                @"SELECT * FROM CONTENT_ITEM_ACCESS {0} ORDER BY CONTENT_ITEM_ACCESS_ID"
            };

            var queries = AddFilterStatement("CONTENT_ITEM_ACCESS_ID", FieldName.ContentItemId, querySetting, queryTemplates);
            queries = AddCurrentIdentityStatement("[CONTENT_ITEM_ACCESS]", querySetting, queries);
            foreach (var q in queries)
            {
                IterateQuery(q, rowIterator);
            }
        }

        public void IterateWorkflowPermission(FingerprintEntityTypeSettings querySetting, Action<IEnumerable<object>> rowIterator)
        {
            IEnumerable<string> queryTemplates = new[]
            {
                @"SELECT * FROM WORKFLOW_ACCESS {0} ORDER BY WORKFLOW_ACCESS_ACCESS_ID"
            };

            var queries = AddFilterStatement("WORKFLOW_ACCESS_ACCESS_ID", "WORKFLOW_ID", querySetting, queryTemplates);
            queries = AddCurrentIdentityStatement("[WORKFLOW_ACCESS]", querySetting, queries);
            foreach (var q in queries)
            {
                IterateQuery(q, rowIterator);
            }
        }

        public void IterateSiteFolderPermission(FingerprintEntityTypeSettings querySetting, Action<IEnumerable<object>> rowIterator)
        {
            IEnumerable<string> queryTemplates = new[]
            {
                @"SELECT * FROM FOLDER_ACCESS {0} ORDER BY FOLDER_ACCESS_ID"
            };

            var queries = AddFilterStatement("FOLDER_ACCESS_ID", "FOLDER_ID", querySetting, queryTemplates);
            queries = AddCurrentIdentityStatement("[FOLDER_ACCESS]", querySetting, queries);
            foreach (var q in queries)
            {
                IterateQuery(q, rowIterator);
            }
        }

        public void IterateEntityTypePermission(FingerprintEntityTypeSettings querySetting, Action<IEnumerable<object>> rowIterator)
        {
            IEnumerable<string> queryTemplates = new[]
            {
                @"SELECT * FROM ENTITY_TYPE_ACCESS {0} ORDER BY ENTITY_TYPE_ACCESS_ID"
            };
            var queries = AddFilterStatement("ENTITY_TYPE_ACCESS_ID", "ENTITY_TYPE_ID", querySetting, queryTemplates);
            queries = AddCurrentIdentityStatement("[ENTITY_TYPE_ACCESS]", querySetting, queries);
            foreach (var q in queries)
            {
                IterateQuery(q, rowIterator);
            }
        }

        public void IterateActionPermission(FingerprintEntityTypeSettings querySetting, Action<IEnumerable<object>> rowIterator)
        {
            IEnumerable<string> queryTemplates = new[]
            {
                @"SELECT * FROM ACTION_ACCESS {0} ORDER BY ACTION_ACCESS_ID"
            };
            var queries = AddFilterStatement("ACTION_ACCESS_ID", "ACTION_ID", querySetting, queryTemplates);
            queries = AddCurrentIdentityStatement("[ACTION_ACCESS]", querySetting, queries);
            foreach (var q in queries)
            {
                IterateQuery(q, rowIterator);
            }
        }

        public void IterateTemplate(FingerprintEntityTypeSettings querySetting, Action<IEnumerable<object>> rowIterator)
        {
            IEnumerable<string> queryTemplates = new[]
            {
                @"select * FROM [PAGE_TEMPLATE] {0} ORDER BY [PAGE_TEMPLATE].PAGE_TEMPLATE_ID"
            };
            var queries = AddFilterStatement("[PAGE_TEMPLATE].PAGE_TEMPLATE_ID", "[PAGE_TEMPLATE].SITE_ID", querySetting, queryTemplates);
            queries = AddCurrentIdentityStatement("[PAGE_TEMPLATE]", querySetting, queries);
            foreach (var q in queries)
            {
                IterateQuery(q, rowIterator);
            }
        }

        public void IterateTemplateObject(FingerprintEntityTypeSettings querySetting, Action<IEnumerable<object>> rowIterator)
        {
            IEnumerable<string> queryTemplates = new[]
            {
                @"SELECT * FROM [OBJECT] WHERE [OBJECT].PAGE_ID IS NULL {0} ORDER BY [OBJECT].[OBJECT_ID]"
            };
            var queries = AddFilterStatement("[OBJECT].[OBJECT_ID]", "[OBJECT].[PAGE_TEMPLATE_ID]", querySetting, queryTemplates, false);
            queries = AddCurrentIdentityStatement("[OBJECT]", querySetting, queries);
            foreach (var q in queries)
            {
                IterateQuery(q, rowIterator);
            }
        }

        public void IterateTemplateObjectFormat(FingerprintEntityTypeSettings querySetting, Action<IEnumerable<object>> rowIterator)
        {
            IEnumerable<string> queryTemplates = new[]
            {
                @"SELECT OBJECT_FORMAT.* FROM OBJECT_FORMAT
				JOIN [OBJECT] ON [OBJECT].[OBJECT_ID] = [OBJECT_FORMAT].[OBJECT_ID]
				WHERE [OBJECT].PAGE_ID IS NULL {0}
				ORDER BY [OBJECT_FORMAT].OBJECT_FORMAT_ID"
            };
            var queries = AddFilterStatement("[OBJECT_FORMAT].[OBJECT_FORMAT_ID]", "[OBJECT_FORMAT].[OBJECT_ID]", querySetting, queryTemplates, false);
            queries = AddCurrentIdentityStatement("[OBJECT_FORMAT]", querySetting, queries);
            foreach (var q in queries)
            {
                IterateQuery(q, rowIterator);
            }
        }

        public void IteratePage(FingerprintEntityTypeSettings querySetting, Action<IEnumerable<object>> rowIterator)
        {
            IEnumerable<string> queryTemplates = new[]
            {
                @"SELECT * FROM [PAGE] {0} ORDER BY [PAGE].PAGE_ID"
            };
            var queries = AddFilterStatement("[PAGE].PAGE_ID", "[PAGE].PAGE_TEMPLATE_ID", querySetting, queryTemplates);
            queries = AddCurrentIdentityStatement("[PAGE]", querySetting, queries);
            foreach (var q in queries)
            {
                IterateQuery(q, rowIterator);
            }
        }

        public void IteratePageObject(FingerprintEntityTypeSettings querySetting, Action<IEnumerable<object>> rowIterator)
        {
            IEnumerable<string> queryTemplates = new[]
            {
                @"SELECT * FROM [OBJECT] WHERE [OBJECT].PAGE_ID IS NOT NULL {0} ORDER BY [OBJECT].[OBJECT_ID]"
            };
            var queries = AddFilterStatement("[OBJECT].[OBJECT_ID]", "[OBJECT].[PAGE_ID]", querySetting, queryTemplates, false);
            queries = AddCurrentIdentityStatement("[OBJECT]", querySetting, queries);
            foreach (var q in queries)
            {
                IterateQuery(q, rowIterator);
            }
        }

        public void IteratePageObjectFormat(FingerprintEntityTypeSettings querySetting, Action<IEnumerable<object>> rowIterator)
        {
            IEnumerable<string> queryTemplates = new[]
            {
                @"SELECT OBJECT_FORMAT.* FROM OBJECT_FORMAT
				JOIN [OBJECT] ON [OBJECT].[OBJECT_ID] = [OBJECT_FORMAT].[OBJECT_ID]
				WHERE [OBJECT].PAGE_ID IS NOT NULL {0}
				ORDER BY [OBJECT_FORMAT].OBJECT_FORMAT_ID"
            };
            var queries = AddFilterStatement("[OBJECT_FORMAT].[OBJECT_FORMAT_ID]", "[OBJECT_FORMAT].[OBJECT_ID]", querySetting, queryTemplates, false);
            queries = AddCurrentIdentityStatement("[OBJECT_FORMAT]", querySetting, queries);
            foreach (var q in queries)
            {
                IterateQuery(q, rowIterator);
            }
        }

        public Dictionary<string, EntityType> GetEntityTypesCodeKeyDictionaty()
        {
            return EntityTypeRepository.GetList().ToDictionary(et => et.Code, StringComparer.CurrentCultureIgnoreCase);
        }

        #endregion

        private static void IterateQuery(string query, Action<IEnumerable<object>> rowIterator)
        {
            using (var scope = new QPConnectionScope())
            {
                Common.IterateRows(scope.DbConnection, query, rowIterator);
            }
        }

        /// <summary>
        /// Добавить в запросы фильтрацию по ID и ParentID
        /// </summary>
        internal static IEnumerable<string> AddFilterStatement(string fieldId, string fieldParentId, FingerprintEntityTypeSettings querySetting, IEnumerable<string> queryTemplates, bool addWhere = true)
        {
            Debug.Assert(querySetting != null);

            var filter = " ";
            if (querySetting.HasDirectRestriction)
            {
                filter = GetDirectRestrictionFilter(fieldId, fieldParentId, querySetting);
            }
            else if (querySetting.HasAncestorRestriction)
            {
                filter = GetAncestorRestrictionFilter(fieldParentId, querySetting);
            }

            if (!string.IsNullOrWhiteSpace(filter))
            {
                filter = (addWhere ? " WHERE " : " AND ") + filter + " ";
            }
            else
            {
                filter = " ";
            }
            return queryTemplates.Select(t => string.Format(t, filter)).ToArray();
        }

        private static string GetAncestorRestrictionFilter(string fieldParentId, FingerprintEntityTypeSettings querySetting)
        {
            Debug.Assert(querySetting != null);
            Debug.Assert(!string.IsNullOrWhiteSpace(fieldParentId));
            Debug.Assert(querySetting.HasAncestorRestriction, "There isn't ancestor restriction for " + querySetting.EntityTypeCode);

            var sb = new StringBuilder();

            // для первого ancestor - select
            sb.AppendFormat(" {0} IN (SELECT [{2}].[{1}] from [{2}] ",
                fieldParentId,
                querySetting.AncestorRestrictionTree.EntityType.IdField,
                querySetting.AncestorRestrictionTree.EntityType.Source
            );

            // для всех у которых есть родитель - join
            // если нашли Settings, то генерим where и останавливаемся
            Action<FingerprintAncestorRestrictionTree> generateJoins = null;
            generateJoins = ar =>
            {
                if (ar.Parent != null)
                {
                    sb.AppendFormat(" JOIN [{0}] ON [{0}].[{1}] = [{2}].[{3}] ",
                        ar.Parent.EntityType.Source, ar.Parent.EntityType.IdField,
                        ar.EntityType.Source, ar.EntityType.ParentIdField
                    );

                    generateJoins(ar.Parent);
                }
                else if (ar.Settings != null)
                {
                    var restFilter = AddFilterStatement(
                        $"[{ar.EntityType.Source}].[{ar.EntityType.IdField}]",
                        $"[{ar.EntityType.Source}].[{ar.EntityType.ParentIdField}]",
                        ar.Settings, new[] { "{0}" }).First();

                    sb.AppendFormat(" {0} ", restFilter);
                }
            };

            generateJoins(querySetting.AncestorRestrictionTree);
            sb.Append(") ");
            return sb.ToString();
        }

        private static string GetDirectRestrictionFilter(string fieldId, string fieldParentId, FingerprintEntityTypeSettings querySetting)
        {
            Debug.Assert(querySetting != null);
            Debug.Assert(querySetting.HasDirectRestriction, "There are no any direction restriction for " + querySetting.EntityTypeCode);

            string idFilter = null;
            string parentIdFilter = null;
            if (!string.IsNullOrWhiteSpace(fieldId))
            {
                if (querySetting.IncludedIDs.Any())
                {
                    idFilter = $"{fieldId} IN ({string.Join(",", querySetting.IncludedIDs)})";
                }
                else if (querySetting.ExceptedIDs.Any())
                {
                    idFilter = $"{fieldId} NOT IN ({string.Join(",", querySetting.ExceptedIDs)})";
                }
            }

            if (!string.IsNullOrWhiteSpace(fieldParentId))
            {
                if (querySetting.IncludedParentIDs.Any())
                {
                    parentIdFilter = $"{fieldParentId} IN ({string.Join(",", querySetting.IncludedParentIDs)})";
                }
                else if (querySetting.ExceptedParentIDs.Any())
                {
                    parentIdFilter = $"{fieldParentId} NOT IN ({string.Join(",", querySetting.ExceptedParentIDs)})";
                }
            }

            return string.Join(" AND ", new[] { idFilter, parentIdFilter }.Where(f => !string.IsNullOrWhiteSpace(f)));
        }

        internal static IEnumerable<string> AddCurrentIdentityStatement(string tableName, FingerprintEntityTypeSettings querySetting, IEnumerable<string> queryTemplates)
        {
            if (string.IsNullOrWhiteSpace(tableName))
            {
                throw new ArgumentNullException(nameof(tableName));
            }

            return querySetting.ConsiderCurentIdentity
                ? queryTemplates.Concat(new[] { $"SELECT IDENT_CURRENT('{tableName}')" }).ToArray()
                : queryTemplates;
        }
    }
}
