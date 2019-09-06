// using System;
// using System.Linq;
// using System.Linq.Expressions;
// using Quantumart.QP8.Constants;
// using Quantumart.QP8.WebMvc.ViewModels.Article;
//
// namespace Quantumart.QP8.WebMvc.Infrastructure.Helpers.XmlDbUpdate
// {
//     internal static class XmlDbUpdateQpActionHelpers
//     {
//         internal static bool IsArticleAndHasResultUniqueId(string actionCode) => IsArticleAndHasUniqueId(actionCode) && IsActionHasResultId(actionCode);
//
//         internal static bool IsNewArticle(string actionCode) => new[]
//         {
//             ActionCode.AddNewArticle
//         }.Contains(actionCode);
//
//         internal static bool IsArticleAndHasUniqueId(string actionCode) => new[]
//         {
//             ActionCode.AddNewArticle,
//             ActionCode.EditArticle,
//             ActionCode.CreateLikeArticle,
//             ActionCode.RemoveArticle,
//             ActionCode.MultipleRemoveArticle,
//             ActionCode.MultipleRemoveArticleFromArchive,
//             ActionCode.MoveArticleToArchive,
//             ActionCode.RestoreArticleFromArchive,
//             ActionCode.MultiplePublishArticles,
//             ActionCode.RestoreArticleFromArchive,
//             ActionCode.MultipleMoveArticleToArchive,
//             ActionCode.MultipleRestoreArticleFromArchive
//         }.Contains(actionCode);
//
//         internal static bool IsActionHasResultId(string actionCode) => new[]
//         {
//             // ArticleController
//             ActionCode.AddNewArticle,
//             ActionCode.EditArticle,
//             ActionCode.CreateLikeArticle,
//
//             // PermissionControllerBase
//             ActionCode.AddNewActionPermission,
//             ActionCode.AddNewArticlePermission,
//             ActionCode.AddNewContentPermission,
//             ActionCode.AddNewEntityTypePermission,
//             ActionCode.AddNewSiteFolderPermission,
//             ActionCode.AddNewWorkflowPermission,
//
//             // ContentController
//             ActionCode.AddNewContent,
//             ActionCode.CreateLikeContent,
//             ActionCode.AddNewContentGroup,
//
//             // ContentFolderController
//             ActionCode.AddNewContentFolder,
//             ActionCode.ContentFolderProperties,
//
//             // CustomActionController
//             ActionCode.AddNewCustomAction,
//
//             // FieldController
//             ActionCode.AddNewField,
//             ActionCode.CreateLikeField,
//
//             // FormatController
//             ActionCode.AddNewPageObjectFormat,
//             ActionCode.AddNewTemplateObjectFormat,
//
//             // NotificationController
//             ActionCode.AddNewNotification,
//
//             // ObjectController
//             ActionCode.AddNewPageObject,
//             ActionCode.AddNewTemplateObject,
//
//             // PageController
//             ActionCode.AddNewPage,
//             ActionCode.CreateLikePage,
//
//             // PageTemplateController
//             ActionCode.AddNewPageTemplate,
//
//             // SiteController
//             ActionCode.AddNewSite,
//
//             // SiteFolderController
//             ActionCode.AddNewSiteFolder,
//
//             //StatusTypeController
//             ActionCode.AddNewStatusType,
//
//             // UserController
//             ActionCode.AddNewUser,
//             ActionCode.CreateLikeUser,
//
//             // UserGroupController
//             ActionCode.AddNewUserGroup,
//             ActionCode.CreateLikeUserGroup,
//
//             // VirtualContentController
//             ActionCode.AddNewVirtualContents,
//
//             // VisualEdtiorPluginController
//             ActionCode.VisualEditorPluginProperties,
//             ActionCode.AddNewVisualEditorPlugin,
//
//             // VisualEdtiorStyleController
//             ActionCode.AddNewVisualEditorStyle,
//
//             // WorkflowController
//             ActionCode.AddNewWorkflow
//         }.Contains(actionCode);
//
//         internal static bool IsArticleAndStoreUniqueIdInForm(string actionCode) => new[]
//         {
//             ActionCode.AddNewArticle,
//             ActionCode.EditArticle
//         }.Contains(actionCode);
//
//         internal static string GetFieldName<T>(Expression<Func<ArticleViewModel, T>> fieldExpression) => ExpressionHelper.GetExpressionText(fieldExpression);
//     }
// }
