using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.SessionState;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Configuration;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Constants.Mvc;
using Quantumart.QP8.Resources;
using Quantumart.QP8.WebMvc.ViewModels;
using Quantumart.QP8.WebMvc.ViewModels.Audit;

// ReSharper disable once CheckNamespace
namespace Quantumart.QP8.WebMvc.Backend
{
    public class JsConstants : IHttpHandler, IReadOnlySessionState
    {
        public bool IsReusable => false;

        public void ProcessRequest(HttpContext context)
        {
            var currentTheme = context.Session[HttpContextSession.CurrentCssTheme].ToString();
            var constants = new StringBuilder();

            constants.AppendLine("Type.registerNamespace(\"Quantumart.QP8.Enums\");");

            //Версия приложения
            constants.AppendLine("// Версия приложения");
            constants.AppendLine(GenerateStringConstant("BACKEND_VERSION", new ApplicationInfoRepository().GetCurrentDbVersion()));

            // Константы кодов типов узлов
            constants.AppendLine("// Константы кодов типов узлов");
            constants.AppendLine(GenerateStringConstant("ENTITY_TYPE_CODE_NONE", EntityTypeCode.None));
            constants.AppendLine(GenerateStringConstant("ENTITY_TYPE_CODE_CUSTOMER_CODE", EntityTypeCode.CustomerCode));
            constants.AppendLine(GenerateStringConstant("ENTITY_TYPE_CODE_SITE", EntityTypeCode.Site));
            constants.AppendLine(GenerateStringConstant("ENTITY_TYPE_CODE_CONTENT_GROUP", EntityTypeCode.ContentGroup));
            constants.AppendLine(GenerateStringConstant("ENTITY_TYPE_CODE_CONTENT", EntityTypeCode.Content));
            constants.AppendLine(GenerateStringConstant("ENTITY_TYPE_CODE_ARTICLE", EntityTypeCode.Article));
            constants.AppendLine(GenerateStringConstant("ENTITY_TYPE_CODE_ARCHIVE_ARTICLE", EntityTypeCode.ArchiveArticle));
            constants.AppendLine(GenerateStringConstant("ENTITY_TYPE_CODE_ARTICLE_VERSION", EntityTypeCode.ArticleVersion));
            constants.AppendLine(GenerateStringConstant("ENTITY_TYPE_CODE_FIELD", EntityTypeCode.Field));
            constants.AppendLine(GenerateStringConstant("ENTITY_TYPE_CODE_VIRTUAL_CONTENT", EntityTypeCode.VirtualContent));
            constants.AppendLine(GenerateStringConstant("ENTITY_TYPE_CODE_VIRTUAL_ARTICLE", EntityTypeCode.VirtualArticle));
            constants.AppendLine(GenerateStringConstant("ENTITY_TYPE_CODE_VIRTUAL_FIELD", EntityTypeCode.VirtualField));
            constants.AppendLine(GenerateStringConstant("ENTITY_TYPE_CODE_SITE_FOLDER", EntityTypeCode.SiteFolder));
            constants.AppendLine(GenerateStringConstant("ENTITY_TYPE_CODE_CONTENT_FOLDER", EntityTypeCode.ContentFolder));
            constants.AppendLine(GenerateStringConstant("ENTITY_TYPE_CODE_SITE_FILE", EntityTypeCode.SiteFile));
            constants.AppendLine(GenerateStringConstant("ENTITY_TYPE_CODE_CONTENT_FILE", EntityTypeCode.ContentFile));
            constants.AppendLine(GenerateStringConstant("ENTITY_TYPE_CODE_NOTIFICATION", EntityTypeCode.Notification));
            constants.AppendLine(GenerateStringConstant("ENTITY_TYPE_CODE_WORKFLOW", EntityTypeCode.Workflow));
            constants.AppendLine(GenerateStringConstant("ENTITY_TYPE_CODE_STATUS", EntityTypeCode.StatusType));
            constants.AppendLine(GenerateStringConstant("ENTITY_TYPE_CODE_STYLE", EntityTypeCode.Style));
            constants.AppendLine(GenerateStringConstant("ENTITY_TYPE_CODE_SNIPPET", EntityTypeCode.Snippet));
            constants.AppendLine(GenerateStringConstant("ENTITY_TYPE_CODE_USER", EntityTypeCode.User));
            constants.AppendLine(GenerateStringConstant("ENTITY_TYPE_CODE_USER_GROUP", EntityTypeCode.UserGroup));
            constants.AppendLine(GenerateStringConstant("ENTITY_TYPE_CODE_CUSTOM_ACTION", EntityTypeCode.CustomAction));
            constants.AppendLine(GenerateStringConstant("ENTITY_TYPE_CODE_USER", EntityTypeCode.User));
            constants.AppendLine(GenerateStringConstant("ENTITY_TYPE_CODE_CONTENT_PERMISSION", EntityTypeCode.ContentPermission));
            constants.AppendLine(GenerateStringConstant("ENTITY_TYPE_CODE_ARTICLE_PERMISSION", EntityTypeCode.ArticlePermission));
            constants.AppendLine(GenerateStringConstant("ENTITY_TYPE_CODE_SITE_PERMISSION", EntityTypeCode.SitePermission));
            constants.AppendLine(GenerateStringConstant("ENTITY_TYPE_CODE_WORKFLOW_PERMISSION", EntityTypeCode.WorkflowPermission));
            constants.AppendLine(GenerateStringConstant("ENTITY_TYPE_CODE_SITE_FODER_TYPE_PERMISSION", EntityTypeCode.SiteFolderPermission));
            constants.AppendLine(GenerateStringConstant("ENTITY_TYPE_CODE_ENTITY_TYPE_PERMISSION", EntityTypeCode.EntityTypePermission));
            constants.AppendLine(GenerateStringConstant("ENTITY_TYPE_CODE_ACTION_PERMISSION", EntityTypeCode.ActionPermission));
            constants.AppendLine();

            // Константы типов действий
            constants.AppendLine("// Константы типов действий");
            constants.AppendLine(GenerateStringConstant("ACTION_TYPE_CODE_NONE", ActionTypeCode.None));
            constants.AppendLine(GenerateStringConstant("ACTION_TYPE_CODE_REFRESH", ActionTypeCode.Refresh));
            constants.AppendLine(GenerateStringConstant("ACTION_TYPE_CODE_ADD_NEW", ActionTypeCode.AddNew));
            constants.AppendLine(GenerateStringConstant("ACTION_TYPE_CODE_LIST", ActionTypeCode.List));
            constants.AppendLine(GenerateStringConstant("ACTION_TYPE_CODE_SELECT", ActionTypeCode.Select));
            constants.AppendLine(GenerateStringConstant("ACTION_TYPE_CODE_MULTIPLE_SELECT", ActionTypeCode.MultipleSelect));
            constants.AppendLine(GenerateStringConstant("ACTION_TYPE_CODE_READ", ActionTypeCode.Read));
            constants.AppendLine(GenerateStringConstant("ACTION_TYPE_CODE_LIBRARY", ActionTypeCode.Library));
            constants.AppendLine(GenerateStringConstant("ACTION_TYPE_CODE_PREVIEW", ActionTypeCode.Preview));
            constants.AppendLine(GenerateStringConstant("ACTION_TYPE_CODE_CROP", ActionTypeCode.Crop));
            constants.AppendLine(GenerateStringConstant("ACTION_TYPE_CODE_ALL_FILES_UPLOADED", ActionTypeCode.AllFilesUploaded));
            constants.AppendLine(GenerateStringConstant("ACTION_TYPE_CODE_FILE_CROPPED", ActionTypeCode.FileCropped));
            constants.AppendLine(GenerateStringConstant("ACTION_TYPE_CODE_DOWNLOAD", ActionTypeCode.Download));
            constants.AppendLine(GenerateStringConstant("ACTION_TYPE_CODE_REMOVE", ActionTypeCode.Remove));
            constants.AppendLine(GenerateStringConstant("ACTION_TYPE_CODE_MULTIPLE_REMOVE", ActionTypeCode.MultipleRemove));
            constants.AppendLine(GenerateStringConstant("ACTION_TYPE_CODE_SAVE", ActionTypeCode.Save));
            constants.AppendLine(GenerateStringConstant("ACTION_TYPE_CODE_UPDATE", ActionTypeCode.Update));
            constants.AppendLine(GenerateStringConstant("ACTION_TYPE_CODE_SAVE_AND_UP", ActionTypeCode.SaveAndUp));
            constants.AppendLine(GenerateStringConstant("ACTION_TYPE_CODE_UPDATE_AND_UP", ActionTypeCode.UpdateAndUp));
            constants.AppendLine(GenerateStringConstant("ACTION_TYPE_CODE_CANCEL", ActionTypeCode.Cancel));
            constants.AppendLine(GenerateStringConstant("ACTION_TYPE_CODE_COPY", ActionTypeCode.Copy));
            constants.AppendLine(GenerateStringConstant("ACTION_TYPE_CODE_ARCHIVE", ActionTypeCode.Archive));
            constants.AppendLine(GenerateStringConstant("ACTION_TYPE_CODE_MULTIPLE_ARCHIVE", ActionTypeCode.MultipleArchive));
            constants.AppendLine(GenerateStringConstant("ACTION_TYPE_CODE_RESTORE", ActionTypeCode.Restore));
            constants.AppendLine(GenerateStringConstant("ACTION_TYPE_CODE_MULTIPLE_RESTORE", ActionTypeCode.MultipleRestore));
            constants.AppendLine(GenerateStringConstant("ACTION_TYPE_CODE_CHANGE_LOCK", ActionTypeCode.ChangeLock));
            constants.AppendLine(GenerateStringConstant("ACTION_TYPE_CODE_MULTIPLE_UNLOCK", ActionTypeCode.MultipleUnlock));
            constants.AppendLine(GenerateStringConstant("ACTION_TYPE_CODE_SEARCH", ActionTypeCode.Search));
            constants.AppendLine(GenerateStringConstant("ACTION_TYPE_CODE_DESELECT_ALL", ActionTypeCode.DeselectAll));
            constants.AppendLine(GenerateStringConstant("ACTION_TYPE_SIMPLE_UPDATE", ActionTypeCode.SimpleUpdate));
            constants.AppendLine(GenerateStringConstant("ACTION_TYPE_CHILD_ENTITY_PERMISSION_SAVE", ActionTypeCode.ChildEntityPermissionSave));
            constants.AppendLine(GenerateStringConstant("ACTION_TYPE_CHILD_ENTITY_MULTIPLE_REMOVE", ActionTypeCode.MultipleChildEntityPermissionRemove));
            constants.AppendLine(GenerateStringConstant("ACTION_TYPE_CHILD_ENTITY_REMOVE_ALL", ActionTypeCode.ChildEntityPermissionRemoveAll));
            constants.AppendLine(GenerateStringConstant("ACTION_TYPE_CHILD_ENTITY_REMOVE", ActionTypeCode.ChildEntityPermissionRemove));
            constants.AppendLine(GenerateStringConstant("ACTION_TYPE_ACTION_PERMISSION_TREE", ActionTypeCode.ActionPermissionTree));
            constants.AppendLine(GenerateStringConstant("ACTION_TYPE_SELECT_CHILD_ARTICLES", ActionTypeCode.SelectChildArticles));
            constants.AppendLine(GenerateStringConstant("ACTION_TYPE_UNSELECT_CHILD_ARTICLES", ActionTypeCode.UnselectChildArticles));

            constants.AppendLine();
            constants.AppendLine(GenerateIntegerConstant("MAX_ITEMS_AFFECTED_NUMBER", 255));
            constants.AppendLine(GenerateStringConstant("CHANGED_FIELD_CLASS_NAME", CssClasses.ChangedField));
            constants.AppendLine(GenerateStringConstant("REFRESHED_FIELD_CLASS_NAME", CssClasses.RefreshedField));
            constants.AppendLine();

            // Константы действий
            constants.AppendLine("// Константы действий");
            constants.AppendLine(GenerateStringConstant("ACTION_CODE_UPDATE_SITE_FILE", ActionCode.UpdateSiteFile));
            constants.AppendLine(GenerateStringConstant("ACTION_CODE_UPDATE_CONTENT_FILE", ActionCode.UpdateContentFile));
            constants.AppendLine(GenerateStringConstant("ACTION_CODE_NONE", ActionCode.None));
            constants.AppendLine(GenerateStringConstant("ACTION_CODE_EDIT_PROFILE", ActionCode.EditProfile));
            constants.AppendLine(GenerateStringConstant("ACTION_CODE_ADD_NEW_SITE", ActionCode.AddNewSite));
            constants.AppendLine(GenerateStringConstant("ACTION_CODE_ARTICLES", ActionCode.Articles));
            constants.AppendLine(GenerateStringConstant("ACTION_CODE_SELECT_ARTICLE", ActionCode.SelectArticle));
            constants.AppendLine(GenerateStringConstant("ACTION_CODE_MULTIPLE_SELECT_ARTICLE", ActionCode.MultipleSelectArticle));
            constants.AppendLine(GenerateStringConstant("ACTION_CODE_MULTIPLE_PUBLISH_ARTICLES", ActionCode.MultiplePublishArticles));
            constants.AppendLine(GenerateStringConstant("ACTION_CODE_ADD_NEW_ARTICLE", ActionCode.AddNewArticle));
            constants.AppendLine(GenerateStringConstant("ACTION_CODE_PREVIEW_ARTICLE_VERSION", ActionCode.PreviewArticleVersion));
            constants.AppendLine(GenerateStringConstant("ACTION_CODE_COMPARE_ARTICLE_VERSION_WITH_CURRENT", ActionCode.CompareArticleVersionWithCurrent));
            constants.AppendLine(GenerateStringConstant("ACTION_CODE_SITE_LIBRARY", ActionCode.SiteLibrary));
            constants.AppendLine(GenerateStringConstant("ACTION_CODE_CONTENT_LIBRARY", ActionCode.ContentLibrary));
            constants.AppendLine(GenerateStringConstant("ACTION_CODE_POPUP_SITE_LIBRARY", ActionCode.PopupSiteLibrary));
            constants.AppendLine(GenerateStringConstant("ACTION_CODE_POPUP_CONTENT_LIBRARY", ActionCode.PopupContentLibrary));
            constants.AppendLine(GenerateStringConstant("ACTION_CODE_ENABLE_ARTICLES_PERMISSIONS", ActionCode.EnableArticlesPermissions));
            constants.AppendLine(GenerateStringConstant("ACTION_CODE_CONTENTS", ActionCode.Contents));

            constants.AppendLine(GenerateStringConstant("ACTION_CODE_CONTENT_PERMISSIONS", ActionCode.ContentPermissions));
            constants.AppendLine(GenerateStringConstant("ACTION_CODE_CHILD_CONTENT_PERMISSIONS", ActionCode.ChildContentPermissions));
            constants.AppendLine(GenerateStringConstant("ACTION_CODE_CONTENT_PERMISSIONS_FOR_CHILD", ActionCode.ContentPermissionsForChild));

            constants.AppendLine(GenerateStringConstant("ASSEMBLE_PARENT_ACTION_TYPE", "assemble_parent"));

            constants.AppendLine(GenerateStringConstant("ACTION_CODE_ARTICLE_PERMISSIONS", ActionCode.ArticlePermissions));
            constants.AppendLine(GenerateStringConstant("ACTION_CODE_CHILD_ARTICLE_PERMISSIONS", ActionCode.ChildArticlePermissions));
            constants.AppendLine(GenerateStringConstant("ACTION_CODE_ARTICLE_PERMISSIONS_FOR_CHILD", ActionCode.ArticlePermissionsForChild));

            constants.AppendLine(GenerateStringConstant("ACTION_CODE_NEW_ENTITY_TYPE_PERMISSION", ActionCode.AddNewEntityTypePermission));
            constants.AppendLine(GenerateStringConstant("ACTION_CODE_EDIT_ENTITY_TYPE_PERMISSION", ActionCode.EntityTypePermissionProperties));
            constants.AppendLine(GenerateStringConstant("ACTION_CODE_CHANGE_ENTITY_TYPE_PERMISSION_NODE", ActionCode.ChangeEntityTypePermission));
            constants.AppendLine(GenerateStringConstant("ACTION_CODE_UPDATE_ENTITY_TYPE_PERMISSION_NODE", ActionCode.UpdateEntityTypePermissionChanges));
            constants.AppendLine(GenerateStringConstant("ACTION_CODE_REMOVE_ENTITY_TYPE_PERMISSION_NODE", ActionCode.RemoveEntityTypePermissionChanges));

            constants.AppendLine(GenerateStringConstant("ACTION_CODE_NEW_ACTION_PERMISSION", ActionCode.AddNewActionPermission));
            constants.AppendLine(GenerateStringConstant("ACTION_CODE_EDIT_ACTION_PERMISSION", ActionCode.ActionPermissionProperties));
            constants.AppendLine(GenerateStringConstant("ACTION_CODE_CHANGE_ACTION_PERMISSION_NODE", ActionCode.ChangeActionPermission));
            constants.AppendLine(GenerateStringConstant("ACTION_CODE_UPDATE_ACTION_PERMISSION_NODE", ActionCode.UpdateActionPermissionChanges));
            constants.AppendLine(GenerateStringConstant("ACTION_CODE_REMOVE_ACTION_PERMISSION_NODE", ActionCode.RemoveActionPermissionChanges));

            constants.AppendLine(GenerateStringConstant("ACTION_CODE_CHANGE_CHILD_ARTICLE_PERMISSION", ActionCode.ChangeChildArticlePermission));
            constants.AppendLine(GenerateStringConstant("ACTION_CODE_CHANGE_CHILD_CONTENT_PERMISSION", ActionCode.ChangeChildContentPermission));
            constants.AppendLine(GenerateStringConstant("ACTION_CODE_REMOVE_CHILD_ARTICLE_PERMISSION", ActionCode.RemoveChildContentPermission));
            constants.AppendLine(GenerateStringConstant("ACTION_CODE_REMOVE_CHILD_CONTENT_PERMISSION", ActionCode.RemoveChildArticlePermission));

            constants.AppendLine(GenerateStringConstant("ACTION_CODE_SELECT_USER_GROUP", ActionCode.SelectUserGroup));
            constants.AppendLine(GenerateStringConstant("ACTION_CODE_SELECT_USER", ActionCode.SelectUser));
            constants.AppendLine(GenerateStringConstant("ACTION_CODE_MULTIPLE_SELECT_USER", ActionCode.MultipleSelectUser));

            constants.AppendLine(GenerateStringConstant("ACTION_CODE_ADD_NEW_CHILD_ARTICLE", ActionCode.AddNewChildArticle));
            constants.AppendLine(GenerateStringConstant("ACTION_CODE_ADD_NEW_ADJACENT_FIELD", ActionCode.AddNewAdjacentField));
            constants.AppendLine();

            // Коды типов представлений
            constants.AppendLine("// Коды типов представлений");
            constants.AppendLine(GenerateStringConstant("VIEW_TYPE_CODE_NONE", ViewTypeCode.None));
            constants.AppendLine(GenerateStringConstant("VIEW_TYPE_CODE_LIST", ViewTypeCode.List));
            constants.AppendLine(GenerateStringConstant("VIEW_TYPE_CODE_TREE", ViewTypeCode.Tree));
            constants.AppendLine(GenerateStringConstant("VIEW_TYPE_CODE_DETAILS", ViewTypeCode.Details));
            constants.AppendLine(GenerateStringConstant("VIEW_TYPE_CODE_THUMBNAILS", ViewTypeCode.Thumbnails));
            constants.AppendLine();

            // Конcтанты кодов типов данных JavaScript
            constants.AppendLine("// Конcтанты кодов типов данных JavaScript");
            constants.AppendLine(GenerateStringConstant("JS_TYPE_CODE_UNDEFINED", Utils.Constants.JsTypeCode.Undefined));
            constants.AppendLine(GenerateStringConstant("JS_TYPE_CODE_STRING", Utils.Constants.JsTypeCode.String));
            constants.AppendLine(GenerateStringConstant("JS_TYPE_CODE_INT", Utils.Constants.JsTypeCode.Int));
            constants.AppendLine(GenerateStringConstant("JS_TYPE_CODE_FLOAT", Utils.Constants.JsTypeCode.Float));
            constants.AppendLine(GenerateStringConstant("JS_TYPE_CODE_BOOLEAN", Utils.Constants.JsTypeCode.Boolean));
            constants.AppendLine(GenerateStringConstant("JS_TYPE_CODE_DATE", Utils.Constants.JsTypeCode.Date));
            constants.AppendLine();

            // Константы типов сообщений, возвращаемых Actions
            constants.AppendLine(GenerateStringConstant("ACTION_MESSAGE_TYPE_INFO", ActionMessageType.Info));
            constants.AppendLine(GenerateStringConstant("ACTION_MESSAGE_TYPE_WARNING", ActionMessageType.Warning));
            constants.AppendLine(GenerateStringConstant("ACTION_MESSAGE_TYPE_ERROR", ActionMessageType.Error));
            constants.AppendLine(GenerateStringConstant("ACTION_MESSAGE_TYPE_CONFIRM", ActionMessageType.Confirm));
            constants.AppendLine(GenerateStringConstant("ACTION_MESSAGE_TYPE_DOWNLOAD", ActionMessageType.Download));
            constants.AppendLine();

            // Константы типов валидации
            //constants.AppendLine("// Константы типов валидации");
            //constants.AppendLine(GenerateStringConstant("VALIDATION_TYPE_UNKNOWN", Validators.Constants.ValidationType.None));
            //constants.AppendLine(GenerateStringConstant("VALIDATION_TYPE_REQUIRED", Validators.Constants.ValidationType.Required));
            //constants.AppendLine(GenerateStringConstant("VALIDATION_TYPE_STRING_LENGTH", Validators.Constants.ValidationType.StringLength));
            //constants.AppendLine(GenerateStringConstant("VALIDATION_TYPE_CONTAINS_CHARACTERS", Validators.Constants.ValidationType.ContainsCharacters));
            //constants.AppendLine(GenerateStringConstant("VALIDATION_TYPE_REGEX", Validators.Constants.ValidationType.Regex));
            //constants.AppendLine(GenerateStringConstant("VALIDATION_TYPE_TYPE_CONVERSION", Validators.Constants.ValidationType.TypeConversion));
            //constants.AppendLine(GenerateStringConstant("VALIDATION_TYPE_VALUE_COMPARISON", Validators.Constants.ValidationType.ValueComparison));
            //constants.AppendLine(GenerateStringConstant("VALIDATION_TYPE_PROPERTY_COMPARISON", Validators.Constants.ValidationType.PropertyComparison));
            //constants.AppendLine(GenerateStringConstant("VALIDATION_TYPE_RANGE", Validators.Constants.ValidationType.Range));
            //constants.AppendLine();

            // Костанты URL`ов
            constants.AppendLine(GenerateStringConstant("APPLICATION_ROOT_URL", Utils.Url.ToAbsolute("~/")));

            // Константы URL`ов контроллеров
            constants.AppendLine("// Константы URL`ов контроллеров");
            constants.AppendLine(GenerateStringConstant("CONTROLLER_URL_TREE_MENU", Utils.Url.ToAbsolute("~/TreeMenu/")));
            constants.AppendLine(GenerateStringConstant("CONTROLLER_URL_CONTEXT_MENU", Utils.Url.ToAbsolute("~/ContextMenu/")));
            constants.AppendLine(GenerateStringConstant("CONTROLLER_URL_TOOLBAR", Utils.Url.ToAbsolute("~/Toolbar/")));
            constants.AppendLine(GenerateStringConstant("CONTROLLER_URL_ENTITY_TYPE", Utils.Url.ToAbsolute("~/EntityType/")));
            constants.AppendLine(GenerateStringConstant("CONTROLLER_URL_ENTITY_OBJECT", Utils.Url.ToAbsolute("~/EntityObject/")));
            constants.AppendLine(GenerateStringConstant("CONTROLLER_URL_BACKEND_ACTION", Utils.Url.ToAbsolute("~/BackendAction/")));
            constants.AppendLine(GenerateStringConstant("CONTROLLER_URL_BACKEND_ACTION_TYPE", Utils.Url.ToAbsolute("~/BackendActionType/")));
            constants.AppendLine(GenerateStringConstant("CONTROLLER_URL_ARTICLE_SEARCH_BLOCK", Utils.Url.ToAbsolute("~/ArticleSearchBlock/")));
            constants.AppendLine(GenerateStringConstant("CONTROLLER_URL_WORKFLOW", Utils.Url.ToAbsolute("~/Workflow/")));
            constants.AppendLine(GenerateStringConstant("CONTROLLER_URL_CONTENT_SEARCH_BLOCK", Utils.Url.ToAbsolute("~/Content/")));
            constants.AppendLine(GenerateStringConstant("CONTROLLER_URL_SITE", Utils.Url.ToAbsolute("~/Site/")));
            constants.AppendLine(GenerateStringConstant("CONTROLLER_URL_CONTENT", Utils.Url.ToAbsolute("~/Content/")));
            constants.AppendLine(GenerateStringConstant("CONTROLLER_URL_ARTICLE", Utils.Url.ToAbsolute("~/Article/")));
            constants.AppendLine(GenerateStringConstant("CONTROLLER_URL_THUMBNAIL", Utils.Url.ToAbsolute("~/Thumbnail/")));
            constants.AppendLine(GenerateStringConstant("CONTROLLER_URL_FIELD", Utils.Url.ToAbsolute("~/Field/")));
            constants.AppendLine(GenerateStringConstant("CONTROLLER_URL_VIRTUAL_CONTENT", Utils.Url.ToAbsolute("~/VirtualContent/")));
            constants.AppendLine(GenerateStringConstant("CONTROLLER_URL_USER", Utils.Url.ToAbsolute("~/User/")));
            constants.AppendLine(GenerateStringConstant("CONTROLLER_URL_ACTION_PERMISSION_TREE", Utils.Url.ToAbsolute("~/ActionPermissionTree/")));
            constants.AppendLine(GenerateStringConstant("CONTROLLER_URL_LOGON", Utils.Url.ToAbsolute("~/LogOn/")));
            constants.AppendLine(GenerateStringConstant("CONTROLLER_URL_WINLOGON", Utils.Url.ToAbsolute("~/WinLogOn/")));
            constants.AppendLine(GenerateStringConstant("CONTROLLER_URL_PAGE_TEMPLATE", Utils.Url.ToAbsolute("~/PageTemplate/")));
            constants.AppendLine(GenerateStringConstant("CONTROLLER_URL_CUSTOM_ACTION", Utils.Url.ToAbsolute("~/CustomAction/")));
            constants.AppendLine(GenerateStringConstant("CONTROLLER_URL_AUTH", Utils.Url.ToAbsolute("~/LogOn/")));
            constants.AppendLine();

            // Константы URL`ов директорий с рисунками
            constants.AppendLine("// Константы URL`ов директорий с рисунками");
            constants.AppendLine(GenerateStringConstant("COMMON_IMAGE_FOLDER_URL_ROOT", SitePathHelper.GetCommonRootImageFolderUrl()));
            constants.AppendLine(GenerateStringConstant("THEME_IMAGE_FOLDER_URL_ROOT", SitePathHelper.GetThemeRootImageFolderUrl(currentTheme)));
            constants.AppendLine(GenerateStringConstant("THEME_IMAGE_FOLDER_URL_SMALL_ICONS", SitePathHelper.GetThemeSmallIconsImageFolderUrl(currentTheme)));
            constants.AppendLine(GenerateStringConstant("THEME_IMAGE_FOLDER_URL_AJAX_LOADER_ICONS", SitePathHelper.GetThemeAjaxLoaderIconsImageFolderUrl(currentTheme)));
            constants.AppendLine(GenerateStringConstant("THEME_IMAGE_FOLDER_URL_SMALL_FILE_TYPE_ICONS", SitePathHelper.GetThemeSmallFileTypeIconFolderUrl(currentTheme)));
            constants.AppendLine(GenerateStringConstant("THEME_IMAGE_FOLDER_URL_BIG_FILE_TYPE_ICONS", SitePathHelper.GetThemeBigFileTypeIconFolderUrl(currentTheme)));

            // Коды контекстного меню
            constants.AppendLine("// Коды контекстного меню");
            constants.AppendLine(GenerateStringConstant("CONTEXT_MENU_CODE_ENTITY_TYPE_PERMISSION_NODE", ContextMenuCodes.ActionPermissionEntityTypeNode));
            constants.AppendLine(GenerateStringConstant("CONTEXT_MENU_CODE_ACTION_PERMISSION_NODE", ContextMenuCodes.ActionPermissionActionNode));

            // Константы серверного окружения
            constants.AppendLine("// Константы серверного окружения");
            constants.AppendLine(GenerateIntegerConstant("MAX_UPLOAD_SIZE_BYTES", QPConfiguration.WebConfigSection.UploadMaxSize * 1024 * 1024));

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
            constants.AppendLine(GenerateIntegerConstant("DATE_TIME_PICKER_MODE_DATE", DateTimePickerMode.Date));
            constants.AppendLine(GenerateIntegerConstant("DATE_TIME_PICKER_MODE_TIME", DateTimePickerMode.Time));
            constants.AppendLine(GenerateIntegerConstant("DATE_TIME_PICKER_MODE_DATE_TIME", DateTimePickerMode.DateTime));
            constants.AppendLine();

            // Локализация Silverlight Upload
            constants.AppendLine(GenerateStringConstant("UPLOAD_BROWSE_BUTTON_NAME", LibraryStrings.Upload));
            constants.AppendLine(GenerateStringConstant("UPLOAD_TOTAL_LABEL", LibraryStrings.Total));
            constants.AppendLine(GenerateStringConstant("UPLOAD_MAX_SIZE_MESSAGE", LibraryStrings.MaxSizeExceeded));
            constants.AppendLine(GenerateStringConstant("UPLOAD_OVERWRITE_MESSAGE", LibraryStrings.AlreadyExists));
            constants.AppendLine(GenerateStringConstant("UPLOAD_EXTENSION_MESSAGE", LibraryStrings.NotAllowedExtensionForUpload));
            constants.AppendLine(GenerateStringConstant("UPLOAD_SECURITY_MESSAGE", LibraryStrings.UploadIsNotAllowed));
            constants.AppendLine(GenerateStringConstant("HTML_UPLOAD_MAX_SIZE_MESSAGE", LibraryStrings.HtmlUploaderMaxSizeExceeded));
            constants.AppendLine(GenerateStringConstant("HTML_UPLOAD_ERROR_MESSAGE", LibraryStrings.HtmlUploaderErrorMesage));

            // Константы PlUpload
            constants.AppendLine(GenerateStringConstant("PL_IMAGE_RESOLUTION", Plupload.ImageResolution.ToString()));

            // Локализация PlUpload
            constants.AppendLine(GenerateStringConstant("PL_UPLOAD_ERROR_REPORT", LibraryStrings.PlUploadErrorReport));
            constants.AppendLine(GenerateStringConstant("PL_UPLOAD_ZERO_SIZE_WARN", LibraryStrings.ZeroSizeWarning));

            // Типы полей
            constants.AppendLine(GenerateStringConstant("STRING_FIELD_TYPE", FieldExactTypes.String.ToString()));
            constants.AppendLine(GenerateStringConstant("NUMERIC_FIELD_TYPE", FieldExactTypes.Numeric.ToString()));
            constants.AppendLine(GenerateStringConstant("BOOLEAN_FIELD_TYPE", FieldExactTypes.Boolean.ToString()));
            constants.AppendLine(GenerateStringConstant("DATE_FIELD_TYPE", FieldExactTypes.Date.ToString()));
            constants.AppendLine(GenerateStringConstant("TIME_FIELD_TYPE", FieldExactTypes.Time.ToString()));
            constants.AppendLine(GenerateStringConstant("DATETIME_FIELD_TYPE", FieldExactTypes.DateTime.ToString()));
            constants.AppendLine(GenerateStringConstant("FILE_FIELD_TYPE", FieldExactTypes.File.ToString()));
            constants.AppendLine(GenerateStringConstant("IMAGE_FIELD_TYPE", FieldExactTypes.Image.ToString()));
            constants.AppendLine(GenerateStringConstant("DYNAMIC_IMAGE_FIELD_TYPE", FieldExactTypes.DynamicImage.ToString()));
            constants.AppendLine(GenerateStringConstant("TEXTBOX_FIELD_TYPE", FieldExactTypes.Textbox.ToString()));
            constants.AppendLine(GenerateStringConstant("VISUAL_EDIT_FIELD_TYPE", FieldExactTypes.VisualEdit.ToString()));
            constants.AppendLine(GenerateStringConstant("O2M_RELATION_FIELD_TYPE", FieldExactTypes.O2MRelation.ToString()));
            constants.AppendLine(GenerateStringConstant("M2M_RELATION_FIELD_TYPE", FieldExactTypes.M2MRelation.ToString()));
            constants.AppendLine(GenerateStringConstant("M2O_RELATION_FIELD_TYPE", FieldExactTypes.M2ORelation.ToString()));
            constants.AppendLine(GenerateStringConstant("STRING_ENUM_FIELD_TYPE", FieldExactTypes.StringEnum.ToString()));
            constants.AppendLine(GenerateStringConstant("CLASSIFIER_FIELD_TYPE", FieldExactTypes.Classifier.ToString()));
            constants.AppendLine();

            //Локализация HTA
            constants.AppendLine(GenerateStringConstant("HTA_DEFAULT_CONFIRM", TemplateStrings.ReplaceConfirmation));
            constants.AppendLine(GenerateStringConstant("HTA_INSERT_CALL", TemplateStrings.InsertCall));

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

            // Типы компонентов загрузки файлов
            constants.AppendLine("// Типы компонентов загрузки файлов");
            constants.AppendLine(GenerateEnumeration("Quantumart.QP8.Enums.UploaderType", new Dictionary<string, int>
            {
                { "PlUpload", (int)UploaderType.PlUpload },
                { "Silverlight", (int)UploaderType.Silverlight },
                { "Html", (int)UploaderType.Html }
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

            constants.AppendLine(GenerateStringConstant("CKEDITOR_CONFIG_TIMESTAMP", DateTime.Now.Ticks.ToString()));
            constants.AppendLine(GenerateStringConstant("BACKEND_ACTION_CODE_HIDDEN_NAME", Default.ActionCodeHiddenName));

            // Custom jQuery Events
            constants.AppendLine(GenerateStringConstant("JQ_CUSTOM_EVENT_ON_FIELD_CHANGED", "qp8.onFieldChanged"));
            constants.AppendLine();

            constants.AppendLine("window.$e = Quantumart.QP8.Enums;");

            context.Response.ContentType = "text/javascript";
            context.Response.Write(constants.ToString());

            constants.Clear();
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

        private static string GenerateStringConstant(string name, string value)
        {
            return $"var {name} = \"{value}\";";
        }

        private static string GenerateIntegerConstant(string name, int value)
        {
            return $"var {name} = {value};";
        }

        private static string GenerateEnumeration(string typeName, Dictionary<string, int> items)
        {
            var enumeration = new StringBuilder();
            enumeration.AppendLine(typeName + " = function() {};");
            enumeration.AppendLine(typeName + ".prototype = {");

            var itemIndex = 0;
            foreach (var key in items.Keys)
            {
                enumeration.AppendLine(itemIndex > 0 ? "," : "");
                enumeration.Append($"   {key}:  {items[key]}");
                itemIndex++;
            }

            enumeration.AppendLine("");
            enumeration.AppendLine("};");
            enumeration.AppendLine(typeName + ".registerEnum(\"" + typeName + "\");");

            return enumeration.ToString();
        }
    }
}
