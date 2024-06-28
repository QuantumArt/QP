using System.Collections.Generic;

namespace Quantumart.QP8.Constants
{
    /// <summary>
    /// Коды типов действий
    /// </summary>
    public static class ActionCode
    {
        public const string None = "";

        public const string EditProfile = "edit_profile";
        public const string UpdateProfile = "update_profile";
        public const string Home = "home";
        public const string About = "about";
        public const string ScheduledTasks = "scheduled_tasks";

        public const string DbSettings = "db_settings";
        public const string UpdateDbSettings = "update_db_settings";

        public const string Sites = "list_site";
        public const string MultipleSelectSites = "multiple_select_site";
        public const string AddNewSite = "new_site";
        public const string RefreshSites = "refresh_sites";
        public const string SaveAndEditSite = "save_edit_site";
        public const string SiteProperties = "edit_site";
        public const string SaveSite = "save_site";
        public const string UpdateSite = "update_site";
        public const string RemoveSite = "remove_site";
        public const string MultipleRemoveSite = "multiple_remove_site";
        public const string AssembleSite = "assemble_site";
        public const string AssembleContents = "assemble_contents";
        public const string CaptureLockSite = "capture_lock_site";
        public const string CancelSite = "cancel_site";
        public const string SearchInArticles = "search_in_articles";
        public const string SearchInCode = "search_in_code";
        public const string SiteLibrary = "site_library";
        public const string PopupSiteLibrary = "popup_site_library";
        public const string SearchInTemplates = "search_in_templates";
        public const string SearchInObjects = "search_in_objects";
        public const string SimpleRemoveSite = "simple_remove_site";
        public const string CreateLikeSite = "copy_site";

        public const string Articles = "list_article";
        public const string SelectArticle = "select_article";
        public const string MultipleSelectArticle = "multiple_select_article";
        public const string AddNewArticle = "new_article";
        public const string AddNewChildArticle = "new_child_article";
        public const string EditArticle = "edit_article";
        public const string ViewLiveArticle = "view_live_article";
        public const string ViewVirtualArticle = "view_virtual_article";
        public const string ViewArchiveArticle = "view_archive_article";
        public const string SaveArticle = "save_article";
        public const string SaveArticleAndUp = "save_article_and_up";
        public const string UpdateArticle = "update_article";
        public const string UpdateArticleAndUp = "update_article_and_up";
        public const string MoveArticleToArchive = "move_to_archive_article";
        public const string MultipleMoveArticleToArchive = "multiple_move_to_archive_article";
        public const string MultipleUpdateArticles = "multiple_update_article";
        public const string MultipleSaveArticles = "multiple_save_article";
        public const string MultiplePublishArticles = "multiple_publish_articles";
        public const string RestoreArticleFromArchive = "restore_from_archive_article";
        public const string MultipleRestoreArticleFromArchive = "multiple_restore_from_archive_article";
        public const string RefreshArticle = "refresh_article";
        public const string RefreshArticles = "refresh_articles";
        public const string CaptureLockArticle = "capture_lock_article";
        public const string CreateLikeArticle = "copy_article";
        public const string RemoveArticle = "remove_article";
        public const string MultipleRemoveArticle = "multiple_remove_article";
        public const string MultipleRemoveArticleFromArchive = "multiple_remove_archive_article";
        public const string CancelArticle = "cancel_article";
        public const string LockedArticles = "list_locked_article";
        public const string UnlockArticles = "unlock_articles";
        public const string ArticlesForApproval = "list_articles_for_approval";
        public const string ExportArticles = "export_articles";
        public const string ExportSelectedArticles = "multiple_export_article";
        public const string ExportArchiveArticles = "export_archive_article";
        public const string ExportSelectedArchiveArticles = "multiple_export_archive_article";

        public const string ImportArticles = "import_articles";

        public const string VirtualArticles = "list_virtual_article";
        public const string ArchiveArticles = "list_archive_article";

        public const string SelectChildArticles = "select_child_articles";
        public const string UnselectChildArticles = "unselect_child_articles";

