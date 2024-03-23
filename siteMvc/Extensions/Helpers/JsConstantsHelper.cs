using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Configuration;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Utils;
using Quantumart.QP8.WebMvc.ViewModels.Audit;

namespace Quantumart.QP8.WebMvc.Extensions.Helpers
{
    public class JsConstantsHelper
    {
        private IUrlHelper _urlHelper;
        public JsConstantsHelper(IUrlHelper urlHelper)
        {
            _urlHelper = urlHelper;
        }

        public string GetResult()
        {
            var currentTheme = QPConfiguration.Options.DefaultTheme;
            var constants = new StringBuilder();

            constants.AppendLine("Type.registerNamespace(\"Quantumart.QP8.Enums\");");

            //Версия приложения
            constants.AppendLine("// Версия приложения");
            constants.AppendLine($"window.BACKEND_VERSION = \"{new ApplicationInfoRepository().GetCurrentDbVersion()}\"");

            // Константы кодов типов узлов
            constants.AppendLine("// Константы кодов типов узлов");
            constants.AppendLine($"window.ENTITY_TYPE_CODE_NONE = \"{EntityTypeCode.None}\"");
            constants.AppendLine($"window.ENTITY_TYPE_CODE_CUSTOMER_CODE = \"{EntityTypeCode.CustomerCode}\"");
            constants.AppendLine($"window.ENTITY_TYPE_CODE_SITE = \"{EntityTypeCode.Site}\"");
            constants.AppendLine($"window.ENTITY_TYPE_CODE_CONTENT_GROUP = \"{EntityTypeCode.ContentGroup}\"");
            constants.AppendLine($"window.ENTITY_TYPE_CODE_CONTENT = \"{EntityTypeCode.Content}\"");
            constants.AppendLine($"window.ENTITY_TYPE_CODE_ARTICLE = \"{EntityTypeCode.Article}\"");
            constants.AppendLine($"window.ENTITY_TYPE_CODE_ARCHIVE_ARTICLE = \"{EntityTypeCode.ArchiveArticle}\"");
            constants.AppendLine($"window.ENTITY_TYPE_CODE_ARTICLE_VERSION = \"{EntityTypeCode.ArticleVersion}\"");
            constants.AppendLine($"window.ENTITY_TYPE_CODE_FIELD = \"{EntityTypeCode.Field}\"");
            constants.AppendLine($"window.ENTITY_TYPE_CODE_VIRTUAL_CONTENT = \"{EntityTypeCode.VirtualContent}\"");
            constants.AppendLine($"window.ENTITY_TYPE_CODE_VIRTUAL_ARTICLE = \"{EntityTypeCode.VirtualArticle}\"");
            constants.AppendLine($"window.ENTITY_TYPE_CODE_VIRTUAL_FIELD = \"{EntityTypeCode.VirtualField}\"");
            constants.AppendLine($"window.ENTITY_TYPE_CODE_SITE_FOLDER = \"{EntityTypeCode.SiteFolder}\"");
            constants.AppendLine($"window.ENTITY_TYPE_CODE_CONTENT_FOLDER = \"{EntityTypeCode.ContentFolder}\"");
            constants.AppendLine($"window.ENTITY_TYPE_CODE_SITE_FILE = \"{EntityTypeCode.SiteFile}\"");
            constants.AppendLine($"window.ENTITY_TYPE_CODE_CONTENT_FILE = \"{EntityTypeCode.ContentFile}\"");
            constants.AppendLine($"window.ENTITY_TYPE_CODE_NOTIFICATION = \"{EntityTypeCode.Notification}\"");
            constants.AppendLine($"window.ENTITY_TYPE_CODE_WORKFLOW = \"{EntityTypeCode.Workflow}\"");
            constants.AppendLine($"window.ENTITY_TYPE_CODE_STATUS = \"{EntityTypeCode.StatusType}\"");
            constants.AppendLine($"window.ENTITY_TYPE_CODE_STYLE = \"{EntityTypeCode.Style}\"");
            constants.AppendLine($"window.ENTITY_TYPE_CODE_SNIPPET = \"{EntityTypeCode.Snippet}\"");
            constants.AppendLine($"window.ENTITY_TYPE_CODE_USER = \"{EntityTypeCode.User}\"");
            constants.AppendLine($"window.ENTITY_TYPE_CODE_USER_GROUP = \"{EntityTypeCode.UserGroup}\"");
            constants.AppendLine($"window.ENTITY_TYPE_CODE_CUSTOM_ACTION = \"{EntityTypeCode.CustomAction}\"");
            constants.AppendLine($"window.ENTITY_TYPE_CODE_USER = \"{EntityTypeCode.User}\"");
            constants.AppendLine($"window.ENTITY_TYPE_CODE_CONTENT_PERMISSION = \"{EntityTypeCode.ContentPermission}\"");
            constants.AppendLine($"window.ENTITY_TYPE_CODE_ARTICLE_PERMISSION = \"{EntityTypeCode.ArticlePermission}\"");
            constants.AppendLine($"window.ENTITY_TYPE_CODE_SITE_PERMISSION = \"{EntityTypeCode.SitePermission}\"");
            constants.AppendLine($"window.ENTITY_TYPE_CODE_WORKFLOW_PERMISSION = \"{EntityTypeCode.WorkflowPermission}\"");
            constants.AppendLine($"window.ENTITY_TYPE_CODE_SITE_FOLDER_TYPE_PERMISSION = \"{EntityTypeCode.SiteFolderPermission}\"");
            constants.AppendLine($"window.ENTITY_TYPE_CODE_ENTITY_TYPE_PERMISSION = \"{EntityTypeCode.EntityTypePermission}\"");
            constants.AppendLine($"window.ENTITY_TYPE_CODE_ACTION_PERMISSION = \"{EntityTypeCode.ActionPermission}\"");
            constants.AppendLine($"window.ENTITY_TYPE_CODE_ARTICLE_EXTERNAL_WORKFLOW = \"{EntityTypeCode.ArticleExternalWorkflow}\"");
            constants.AppendLine();

            // Константы типов действий
            constants.AppendLine("// Константы типов действий");
            constants.AppendLine($"window.ACTION_TYPE_CODE_NONE = \"{ActionTypeCode.None}\"");
            constants.AppendLine($"window.ACTION_TYPE_CODE_REFRESH = \"{ActionTypeCode.Refresh}\"");
            constants.AppendLine($"window.ACTION_TYPE_CODE_ADD_NEW = \"{ActionTypeCode.AddNew}\"");
            constants.AppendLine($"window.ACTION_TYPE_CODE_LIST = \"{ActionTypeCode.List}\"");
            constants.AppendLine($"window.ACTION_TYPE_CODE_SELECT = \"{ActionTypeCode.Select}\"");
            constants.AppendLine($"window.ACTION_TYPE_CODE_MULTIPLE_SELECT = \"{ActionTypeCode.MultipleSelect}\"");
            constants.AppendLine($"window.ACTION_TYPE_CODE_READ = \"{ActionTypeCode.Read}\"");
            constants.AppendLine($"window.ACTION_TYPE_CODE_LIBRARY = \"{ActionTypeCode.Library}\"");
            constants.AppendLine($"window.ACTION_TYPE_CODE_PREVIEW = \"{ActionTypeCode.Preview}\"");
            constants.AppendLine($"window.ACTION_TYPE_CODE_CROP = \"{ActionTypeCode.Crop}\"");
            constants.AppendLine($"window.ACTION_TYPE_CODE_AUTO_RESIZE = \"{ActionTypeCode.AutoResize}\"");
            constants.AppendLine($"window.ACTION_TYPE_CODE_ALL_FILES_UPLOADED = \"{ActionTypeCode.AllFilesUploaded}\"");
            constants.AppendLine($"window.ACTION_TYPE_CODE_FILE_CROPPED = \"{ActionTypeCode.FileCropped}\"");
            constants.AppendLine($"window.ACTION_TYPE_CODE_FILE_RESIZED = \"{ActionTypeCode.FileResized}\"");
            constants.AppendLine($"window.ACTION_TYPE_CODE_DOWNLOAD = \"{ActionTypeCode.Download}\"");
            constants.AppendLine($"window.ACTION_TYPE_CODE_REMOVE = \"{ActionTypeCode.Remove}\"");
            constants.AppendLine($"window.ACTION_TYPE_CODE_MULTIPLE_REMOVE = \"{ActionTypeCode.MultipleRemove}\"");
            constants.AppendLine($"window.ACTION_TYPE_CODE_SAVE = \"{ActionTypeCode.Save}\"");
            constants.AppendLine($"window.ACTION_TYPE_CODE_UPDATE = \"{ActionTypeCode.Update}\"");
            constants.AppendLine($"window.ACTION_TYPE_CODE_SAVE_AND_UP = \"{ActionTypeCode.SaveAndUp}\"");
            constants.AppendLine($"window.ACTION_TYPE_CODE_UPDATE_AND_UP = \"{ActionTypeCode.UpdateAndUp}\"");
            constants.AppendLine($"window.ACTION_TYPE_CODE_CANCEL = \"{ActionTypeCode.Cancel}\"");
            constants.AppendLine($"window.ACTION_TYPE_CODE_COPY = \"{ActionTypeCode.Copy}\"");
            constants.AppendLine($"window.ACTION_TYPE_CODE_ARCHIVE = \"{ActionTypeCode.Archive}\"");
            constants.AppendLine($"window.ACTION_TYPE_CODE_MULTIPLE_ARCHIVE = \"{ActionTypeCode.MultipleArchive}\"");
            constants.AppendLine($"window.ACTION_TYPE_CODE_RESTORE = \"{ActionTypeCode.Restore}\"");
            constants.AppendLine($"window.ACTION_TYPE_CODE_MULTIPLE_RESTORE = \"{ActionTypeCode.MultipleRestore}\"");
            constants.AppendLine($"window.ACTION_TYPE_CODE_CHANGE_LOCK = \"{ActionTypeCode.ChangeLock}\"");
            constants.AppendLine($"window.ACTION_TYPE_CODE_MULTIPLE_UNLOCK = \"{ActionTypeCode.MultipleUnlock}\"");
            constants.AppendLine($"window.ACTION_TYPE_CODE_SEARCH = \"{ActionTypeCode.Search}\"");
            constants.AppendLine($"window.ACTION_TYPE_CODE_DESELECT_ALL = \"{ActionTypeCode.DeselectAll}\"");
            constants.AppendLine($"window.ACTION_TYPE_CODE_COMPLETE = \"{ActionTypeCode.Complete}\"");
            constants.AppendLine($"window.ACTION_TYPE_SIMPLE_UPDATE = \"{ActionTypeCode.SimpleUpdate}\"");
            constants.AppendLine($"window.ACTION_TYPE_CHILD_ENTITY_PERMISSION_SAVE = \"{ActionTypeCode.ChildEntityPermissionSave}\"");
            constants.AppendLine($"window.ACTION_TYPE_CHILD_ENTITY_MULTIPLE_REMOVE = \"{ActionTypeCode.MultipleChildEntityPermissionRemove}\"");
            constants.AppendLine($"window.ACTION_TYPE_CHILD_ENTITY_REMOVE_ALL = \"{ActionTypeCode.ChildEntityPermissionRemoveAll}\"");
            constants.AppendLine($"window.ACTION_TYPE_CHILD_ENTITY_REMOVE = \"{ActionTypeCode.ChildEntityPermissionRemove}\"");
            constants.AppendLine($"window.ACTION_TYPE_ACTION_PERMISSION_TREE = \"{ActionTypeCode.ActionPermissionTree}\"");
            constants.AppendLine($"window.ACTION_TYPE_SELECT_CHILD_ARTICLES = \"{ActionTypeCode.SelectChildArticles}\"");
            constants.AppendLine($"window.ACTION_TYPE_UNSELECT_CHILD_ARTICLES = \"{ActionTypeCode.UnselectChildArticles}\"");

            constants.AppendLine();
            constants.AppendLine($"window.CHECK_DB_MODE_INTERVAL = {30000}");
            constants.AppendLine($"window.MAX_ITEMS_AFFECTED_NUMBER = {255}");
            constants.AppendLine($"window.CHANGED_FIELD_CLASS_NAME = \"{CssClasses.ChangedField}\"");
            constants.AppendLine($"window.REFRESHED_FIELD_CLASS_NAME = \"{CssClasses.RefreshedField}\"");
            constants.AppendLine();

            // Константы действий
            constants.AppendLine("// Константы действий");
            constants.AppendLine($"window.ACTION_CODE_UPDATE_SITE_FILE = \"{ActionCode.UpdateSiteFile}\"");
            constants.AppendLine($"window.ACTION_CODE_UPDATE_CONTENT_FILE = \"{ActionCode.UpdateContentFile}\"");
            constants.AppendLine($"window.ACTION_CODE_NONE = \"{ActionCode.None}\"");
            constants.AppendLine($"window.ACTION_CODE_EDIT_PROFILE = \"{ActionCode.EditProfile}\"");
            constants.AppendLine($"window.ACTION_CODE_ADD_NEW_SITE = \"{ActionCode.AddNewSite}\"");
            constants.AppendLine($"window.ACTION_CODE_ARTICLES = \"{ActionCode.Articles}\"");
            constants.AppendLine($"window.ACTION_CODE_SELECT_ARTICLE = \"{ActionCode.SelectArticle}\"");
            constants.AppendLine($"window.ACTION_CODE_MULTIPLE_SELECT_ARTICLE = \"{ActionCode.MultipleSelectArticle}\"");
            constants.AppendLine($"window.ACTION_CODE_MULTIPLE_PUBLISH_ARTICLES = \"{ActionCode.MultiplePublishArticles}\"");
            constants.AppendLine($"window.ACTION_CODE_ADD_NEW_ARTICLE = \"{ActionCode.AddNewArticle}\"");
            constants.AppendLine($"window.ACTION_CODE_PREVIEW_ARTICLE_VERSION = \"{ActionCode.PreviewArticleVersion}\"");
            constants.AppendLine($"window.ACTION_CODE_COMPARE_ARTICLE_VERSION_WITH_CURRENT = \"{ActionCode.CompareArticleVersionWithCurrent}\"");
            constants.AppendLine($"window.ACTION_CODE_SITE_LIBRARY = \"{ActionCode.SiteLibrary}\"");
            constants.AppendLine($"window.ACTION_CODE_CONTENT_LIBRARY = \"{ActionCode.ContentLibrary}\"");
            constants.AppendLine($"window.ACTION_CODE_POPUP_SITE_LIBRARY = \"{ActionCode.PopupSiteLibrary}\"");
            constants.AppendLine($"window.ACTION_CODE_POPUP_CONTENT_LIBRARY = \"{ActionCode.PopupContentLibrary}\"");
            constants.AppendLine($"window.ACTION_CODE_ENABLE_ARTICLES_PERMISSIONS = \"{ActionCode.EnableArticlesPermissions}\"");
            constants.AppendLine($"window.ACTION_CODE_CONTENTS = \"{ActionCode.Contents}\"");

            constants.AppendLine($"window.ACTION_CODE_CONTENT_PERMISSIONS = \"{ActionCode.ContentPermissions}\"");
            constants.AppendLine($"window.ACTION_CODE_CHILD_CONTENT_PERMISSIONS = \"{ActionCode.ChildContentPermissions}\"");
            constants.AppendLine($"window.ACTION_CODE_CONTENT_PERMISSIONS_FOR_CHILD = \"{ActionCode.ContentPermissionsForChild}\"");

            constants.AppendLine("window.ASSEMBLE_PARENT_ACTION_TYPE = \"assemble_parent\"");

            constants.AppendLine($"window.ACTION_CODE_ARTICLE_PERMISSIONS = \"{ActionCode.ArticlePermissions}\"");
            constants.AppendLine($"window.ACTION_CODE_CHILD_ARTICLE_PERMISSIONS = \"{ActionCode.ChildArticlePermissions}\"");
            constants.AppendLine($"window.ACTION_CODE_ARTICLE_PERMISSIONS_FOR_CHILD = \"{ActionCode.ArticlePermissionsForChild}\"");

            constants.AppendLine($"window.ACTION_CODE_NEW_ENTITY_TYPE_PERMISSION = \"{ActionCode.AddNewEntityTypePermission}\"");
            constants.AppendLine($"window.ACTION_CODE_EDIT_ENTITY_TYPE_PERMISSION = \"{ActionCode.EntityTypePermissionProperties}\"");
            constants.AppendLine($"window.ACTION_CODE_CHANGE_ENTITY_TYPE_PERMISSION_NODE = \"{ActionCode.ChangeEntityTypePermission}\"");
            constants.AppendLine($"window.ACTION_CODE_UPDATE_ENTITY_TYPE_PERMISSION_NODE = \"{ActionCode.UpdateEntityTypePermissionChanges}\"");
            constants.AppendLine($"window.ACTION_CODE_REMOVE_ENTITY_TYPE_PERMISSION_NODE = \"{ActionCode.RemoveEntityTypePermissionChanges}\"");

            constants.AppendLine($"window.ACTION_CODE_NEW_ACTION_PERMISSION = \"{ActionCode.AddNewActionPermission}\"");
            constants.AppendLine($"window.ACTION_CODE_EDIT_ACTION_PERMISSION = \"{ActionCode.ActionPermissionProperties}\"");
            constants.AppendLine($"window.ACTION_CODE_CHANGE_ACTION_PERMISSION_NODE = \"{ActionCode.ChangeActionPermission}\"");
            constants.AppendLine($"window.ACTION_CODE_UPDATE_ACTION_PERMISSION_NODE = \"{ActionCode.UpdateActionPermissionChanges}\"");
            constants.AppendLine($"window.ACTION_CODE_REMOVE_ACTION_PERMISSION_NODE = \"{ActionCode.RemoveActionPermissionChanges}\"");

            constants.AppendLine($"window.ACTION_CODE_CHANGE_CHILD_ARTICLE_PERMISSION = \"{ActionCode.ChangeChildArticlePermission}\"");
            constants.AppendLine($"window.ACTION_CODE_CHANGE_CHILD_CONTENT_PERMISSION = \"{ActionCode.ChangeChildContentPermission}\"");
            constants.AppendLine($"window.ACTION_CODE_REMOVE_CHILD_ARTICLE_PERMISSION = \"{ActionCode.RemoveChildContentPermission}\"");
            constants.AppendLine($"window.ACTION_CODE_REMOVE_CHILD_CONTENT_PERMISSION = \"{ActionCode.RemoveChildArticlePermission}\"");

            constants.AppendLine($"window.ACTION_CODE_SELECT_USER_GROUP = \"{ActionCode.SelectUserGroup}\"");
            constants.AppendLine($"window.ACTION_CODE_SELECT_USER = \"{ActionCode.SelectUser}\"");
            constants.AppendLine($"window.ACTION_CODE_MULTIPLE_SELECT_USER = \"{ActionCode.MultipleSelectUser}\"");

            constants.AppendLine($"window.ACTION_CODE_ADD_NEW_CHILD_ARTICLE = \"{ActionCode.AddNewChildArticle}\"");
            constants.AppendLine($"window.ACTION_CODE_ADD_NEW_ADJACENT_FIELD = \"{ActionCode.AddNewAdjacentField}\"");

            constants.AppendLine($"window.ACTION_CODE_EXTERNAL_WORKFLOW_TASKS = \"{ActionCode.ListExternalWorkflowUserTasks}\"");
            constants.AppendLine($"window.ACTION_CODE_COMPLETE_EXTERNAL_WORKFLOW_TASK = \"{ActionCode.CompleteExternalWorkflowUserTask}\"");

            constants.AppendLine();

            // Коды типов представлений
            constants.AppendLine("// Коды типов представлений");
            constants.AppendLine($"window.VIEW_TYPE_CODE_NONE = \"{ViewTypeCode.None}\"");
            constants.AppendLine($"window.VIEW_TYPE_CODE_LIST = \"{ViewTypeCode.List}\"");
            constants.AppendLine($"window.VIEW_TYPE_CODE_TREE = \"{ViewTypeCode.Tree}\"");
            constants.AppendLine($"window.VIEW_TYPE_CODE_DETAILS = \"{ViewTypeCode.Details}\"");
            constants.AppendLine($"window.VIEW_TYPE_CODE_THUMBNAILS = \"{ViewTypeCode.Thumbnails}\"");
            constants.AppendLine();

            // Константы типов сообщений, возвращаемых Actions
            constants.AppendLine($"window.ACTION_MESSAGE_TYPE_INFO = \"{ActionMessageType.Info}\"");
            constants.AppendLine($"window.ACTION_MESSAGE_TYPE_WARNING = \"{ActionMessageType.Warning}\"");
            constants.AppendLine($"window.ACTION_MESSAGE_TYPE_ERROR = \"{ActionMessageType.Error}\"");
            constants.AppendLine($"window.ACTION_MESSAGE_TYPE_CONFIRM = \"{ActionMessageType.Confirm}\"");
            constants.AppendLine($"window.ACTION_MESSAGE_TYPE_DOWNLOAD = \"{ActionMessageType.Download}\"");
            constants.AppendLine();

            // Костанты URL`ов
            constants.AppendLine($"window.APPLICATION_ROOT_URL = \"{_urlHelper.Content( "~/")}\"");

            // Константы URL`ов контроллеров
            constants.AppendLine("// Константы URL`ов контроллеров");
            constants.AppendLine($"window.CONTROLLER_URL_TREE_MENU = \"{_urlHelper.Content("~/TreeMenu/")}\"");
            constants.AppendLine($"window.CONTROLLER_URL_CONTEXT_MENU = \"{_urlHelper.Content("~/ContextMenu/")}\"");
            constants.AppendLine($"window.CONTROLLER_URL_TOOLBAR = \"{_urlHelper.Content("~/Toolbar/")}\"");
            constants.AppendLine($"window.CONTROLLER_URL_ENTITY_TYPE = \"{_urlHelper.Content("~/EntityType/")}\"");
            constants.AppendLine($"window.CONTROLLER_URL_ENTITY_OBJECT = \"{_urlHelper.Content("~/EntityObject/")}\"");
            constants.AppendLine($"window.CONTROLLER_URL_BACKEND_ACTION = \"{_urlHelper.Content("~/BackendAction/")}\"");
            constants.AppendLine($"window.CONTROLLER_URL_BACKEND_ACTION_TYPE = \"{_urlHelper.Content( "~/BackendActionType/")}\"");
            constants.AppendLine($"window.CONTROLLER_URL_ARTICLE_SEARCH_BLOCK = \"{_urlHelper.Content( "~/ArticleSearchBlock/")}\"");
            constants.AppendLine($"window.CONTROLLER_URL_WORKFLOW = \"{_urlHelper.Content( "~/Workflow/")}\"");
            constants.AppendLine($"window.CONTROLLER_URL_CONTENT_SEARCH_BLOCK = \"{_urlHelper.Content( "~/Content/")}\"");
            constants.AppendLine($"window.CONTROLLER_URL_SITE = \"{_urlHelper.Content( "~/Site/")}\"");
            constants.AppendLine($"window.CONTROLLER_URL_CONTENT = \"{_urlHelper.Content( "~/Content/")}\"");
            constants.AppendLine($"window.CONTROLLER_URL_ARTICLE = \"{_urlHelper.Content( "~/Article/")}\"");
            constants.AppendLine($"window.CONTROLLER_URL_THUMBNAIL = \"{_urlHelper.Content( "~/Thumbnail/")}\"");
            constants.AppendLine($"window.CONTROLLER_URL_FIELD = \"{_urlHelper.Content("~/Field/")}\"");
            constants.AppendLine($"window.CONTROLLER_URL_VIRTUAL_CONTENT = \"{_urlHelper.Content("~/VirtualContent/")}\"");
            constants.AppendLine($"window.CONTROLLER_URL_USER = \"{_urlHelper.Content("~/User/")}\"");
            constants.AppendLine($"window.CONTROLLER_URL_ACTION_PERMISSION_TREE = \"{_urlHelper.Content("~/ActionPermissionTree/")}\"");
            constants.AppendLine($"window.CONTROLLER_URL_LOGON = \"{_urlHelper.Content("~/LogOn/")}\"");
            constants.AppendLine($"window.CONTROLLER_URL_WINLOGON = \"{_urlHelper.Content("~/WinLogOn/")}\"");
            constants.AppendLine($"window.CONTROLLER_URL_PAGE_TEMPLATE = \"{_urlHelper.Content("~/PageTemplate/")}\"");
            constants.AppendLine($"window.CONTROLLER_URL_CUSTOM_ACTION = \"{_urlHelper.Content("~/CustomAction/")}\"");
            constants.AppendLine($"window.CONTROLLER_URL_AUTH = \"{_urlHelper.Content("~/LogOn/")}\"");
            constants.AppendLine($"window.CONTROLLER_URL_DB = \"{_urlHelper.Content("~/Db/")}\"");
            constants.AppendLine();

            // Константы URL`ов директорий с рисунками
            constants.AppendLine("// Константы URL`ов директорий с рисунками");
            constants.AppendLine($"window.COMMON_IMAGE_FOLDER_URL_ROOT = \"{SitePathHelper.GetCommonRootImageFolderUrl()}\"");
            constants.AppendLine($"window.THEME_IMAGE_FOLDER_URL_ROOT = \"{SitePathHelper.GetThemeRootImageFolderUrl(currentTheme)}\"");
            constants.AppendLine($"window.THEME_IMAGE_FOLDER_URL_SMALL_ICONS = \"{SitePathHelper.GetThemeSmallIconsImageFolderUrl(currentTheme)}\"");
            constants.AppendLine($"window.THEME_IMAGE_FOLDER_URL_AJAX_LOADER_ICONS = \"{SitePathHelper.GetThemeAjaxLoaderIconsImageFolderUrl(currentTheme)}\"");
            constants.AppendLine($"window.THEME_IMAGE_FOLDER_URL_SMALL_FILE_TYPE_ICONS = \"{SitePathHelper.GetThemeSmallFileTypeIconFolderUrl(currentTheme)}\"");
            constants.AppendLine($"window.THEME_IMAGE_FOLDER_URL_BIG_FILE_TYPE_ICONS = \"{SitePathHelper.GetThemeBigFileTypeIconFolderUrl(currentTheme)}\"");

            // Коды контекстного меню
            constants.AppendLine("// Коды контекстного меню");
            constants.AppendLine($"window.CONTEXT_MENU_CODE_ENTITY_TYPE_PERMISSION_NODE = \"{ContextMenuCodes.ActionPermissionEntityTypeNode}\"");
            constants.AppendLine($"window.CONTEXT_MENU_CODE_ACTION_PERMISSION_NODE = \"{ContextMenuCodes.ActionPermissionActionNode}\"");

            // Константы серверного окружения
            constants.AppendLine("// Константы серверного окружения");
            constants.AppendLine($"window.MAX_UPLOAD_SIZE_BYTES = {QPConfiguration.Options.UploadMaxSize * 1024 * 1024}");
            constants.AppendLine($"window.DIRECTORY_SEPARATOR_CHAR = \"\\{System.IO.Path.DirectorySeparatorChar}\"");


            // Типы вхождения в диапазон
            constants.AppendLine("// Типы вхождения в диапазон");
            constants.AppendLine(GenerateEnumeration("Quantumart.QP8.Enums.RangeBoundaryType", new Dictionary<string, int>
            {
                { "Ignore", 0 },
                { "Inclusive", 1 },
                { "Exclusive", 2 }
            }));

            // Типы вхождения символов
            constants.AppendLine("// Типы вхождения символов");
            constants.AppendLine(GenerateEnumeration("Quantumart.QP8.Enums.ContainsCharacters", new Dictionary<string, int>
            {
                { "Any", 0 },
                { "All", 1 }
            }));

            // Режимы выделения списка
            constants.AppendLine("// Режимы выделения списка");
            constants.AppendLine(GenerateEnumeration("Quantumart.QP8.Enums.ListSelectionMode", new Dictionary<string, int>
            {
                { "AllItems", (int)ListSelectionMode.AllItems },
                { "OnlySelectedItems", (int)ListSelectionMode.OnlySelectedItems }
            }));

            // Режимы работы плагина AnyTime
            constants.AppendLine($"window.DATE_TIME_PICKER_MODE_DATE = {DateTimePickerMode.Date}");
            constants.AppendLine($"window.DATE_TIME_PICKER_MODE_TIME = {DateTimePickerMode.Time}");
            constants.AppendLine($"window.DATE_TIME_PICKER_MODE_DATE_TIME = {DateTimePickerMode.DateTime}");
            constants.AppendLine();

            // Локализация Silverlight Upload
            constants.AppendLine($"window.UPLOAD_BROWSE_BUTTON_NAME = \"{LibraryStrings.Upload}\"");
            constants.AppendLine($"window.UPLOAD_TOTAL_LABEL = \"{LibraryStrings.Total}\"");
            constants.AppendLine($"window.UPLOAD_MAX_SIZE_MESSAGE = \"{LibraryStrings.MaxSizeExceeded}\"");
            constants.AppendLine($"window.UPLOAD_OVERWRITE_MESSAGE = \"{LibraryStrings.AlreadyExists}\"");
            constants.AppendLine($"window.UPLOAD_EXTENSION_MESSAGE = \"{LibraryStrings.NotAllowedExtensionForUpload}\"");
            constants.AppendLine($"window.UPLOAD_SECURITY_MESSAGE = \"{LibraryStrings.UploadIsNotAllowed}\"");
            constants.AppendLine($"window.HTML_UPLOAD_MAX_SIZE_MESSAGE = \"{LibraryStrings.HtmlUploaderMaxSizeExceeded}\"");
            constants.AppendLine($"window.HTML_UPLOAD_ERROR_MESSAGE = \"{LibraryStrings.HtmlUploaderErrorMesage}\"");
            constants.AppendLine($"window.AUTO_RESIZE_CONFIRM_MESSAGE = \"{LibraryStrings.AutoResizeConfirmMessage}\"");

            // Константы PlUpload
            constants.AppendLine($"window.PL_IMAGE_RESOLUTION = \"{Plupload.ImageResolution}\"");

            // Локализация PlUpload
            constants.AppendLine($"window.PL_UPLOAD_ERROR_REPORT = \"{LibraryStrings.PlUploadErrorReport}\"");
            constants.AppendLine($"window.PL_UPLOAD_ZERO_SIZE_WARN = \"{LibraryStrings.ZeroSizeWarning}\"");

            // Типы полей
            constants.AppendLine($"window.STRING_FIELD_TYPE = \"{FieldExactTypes.String}\"");
            constants.AppendLine($"window.NUMERIC_FIELD_TYPE = \"{FieldExactTypes.Numeric}\"");
            constants.AppendLine($"window.BOOLEAN_FIELD_TYPE = \"{FieldExactTypes.Boolean}\"");
            constants.AppendLine($"window.DATE_FIELD_TYPE = \"{FieldExactTypes.Date}\"");
            constants.AppendLine($"window.TIME_FIELD_TYPE = \"{FieldExactTypes.Time}\"");
            constants.AppendLine($"window.DATETIME_FIELD_TYPE = \"{FieldExactTypes.DateTime}\"");
            constants.AppendLine($"window.FILE_FIELD_TYPE = \"{FieldExactTypes.File}\"");
            constants.AppendLine($"window.IMAGE_FIELD_TYPE = \"{FieldExactTypes.Image}\"");
            constants.AppendLine($"window.DYNAMIC_IMAGE_FIELD_TYPE = \"{FieldExactTypes.DynamicImage}\"");
            constants.AppendLine($"window.TEXTBOX_FIELD_TYPE = \"{FieldExactTypes.Textbox}\"");
            constants.AppendLine($"window.VISUAL_EDIT_FIELD_TYPE = \"{FieldExactTypes.VisualEdit}\"");
            constants.AppendLine($"window.O2M_RELATION_FIELD_TYPE = \"{FieldExactTypes.O2MRelation}\"");
            constants.AppendLine($"window.M2M_RELATION_FIELD_TYPE = \"{FieldExactTypes.M2MRelation}\"");
            constants.AppendLine($"window.M2O_RELATION_FIELD_TYPE = \"{FieldExactTypes.M2ORelation}\"");
            constants.AppendLine($"window.STRING_ENUM_FIELD_TYPE = \"{FieldExactTypes.StringEnum}\"");
            constants.AppendLine($"window.CLASSIFIER_FIELD_TYPE = \"{FieldExactTypes.Classifier}\"");
            constants.AppendLine();

            //Локализация HTA
            constants.AppendLine($"window.HTA_DEFAULT_CONFIRM = \"{TemplateStrings.ReplaceConfirmation}\"");
            constants.AppendLine($"window.HTA_INSERT_CALL = \"{TemplateStrings.InsertCall}\"");

            // Операторы сравнения
            constants.AppendLine("// Операторы сравнения");
            constants.AppendLine(GenerateEnumeration("Quantumart.QP8.Enums.ComparisonOperator", new Dictionary<string, int>
            {
                { "Equal", 0 },
                { "NotEqual", 1 },
                { "GreaterThan", 2 },
                { "GreaterThanEqual", 3 },
                { "LessThan", 4 },
                { "LessThanEqual", 5 }
            }));

            // Типы поиска по полям статей
            constants.AppendLine("// Типы поиска по полям статей");
            constants.AppendLine(GenerateEnumeration("Quantumart.QP8.Enums.ArticleFieldSearchType", new Dictionary<string, int>
            {
                { "Identifier", (int)ArticleFieldSearchType.Identifier },
                { "FullText", (int)ArticleFieldSearchType.FullText },
                { "Boolean", (int)ArticleFieldSearchType.Boolean },
                { "DateRange", (int)ArticleFieldSearchType.DateRange },
                { "M2MRelation", (int)ArticleFieldSearchType.M2MRelation },
                { "NumericRange", (int)ArticleFieldSearchType.NumericRange },
                { "O2MRelation", (int)ArticleFieldSearchType.O2MRelation },
                { "M2ORelation", (int)ArticleFieldSearchType.M2ORelation },
                { "Text", (int)ArticleFieldSearchType.Text },
                { "TimeRange", (int)ArticleFieldSearchType.TimeRange },
                { "Classifier", (int)ArticleFieldSearchType.Classifier },
                { "DateTimeRange", (int)ArticleFieldSearchType.DateTimeRange },
                { "StringEnum", (int)ArticleFieldSearchType.StringEnum }
            }));

            // Типы файлов
            constants.AppendLine("// Типы файлов");
            constants.AppendLine(GenerateEnumeration("Quantumart.QP8.Enums.LibraryFileType", new Dictionary<string, int>
            {
                { "CSS", (int)FolderFileType.CSS },
                { "Flash", (int)FolderFileType.Flash },
                { "Image", (int)FolderFileType.Image },
                { "Javascript", (int)FolderFileType.Javascript },
                { "Media", (int)FolderFileType.Media },
                { "Office", (int)FolderFileType.Office },
                { "PDF", (int)FolderFileType.PDF },
                { "Unknown", (int)FolderFileType.Unknown }
            }));

            // Расширения по типам файлов
            constants.AppendLine("// Расширения по типам файлов");
            constants.AppendLine(GenerateDictionary("LIBRARY_FILE_EXTENSIONS_DICTIONARY", new Dictionary<string, string>
            {
                { FolderFileType.CSS.ToString("D"), FolderFile.GetTypeExtensions(FolderFileType.CSS) },
                { FolderFileType.Flash.ToString("D"), FolderFile.GetTypeExtensions(FolderFileType.Flash) },
                { FolderFileType.Image.ToString("D"), FolderFile.GetTypeExtensions(FolderFileType.Image) },
                { FolderFileType.Javascript.ToString("D"), FolderFile.GetTypeExtensions(FolderFileType.Javascript) },
                { FolderFileType.Media.ToString("D"), FolderFile.GetTypeExtensions(FolderFileType.Media) },
                { FolderFileType.Office.ToString("D"), FolderFile.GetTypeExtensions(FolderFileType.Office) },
                { FolderFileType.PDF.ToString("D"), FolderFile.GetTypeExtensions(FolderFileType.PDF) },
                { FolderFileType.Unknown.ToString("D"), FolderFile.GetTypeExtensions(FolderFileType.Unknown) }
            }));

            // Операторы сравнения
            constants.AppendLine("// Типы целей для отображения интерфейсного действия");
            constants.AppendLine(GenerateEnumeration("Quantumart.QP8.Enums.ActionTargetType", new Dictionary<string, int>
            {
                { "Self", (int)ActionTargetType.Self },
                { "NewTab", (int)ActionTargetType.NewTab },
                { "NewWindow", (int)ActionTargetType.NewWindow }
            }));

            // Типы главного компонента
            constants.AppendLine("// Типы главного компонента");
            constants.AppendLine(GenerateEnumeration("Quantumart.QP8.Enums.MainComponentType", new Dictionary<string, int>
            {
                { "Editor", (int)MainComponentType.Editor },
                { "Grid", (int)MainComponentType.Grid },
                { "Tree", (int)MainComponentType.Tree },
                { "Library", (int)MainComponentType.Library },
                { "Area", (int)MainComponentType.Area },
                { "ActionPermissionView", (int)MainComponentType.ActionPermissionView },
                { "CustomActionHost", (int)MainComponentType.CustomActionHost }
            }));

            // Состояния контекста
            constants.AppendLine("// Состояния контекста");
            constants.AppendLine(GenerateEnumeration("Quantumart.QP8.Enums.DocumentContextState", new Dictionary<string, int>
            {
                { "None", (int)DocumentContextState.None },
                { "Loaded", (int)DocumentContextState.Loaded },
                { "Error", (int)DocumentContextState.Error },
                { "Saved", (int)DocumentContextState.Saved }
            }));

            // Типы узлов дерева для управления правилами доступа к действиям
            constants.AppendLine("// Типы узлов дерева для управления правилами доступа к действиям");
            constants.AppendLine(GenerateEnumeration("Quantumart.QP8.Enums.ActionPermissionTreeNodeType", new Dictionary<string, int>
            {
                { "ActionNode", ActionPermissionTreeNode.ACTION_NODE },
                { "EntityTypeNode", ActionPermissionTreeNode.ENTITY_TYPE_NODE }
            }));

            constants.AppendLine("// Фильтры по колонкам Action Log");
            constants.AppendLine(GenerateEnumeration("Quantumart.QP8.Enums.ActionLogFilteredColumns", new Dictionary<string, int>
            {
                { "ActionName", (int)FilteredColumnsEnum.ActionName },
                { "ActionTypeName", (int)FilteredColumnsEnum.ActionTypeName },
                { "EntityStringId", (int)FilteredColumnsEnum.EntityStringId },
                { "EntityTitle", (int)FilteredColumnsEnum.EntityTitle },
                { "EntityTypeName", (int)FilteredColumnsEnum.EntityTypeName },
                { "ExecutionTime", (int)FilteredColumnsEnum.ExecutionTime },
                { "ParentEntityId", (int)FilteredColumnsEnum.ParentEntityId },
                { "UserLogin", (int)FilteredColumnsEnum.UserLogin }
            }));

            constants.AppendLine($"window.CKEDITOR_CONFIG_TIMESTAMP = \"{DateTime.Now.Ticks}\"");
            constants.AppendLine($"window.BACKEND_ACTION_CODE_HIDDEN_NAME = \"{Default.ActionCodeHiddenName}\"");

            // Custom jQuery Events
            constants.AppendLine("window.JQ_CUSTOM_EVENT_ON_FIELD_CHANGED = \"qp8.onFieldChanged\"");
            constants.AppendLine();

            constants.AppendLine("window.$e = Quantumart.QP8.Enums;");

            return constants.ToString();


        }

        private static string GenerateDictionary(string typeName, Dictionary<string, string> items)
        {
            var dictionary = new StringBuilder();
            dictionary.AppendFormat("var {0} = {{ ", typeName);

            foreach (var item in items)
            {
                dictionary.AppendFormat("\"{0}\": \"{1}\", ", item.Key, item.Value);
            }

            dictionary.AppendLine(" };");
            return dictionary.ToString();
        }

        private static string GenerateEnumeration(string typeName, Dictionary<string, int> items)
        {
            var enumeration = new StringBuilder();
            var itemIndex = 0;
            foreach (var key in items.Keys)
            {
                enumeration.AppendLine(itemIndex > 0 ? "," : "");
                enumeration.Append($"   {key}:  {items[key]}");
                itemIndex++;
            }

            return $"{typeName} = {{ {enumeration} }}";
        }
    }
}
