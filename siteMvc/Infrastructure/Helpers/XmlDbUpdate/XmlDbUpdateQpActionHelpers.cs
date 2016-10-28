using System;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.ViewModels.Article;

namespace Quantumart.QP8.WebMvc.Infrastructure.Helpers.XmlDbUpdate
{
    internal static class XmlDbUpdateQpActionHelpers
    {
        internal static bool IsArticleAndHasResultUniqueId(string actionCode)
        {
            return IsArticleAndHasUniqueId(actionCode) && IsActionHasResultId(actionCode);
        }

        internal static bool IsNewArticle(string actionCode)
        {
            return new[]
            {
                ActionCode.AddNewArticle
            }.Contains(actionCode);
        }

        internal static bool IsArticleAndHasUniqueId(string actionCode)
        {
            return new[]
            {
                ActionCode.AddNewArticle,
                ActionCode.EditArticle,
                ActionCode.CreateLikeArticle,
                ActionCode.RemoveArticle,
                ActionCode.MultipleRemoveArticle,
                ActionCode.MultipleRemoveArticleFromArchive, // TODO спросить у Паши, почему есть MultipleRemoveArticleFromArchive, но нет RemoveArticleFromArchive
                ActionCode.MoveArticleToArchive,
                ActionCode.RestoreArticleFromArchive,
                ActionCode.MultiplePublishArticles,
                ActionCode.RestoreArticleFromArchive,
                ActionCode.MultipleMoveArticleToArchive,
                ActionCode.MultipleRestoreArticleFromArchive
            }.Contains(actionCode);
        }

        internal static bool IsActionHasResultId(string actionCode)
        {
            // TODO: С Пашей надо проверить
            return new[]
            {
                ActionCode.CreateLikeField,
                ActionCode.CreateLikeContent,
                ActionCode.CreateLikeArticle,
                ActionCode.AddNewArticle,
                ActionCode.EditArticle
            }.Contains(actionCode);
        }

        internal static bool IsArticleAndStoreUniqueIdInForm(string actionCode)
        {
            return new[]
            {
                ActionCode.AddNewArticle,
                ActionCode.EditArticle
            }.Contains(actionCode);
        }

        internal static string GetFieldName<T>(Expression<Func<ArticleViewModel, T>> fieldExpression)
        {
            return ExpressionHelper.GetExpressionText(fieldExpression);
        }
    }
}