        public const string ArticleVersions = "list_article_version";
        public const string PreviewArticleVersion = "preview_article_version";
        public const string CompareArticleVersionWithCurrent = "compare_article_version_with_current";
        public const string CompareArticleLiveWithCurrent = "compare_article_live_with_current";
        public const string CompareArticleVersions = "compare_article_versions";
        public const string RestoreArticleVersion = "restore_article_version";
        public const string RemoveArticleVersion = "remove_article_version";
        public const string MultipleRemoveArticleVersion = "multiple_remove_article_version";
        public const string CancelArticleVersion = "cancel_article_version";
        public const string RefreshArticleVersions = "refresh_article_versions";
        public const string RefreshArticleVersion = "refresh_article_version";
        public const string ArticleStatus = "list_status_history";

        public const string Contents = "list_content";

        public const string SelectContent = "select_content";
        public const string SelectContentForObjectContainer = "select_content_for_object_container";
        public const string SelectContentForObjectForm = "select_content_for_object_form";
        public const string SelectContentForField = "select_content_for_field";
        public const string SelectContentForJoin = "select_content_for_join";

        public const string MultipleSelectContent = "multiple_select_content";
        public const string MultipleSelectContentForCustomAction = "multiple_select_content_for_custom_action";
        public const string MultipleSelectContentForUnion = "multiple_select_content_for_union";
        public const string MultipleSelectContentForWorkflow = "multiple_select_content_for_workflow";
        public const string AddNewContent = "new_content";
        public const string ContentProperties = "edit_content";
        public const string SaveContent = "save_content";
        public const string UpdateContent = "update_content";
        public const string RemoveContent = "remove_content";
        public const string MultipleRemoveContent = "multiple_remove_content";
        public const string SimpleRemoveContent = "simple_remove_content";
        public const string ContentLibrary = "content_library";
        public const string PopupContentLibrary = "popup_content_library";
        public const string CreateLikeContent = "copy_content";
        public const string ClearContent = "clear_content";
        public const string EnableArticlesPermissions = "enable_article_permissions";

        public const string AddNewContentGroup = "new_content_group";
        public const string ContentGroupProperties = "edit_content_group";
        public const string SaveContentGroup = "save_content_group";
        public const string UpdateContentGroup = "update_content_group";

        public const string Fields = "list_field";
        public const string AddNewField = "new_field";
        public const string AddNewAdjacentField = "new_adjacent_field";
        public const string FieldProperties = "edit_field";
        public const string SaveField = "save_field";
        public const string UpdateField = "update_field";
        public const string RemoveField = "remove_field";
        public const string MultipleRemoveField = "multiple_remove_field";
        public const string ApplyFieldDefaultValue = "apply_field_default_value";
        public const string RecreateDynamicImages = "recreate_dynamic_images";
        public const string CreateLikeField = "copy_field";
        public const string MultipleSelectFieldForExport = "multiple_select_field_for_export";
        public const string MultipleSelectFieldForExportExpanded = "multiple_select_field_for_export_expanded";

        public const string VirtualFields = "list_virtual_field";
        public const string VirtualFieldProperties = "edit_virtual_field";
        public const string UpdateVirtualField = "update_virtual_field";

        public const string VirtualContents = "list_virtual_content";
        public const string AddNewVirtualContents = "new_virtual_content";
        public const string VirtualContentProperties = "edit_virtual_content";
        public const string RemoveVirtualContent = "remove_virtual_content";
        public const string SaveVirtualContent = "save_virtual_content";
        public const string UpdateVirtualContent = "update_virtual_content";
        public const string RebuildVirtualContents = "rebuild_virtual_contents";

        public const string SiteFolder = "list_site_folder";
        public const string SiteFolderProperties = "edit_site_folder";
        public const string AddNewSiteFolder = "new_site_folder";
        public const string SaveSiteFolder = "save_site_folder";
        public const string RemoveSiteFolder = "remove_site_folder";
        public const string UpdateSiteFolder = "update_site_folder";

        public const string ContentFolder = "list_content_folder";
        public const string ContentFolderProperties = "edit_content_folder";
        public const string AddNewContentFolder = "new_content_folder";
        public const string RemoveContentFolder = "remove_content_folder";
        public const string UpdateContentFolder = "update_content_folder";

        public const string SiteFileProperties = "edit_site_file";
        public const string UpdateSiteFile = "update_site_file";
        public const string MultipleRemoveSiteFile = "multiple_remove_site_file";
        public const string RemoveSiteFile = "remove_site_file";
        public const string UploadSiteFile = "upload_site_file";

