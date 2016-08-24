using System;
using System.Collections.Generic;
using Quantumart.QP8.BLL.Exceptions;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Repository.Articles;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL.Services
{
    public class ArticleVersionService
    {

        #region Private Members
        private static void Exchange(ref int id1, ref int id2)
        {
            var temp = id1;
            id1 = id2;
            id2 = temp;
        }

        private static Tuple<int, int> GetOrderedIds(int[] ids)
        {
            var id1 = ids[0];
            var id2 = ids[1];

            if (id1 > id2)
                Exchange(ref id1, ref id2);

            if (id1 == ArticleVersion.CurrentVersionId)
                Exchange(ref id1, ref id2);

            return Tuple.Create(id1, id2);
        }
        #endregion

        /// <summary>
        /// Возвращает версию статьи для просмотра
        /// </summary>
        /// <param name="id">ID версии</param>
        /// <param name="articleId">ID статьи (необходим для текущей версии)</param>
        /// <returns>версия</returns>
        public static ArticleVersion Read(int id, int articleId = 0)
        {
            var result = ArticleVersionRepository.GetById(id, articleId);
            if (result == null)
                throw new Exception(string.Format(ArticleStrings.ArticleVersionNotFoundForArticle, id, articleId));
            result.Article.ViewType = ArticleViewType.PreviewVersion;
            return result;
        }

        /// <summary>
        /// Возвращает информацию о библиотеке версии для просмотра/download файлов
        /// </summary>
        /// <param name="fieldId">ID поля</param>
        /// <param name="id">ID версии</param>
        /// <returns>информацию о библиотеке</returns>
        public static PathInfo GetPathInfo(int fieldId, int id)
        {
            if (id != ArticleVersion.CurrentVersionId)
            {
                var result = ArticleVersionRepository.GetById(id);
                if (result == null)
                    throw new Exception(string.Format(ArticleStrings.ArticleVersionNotFound, id));
                return result.PathInfo;
            }
            var field = FieldRepository.GetById(fieldId);
            if (field == null)
                throw new Exception(string.Format(FieldStrings.FieldNotFound, fieldId));
            return field.Content.GetVersionPathInfo(ArticleVersion.CurrentVersionId);
        }


        /// <summary>
        /// Возвращает список версий статей
        /// </summary>
        /// <param name="articleId">идентификатор статьи</param>
        /// <param name="command"></param>
        /// <returns>список версий статей</returns>
        public static List<ArticleVersion> List(int articleId, ListCommand command)
        {
            command.SortExpression = ArticleVersion.TranslateSortExpression(command.SortExpression);
            return ArticleVersionRepository.GetList(articleId, command);
        }

        
        /// <summary>
        /// Осуществляет слияние версий статей для сравнения
        /// </summary>
        /// <param name="ids">массив ID версий</param>
        /// <param name="parentId">ID статьи</param>
        /// <returns>объединенная версия</returns>
        public static ArticleVersion GetMergedVersion(int[] ids, int parentId)
        {
            if (ids == null)
                throw new ArgumentNullException(nameof(ids));
            if (ids.Length != 2)
                throw new ArgumentException("Wrong ids length");
            
            var result = GetOrderedIds(ids);
            var version1 = ArticleVersionRepository.GetById(result.Item1, parentId);
            if (version1 == null)
                throw new Exception(string.Format(ArticleStrings.ArticleVersionNotFoundForArticle, result.Item1, parentId));

            ArticleVersion version2;
            if (result.Item2 == ArticleVersion.CurrentVersionId)
            {
                var parent = ArticleRepository.GetById(parentId);
                if (parent == null)
                    throw new Exception(string.Format(ArticleStrings.ArticleNotFound, parentId));
                version2 = new ArticleVersion { ArticleId = parent.Id, Article = parent, Id = ArticleVersion.CurrentVersionId, Modified = parent.Modified, LastModifiedBy = parent.LastModifiedBy, LastModifiedByUser = parent.LastModifiedByUser };
            }
            else
            {
                version2 = ArticleVersionRepository.GetById(result.Item2, parentId);
                if (version2 == null)
                    throw new Exception(string.Format(ArticleStrings.ArticleVersionNotFoundForArticle, result.Item2, parentId));
            }
            version1.MergeToVersion(version2);
            version1.Article.ViewType = ArticleViewType.CompareVersions;
            version1.AggregatedArticles.ForEach(x => { x.ViewType = ArticleViewType.CompareVersions; });
            return version1;
        }


        /// <summary>
        /// Удаляет версию статьи
        /// </summary>
        /// <param name="id">ID версии</param>
        /// <param name="boundToExternal"></param>
        public static MessageResult Remove(int id, bool? boundToExternal)
        {
            var version = ArticleVersionRepository.GetById(id);
            if (version == null)
                throw new Exception(string.Format(ArticleStrings.ArticleVersionNotFound, id));

            if (!version.Article.IsUpdatable)
                return MessageResult.Error(ArticleStrings.CannotRemoveVersionsBecauseOfSecurity);
            if (!version.Article.IsArticleChangingActionsAllowed(boundToExternal))
                return MessageResult.Error(ContentStrings.ArticleChangingIsProhibited);
            version.Article.RemoveVersionFolder(id);
            ArticleVersionRepository.Delete(id);
            return null;
        }

        /// <summary>
        /// Удаляет версии статьи
        /// </summary>
        /// <param name="ids">массив ID версий</param>
        /// <param name="boundToExternal"></param>
        public static MessageResult MultipleRemove(int[] ids, bool? boundToExternal)
        {
            if (ids == null)
                throw new ArgumentNullException(nameof(ids));
            if (ids.Length == 0)
                throw new ArgumentException("ids is empty");
            
            var version = ArticleVersionRepository.GetById(ids[0]);
            if (version == null)
                throw new Exception(string.Format(ArticleStrings.ArticleVersionNotFound, ids[0]));
            var article = version.Article;
            if (!article.IsUpdatable)
                return MessageResult.Error(ArticleStrings.CannotRemoveVersionsBecauseOfSecurity);
            if (!version.Article.IsArticleChangingActionsAllowed(boundToExternal))
                return MessageResult.Error(ContentStrings.ArticleChangingIsProhibited);
            foreach (var id in ids)
                article.RemoveVersionFolder(id);
            ArticleVersionRepository.MultipleDelete(ids);
            return null;
        }

        /// <summary>
        /// Восстанавливает версию статьи
        /// </summary>
        /// <param name="version"></param>
        /// <param name="boundToExternal"></param>
        /// <param name="disableNotifications"></param>
        /// <returns>восстановленная версия</returns>
        public static Article Restore(ArticleVersion version, bool? boundToExternal, bool disableNotifications)
        {
            if (!version.Article.IsArticleChangingActionsAllowed(boundToExternal))
                throw ActionNotAllowedException.CreateNotAllowedForArticleChangingActionException();
            var result = version.Article.Persist(disableNotifications);
            result.RestoreCurrentFiles(version.Id);
            return result;
        }
    }
}