        public const string ContentFileProperties = "edit_content_file";
        public const string UpdateContentFile = "update_content_file";
        public const string MultipleRemoveContentFile = "multiple_remove_content_file";
        public const string RemoveContentFile = "remove_content_file";
        public const string UploadContentFile = "upload_content_file";

        public const string ActionLog = "list_action_log";
        public const string ButtonTrace = "list_button_trace";
        public const string RemovedEntities = "list_removed_entities";
        public const string SuccessfulSession = "list_successful_sessions";
        public const string FailedSession = "list_failed_sessions";

        public const string CustomActions = "list_custom_action";
        public const string AddNewCustomAction = "new_custom_action";
        public const string CustomActionsProperties = "edit_custom_action";
        public const string SaveCustomAction = "save_custom_action";
        public const string UpdateCustomAction = "update_custom_action";
        public const string RemoveCustomAction = "remove_custom_action";
        public const string MultipleRemoveCustomAction = "multiple_remove_custom_action";
        public const string CreateLikeCustomAction = "copy_custom_action";
        public const string RefreshCustomAction = "refresh_custom_action";
        public const string ActionSettings = "action_settings";

        public const string Users = "list_user";
        public const string AddNewUser = "new_user";
        public const string SaveUser = "save_user";
        public const string CreateLikeUser = "copy_user";
        public const string UserProperties = "edit_user";
        public const string UpdateUser = "update_user";
        public const string RemoveUser = "remove_user";
        public const string MultipleSelectUser = "multiple_select_user";
        public const string SelectUser = "select_user";

        public const string UserGroups = "list_user_group";
        public const string AddNewUserGroup = "new_user_group";
        public const string SaveUserGroup = "save_user_group";
        public const string CreateLikeUserGroup = "copy_user_group";
        public const string UserGroupProperties = "edit_user_group";
        public const string UpdateUserGroup = "update_user_group";
        public const string RemoveUserGroup = "remove_user_group";
        public const string SelectUserGroup = "select_user_group";

        public const string SitePermissions = "list_site_permission";
        public const string AddNewSitePermission = "new_site_permission";
        public const string SaveSitePermission = "save_site_permission";
        public const string SitePermissionProperties = "edit_site_permission";
        public const string UpdateSitePermission = "update_site_permission";
        public const string RemoveSitePermission = "remove_site_permission";
        public const string MultipleRemoveSitePermission = "multiple_remove_site_permission";

        public const string ContentPermissions = "list_content_permission";
        public const string ContentPermissionsForChild = "list_content_permission_for_child";
        public const string AddNewContentPermission = "new_content_permission";
        public const string SaveContentPermission = "save_content_permission";
        public const string ContentPermissionProperties = "edit_content_permission";
        public const string UpdateContentPermission = "update_content_permission";
        public const string RemoveContentPermission = "remove_content_permission";
        public const string MultipleRemoveContentPermission = "multiple_remove_content_permission";
        public const string ChildContentPermissions = "list_child_content_permission";
        public const string MultipleChangeChildContentPermissions = "multiple_change_child_content_permission";
        public const string MultipleRemoveChildContentPermissions = "multiple_remove_child_content_permission";
        public const string ChangeAllChildContentPermissions = "change_all_child_content_permission";
        public const string RemoveAllChildContentPermissions = "remove_all_child_content_permission";
        public const string ChangeChildContentPermission = "change_child_content_permission";
        public const string RemoveChildContentPermission = "remove_child_content_permission";
        public const string SaveChildContentPermission = "save_child_content_permission";

        public const string ArticlePermissions = "list_article_permission";
        public const string ArticlePermissionsForChild = "list_article_permission_for_child";
        public const string AddNewArticlePermission = "new_article_permission";
        public const string SaveArticlePermission = "save_article_permission";
        public const string ArticlePermissionProperties = "edit_article_permission";
        public const string UpdateArticlePermission = "update_article_permission";
        public const string RemoveArticlePermission = "remove_article_permission";
        public const string MultipleRemoveArticlePermission = "multiple_remove_article_permission";
        public const string ChildArticlePermissions = "list_child_article_permission";
        public const string MultipleChangeChildArticlePermissions = "multiple_change_child_article_permission";
        public const string MultipleRemoveChildArticlePermissions = "multiple_remove_child_article_permission";
        public const string ChangeAllChildArticlePermissions = "change_all_child_article_permission";
        public const string RemoveAllChildArticlePermissions = "remove_all_child_article_permission";
        public const string ChangeChildArticlePermission = "change_child_article_permission";
        public const string RemoveChildArticlePermission = "remove_child_article_permission";
        public const string SaveChildArticlePermission = "save_child_article_permission";

        public const string WorkflowPermissions = "list_workflow_permission";
        public const string AddNewWorkflowPermission = "new_workflow_permission";
        public const string SaveWorkflowPermission = "save_workflow_permission";
        public const string WorkflowPermissionProperties = "edit_workflow_permission";
        public const string UpdateWorkflowPermission = "update_workflow_permission";
        public const string RemoveWorkflowPermission = "remove_workflow_permission";
        public const string MultipleRemoveWorkflowPermission = "multiple_remove_workflow_permission";

        public const string SiteFolderPermissions = "list_site_folder_permission";
        public const string AddNewSiteFolderPermission = "new_site_folder_permission";
        public const string SaveSiteFolderPermission = "save_site_folder_permission";
        public const string SiteFolderPermissionProperties = "edit_site_folder_permission";
        public const string UpdateSiteFolderPermission = "update_site_folder_permission";
        public const string RemoveSiteFolderPermission = "remove_site_folder_permission";
        public const string MultipleRemoveSiteFolderPermission = "multiple_remove_site_folder_permission";

        public const string VisualEditorStyles = "list_visual_editor_style";
        public const string AddNewVisualEditorStyle = "new_visual_editor_style";
        public const string VisualEditorStyleProperties = "edit_visual_editor_style";
        public const string UpdateVisualEditorStyle = "update_visual_editor_style";
        public const string RemoveVisualEditorStyle = "remove_visual_editor_style";
        public const string SaveVisualEditorStyle = "save_visual_editor_style";

        public const string AddNewWorkflow = "new_workflow";
        public const string Workflows = "list_workflow";
        public const string WorkflowProperties = "edit_workflow";
        public const string UpdateWorkflow = "update_workflow";
        public const string RemoveWorkflow = "remove_workflow";
        public const string SaveWorkflow = "save_workflow";

        public const string AddNewStatusType = "new_status_type";
        public const string StatusTypes = "list_status_type";
        public const string StatusTypeProperties = "edit_status_type";
        public const string UpdateStatusType = "update_status_type";
        public const string SaveStatusType = "save_status_type";
        public const string RemoveStatusType = "remove_status_type";
        public const string MultipleSelectStatusesForWorkflow = "multiple_select_statuses_for_workflow";

        public const string VisualEditorPlugins = "list_visual_editor_plugin";
        public const string AddNewVisualEditorPlugin = "new_visual_editor_plugin";
        public const string VisualEditorPluginProperties = "edit_visual_editor_plugin";
        public const string UpdateVisualEditorPlugin = "update_visual_editor_plugin";
        public const string RemoveVisualEditorPlugin = "remove_visual_editor_plugin";
        public const string SaveVisualEditorPlugin = "save_visual_editor_plugin";

        public const string QpPlugins = "list_plugin";
        public const string AddNewQpPlugin = "new_plugin";
        public const string QpPluginProperties = "edit_plugin";
        public const string UpdateQpPlugin = "update_plugin";
        public const string RemoveQpPlugin = "remove_plugin";
        public const string SaveQpPlugin = "save_plugin";

        public const string QpPluginVersions = "list_plugin_version";
        public const string CompareQpPluginVersionWithCurrent = "compare_plugin_version_with_current";
        public const string CompareQpPluginVersions = "compare_plugin_versions";
        public const string PreviewQpPluginVersion = "preview_plugin_version";

        public const string Notifications = "list_notification";
        public const string AddNewNotification = "new_notification";
        public const string NotificationProperties = "edit_notification";
        public const string UpdateNotification = "update_notification";
        public const string SaveNotification = "save_notification";
        public const string UnbindNotification = "unbind_notification";
        public const string RemoveNotification = "remove_notification";
        public const string MultipleRemoveNotification = "multiple_remove_notification";
        public const string AssembleNotification = "assemble_notification";
        public const string MultipleAssembleNotification = "multiple_assemble_notification";

        public const string AssemblePageTemplate = "assemble_template";
        public const string AssemblePage = "assemble_page";
        public const string MultipleAssemblePage = "multiple_assemble_page";
        public const string AssemblePageObject = "assemble_page_object";
        public const string MultipleAssemblePageObject = "multiple_assemble_page_object";
        public const string AssembleTemplateObject = "assemble_template_object";
        public const string MultipleAssembleTemplateObject = "multiple_assemble_template_object";

        public const string AssembleTemplate = "assemble_template";
        public const string AssembleTemplateFromTemplateObject = "assemble_template_from_template_object";
        public const string AssembleTemplateFromTemplateObjectFormat = "assemble_template_from_template_object_format";
        public const string AssembleTemplateFromTemplateObjectList = "assemble_template_from_template_object_list";
        public const string AssemblePageFromPageObject = "assemble_page_from_page_object";
        public const string AssemblePageFromPageObjectFormat = "assemble_page_from_page_object_format";
        public const string AssemblePageFromPageObjectList = "assemble_page_from_page_object_list";
        public const string AssembleObjectFromPageObjectFormat = "assemble_object_from_page_object_format";
        public const string AssembleObjectFromTemplateObjectFormat = "assemble_object_from_template_object_format";

        public const string RestorePageObjectFormatVersion = "restore_page_object_format_version";
        public const string RestoreTemplateObjectFormatVersion = "restore_template_object_format_version";
        public const string RefreshComparePageObjectFormatVersions = "refresh_compare_page_object_format_versions";

        public const string TemplateObjects = "list_template_object";
        public const string RefreshTemplateObjects = "refresh_template_objects";
        public const string AddNewTemplateObject = "new_template_object";
        public const string TemplateObjectProperties = "edit_template_object";
        public const string UpdateTemplateObject = "update_template_object";
        public const string CancelTemplateObject = "cancel_template_object";
        public const string RemoveTemplateObject = "remove_template_object";
        public const string SaveTemplateObject = "save_template_object";
        public const string MultipleRemovePage = "multiple_remove_page";
        public const string MultipleRemovePageObject = "multiple_remove_page_object";
        public const string MultipleRemoveTemplateObject = "multiple_remove_template_object";

        public const string PageObjects = "list_page_object";
        public const string RefreshPageObjects = "refresh_page_objects";
        public const string AddNewPageObject = "new_page_object";
        public const string SavePageObject = "save_page_object";
        public const string PageObjectProperties = "edit_page_object";
        public const string UpdatePageObject = "update_page_object";
        public const string CancelPageObject = "cancel_page_object";
        public const string RemovePageObject = "remove_page_object";

        public const string TemplateObjectFormats = "list_template_object_format";
        public const string TemplateObjectFormatVersions = "list_template_object_format_version";
        public const string CancelTemplateObjectFormat = "cancel_template_object_format";
        public const string CancelPageObjectFormat = "cancel_page_object_format";
        public const string SavePageObjectFormat = "save_page_object_format";

        public const string PageObjectFormats = "list_page_object_format";
        public const string PageObjectFormatVersions = "list_page_object_format_version";
        public const string AddNewPageObjectFormatVersion = "new_page_obect_format_version";
        public const string AddNewTemplateObjectFormatVersion = "new_template_obect_format_version";
        public const string AddNewTemplateObjectFormat = "new_template_object_format";
        public const string PromotePageObject = "promote_page_object";
        public const string AddNewPageObjectFormat = "new_page_object_format";
        public const string RemovePageObjectFormat = "remove_page_object_format";
        public const string RemoveTemplateObjectFormat = "remove_template_object_format";
        public const string PageObjectFormatProperties = "edit_page_object_format";
        public const string TemplateObjectFormatProperties = "edit_template_object_format";
        public const string UpdatePageObjectFormat = "update_page_object_format";
        public const string SaveTemplateObjectFormat = "save_template_object_format";
        public const string UpdateTemplateObjectFormat = "update_template_object_format";

        public const string Templates = "list_template";
        public const string AddNewPageTemplate = "new_template";
        public const string RefreshPageTemplates = "refresh_templates";
        public const string RefreshPageTemplate = "refresh_template";
        public const string PageTemplateProperties = "edit_template";
        public const string SavePageTemplate = "save_template";
        public const string UpdatePageTemplate = "update_template";
        public const string RemovePageTemplate = "remove_template";
        public const string CancelTemplate = "cancel_template";
        public const string CaptureLockTemplate = "capture_lock_template";
        public const string CaptureLockPage = "capture_lock_page";
        public const string CaptureLockPageObject = "capture_lock_page_object";
        public const string CaptureLockTemplateObject = "capture_lock_template_object";
        public const string CaptureLockPageObjectFormat = "capture_lock_page_object_format";
        public const string CaptureLockTemplateObjectFormat = "capture_lock_template_object_format";

        public const string TemplateObjectFormatVersionProperties = "edit_template_object_format_version";
        public const string PageObjectFormatVersionProperties = "edit_page_object_format_version";

        public const string CompareWithCurrentTemplateObjectFormatVersion = "compare_with_cur_template_object_format_version";
        public const string CompareWithCurrentPageObjectFormatVersion = "compare_with_cur_page_object_format_version";
        public const string ComparePageObjectFormatVersions = "compare_page_object_format_versions";
        public const string CompareTemplateObjectFormatVersions = "compare_template_object_format_versions";
        public const string MultipleRemoveTemplateObjectFormatVersion = "multiple_remove_template_object_format_version";
        public const string MultipleRemovePageObjectFormatVersion = "multiple_remove_page_object_format_version";

        public const string PageProperties = "edit_page";
        public const string CreateLikePage = "copy_page";
        public const string UpdatePage = "update_page";
        public const string RemovePage = "remove_page";
        public const string Pages = "list_page";
        public const string RefreshPages = "refresh_pages";
        public const string AddNewPage = "new_page";
        public const string SavePage = "save_page";
        public const string CancelPage = "cancel_page";
        public const string RefreshPage = "refresh_page";
        public const string SelectPageForObjectForm = "select_page_for_object_form";

        public const string ActionPermissionTree = "action_permission_tree";

        public const string NotificationObjectFormatProperties = "edit_notification_template_format";

        public const string EntityTypePermissions = "list_entity_type_permission";
        public const string AddNewEntityTypePermission = "new_entity_type_permission";
        public const string SaveEntityTypePermission = "save_entity_type_permission";
        public const string EntityTypePermissionProperties = "edit_entity_type_permission";
        public const string UpdateEntityTypePermission = "update_entity_type_permission";
        public const string RemoveEntityTypePermission = "remove_entity_type_permission";
        public const string MultipleRemoveEntityTypePermission = "multiple_remove_entity_type_permission";
        public const string ChangeEntityTypePermission = "change_entity_type_permission";
        public const string UpdateEntityTypePermissionChanges = "update_entity_type_permission_changes";
        public const string RemoveEntityTypePermissionChanges = "remove_entity_type_permission_changes";

        public const string ActionPermissions = "list_action_permission";
        public const string AddNewActionPermission = "new_action_permission";
        public const string SaveActionPermission = "save_action_permission";
        public const string ActionPermissionProperties = "edit_action_permission";
        public const string UpdateActionPermission = "update_action_permission";
        public const string RemoveActionPermission = "remove_action_permission";
        public const string MultipleRemoveActionPermission = "multiple_remove_action_permission";
        public const string ChangeActionPermission = "change_action_permission";
        public const string UpdateActionPermissionChanges = "update_action_permission_changes";
        public const string RemoveActionPermissionChanges = "remove_action_permission_changes";

        public const string ListExternalWorkflowUserTasks = "list_article_external_workflow_tasks";
        public const string GetExternalWorkflowUserTask = "get_article_external_workflow_task";
        public const string CompleteExternalWorkflowUserTask = "complete_article_external_workflow_task";

        public static IEnumerable<string> ArticleNonChangingActionCodes { get; } = new[]
        {
            Articles,
            RefreshArticles,
            EditArticle,
            RefreshArticle,
            ArticleVersions,
            CancelArticle
        };

        public static IEnumerable<string> ArticleVersionsNonChangingActionCodes { get; } = new[]
        {
            ArticleVersions,
            RefreshArticleVersions,
            PreviewArticleVersion,
            RefreshArticleVersion,
            CompareArticleVersionWithCurrent,
            CompareArticleVersions,
            CancelArticleVersion
        };


    }
}
