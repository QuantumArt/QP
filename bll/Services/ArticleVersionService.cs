using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Text;
using Quantumart.QP8;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Utils;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.BLL.Repository.Articles;
using Quantumart.QP8.Resources;
using Quantumart.QP8.BLL.Exceptions;

namespace Quantumart.QP8.BLL.Services
{
    public class ArticleVersionService
    {

        #region Private Members
        private static void Exchange(ref int id1, ref int id2)
        {
            int temp = id1;
            id1 = id2;
            id2 = temp;
        }

        private static Tuple<int, int> GetOrderedIds(int[] ids)
        {
            int id1 = ids[0];
            int id2 = ids[1];

            if (id1 > id2)
                Exchange(ref id1, ref id2);

            if (id1 == ArticleVersion.CurrentVersionId)
                Exchange(ref id1, ref id2);

            return Tuple.Create<int, int>(id1, id2);
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
			ArticleVersion result = ArticleVersionRepository.GetById(id, articleId);
			if (result == null)
				throw new Exception(String.Format(ArticleStrings.ArticleVersionNotFoundForArticle, id, articleId));
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
                ArticleVersion result = ArticleVersionRepository.GetById(id, 0);
				if (result == null)
					throw new Exception(String.Format(ArticleStrings.ArticleVersionNotFound, id));
                return result.PathInfo;
            }
            else
            {
                Field field = FieldRepository.GetById(fieldId);
				if (field == null)
					throw new Exception(String.Format(FieldStrings.FieldNotFound, fieldId));
                return field.Content.GetVersionPathInfo(ArticleVersion.CurrentVersionId);
            }
        }


		/// <summary>
		/// Возвращает список версий статей
		/// </summary>
		/// <param name="articleId">идентификатор статьи</param>
		/// <param name="sortExpression">настройки сортировки</param>
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
				throw new ArgumentNullException("ids");
			if (ids.Length != 2)
				throw new ArgumentException("Wrong ids length");
			
			Tuple<int, int> result = GetOrderedIds(ids);
            ArticleVersion version1 = ArticleVersionRepository.GetById(result.Item1, parentId);
			if (version1 == null)
				throw new Exception(String.Format(ArticleStrings.ArticleVersionNotFoundForArticle, result.Item1, parentId));

            ArticleVersion version2;
            if (result.Item2 == ArticleVersion.CurrentVersionId)
            {
                Article parent = ArticleRepository.GetById(parentId);
				if (parent == null)
					throw new Exception(String.Format(ArticleStrings.ArticleNotFound, parentId));
                version2 = new ArticleVersion { ArticleId = parent.Id, Article = parent, Id = ArticleVersion.CurrentVersionId, Modified = parent.Modified, LastModifiedBy = parent.LastModifiedBy, LastModifiedByUser = parent.LastModifiedByUser };
            }
            else
            {
                version2 = ArticleVersionRepository.GetById(result.Item2, parentId);
				if (version2 == null)
					throw new Exception(String.Format(ArticleStrings.ArticleVersionNotFoundForArticle, result.Item2, parentId));
            }
            version1.MergeToVersion(version2);
            version1.Article.ViewType = ArticleViewType.CompareVersions;
            return version1;
        }


		/// <summary>
		/// Удаляет версию статьи
		/// </summary>
        /// <param name="id">ID версии</param>
		public static MessageResult Remove(int id, bool? boundToExternal)
        {
			ArticleVersion version = ArticleVersionRepository.GetById(id);
			if (version == null)
				throw new Exception(String.Format(ArticleStrings.ArticleVersionNotFound, id));

			if (!version.Article.IsUpdatable)
				return MessageResult.Error(ArticleStrings.CannotRemoveVersionsBecauseOfSecurity);
			else if (!version.Article.IsArticleChangingActionsAllowed(boundToExternal))
				return MessageResult.Error(ContentStrings.ArticleChangingIsProhibited);
			else
			{
				version.Article.RemoveVersionFolder(id);
				ArticleVersionRepository.Delete(id);
				return null;
			}

        }

		/// <summary>
		/// Удаляет версии статьи
		/// </summary>
        /// <param name="IDs">массив ID версий</param>
		public static MessageResult MultipleRemove(int[] IDs, bool? boundToExternal)
        {
			if (IDs == null)
				throw new ArgumentNullException("IDs");
			if (IDs.Length == 0)
				throw new ArgumentException("IDs is empty");
			
			ArticleVersion version = ArticleVersionRepository.GetById(IDs[0]);
			if (version == null)
				throw new Exception(String.Format(ArticleStrings.ArticleVersionNotFound, IDs[0]));
			Article article = version.Article;
			if (!article.IsUpdatable)
				return MessageResult.Error(ArticleStrings.CannotRemoveVersionsBecauseOfSecurity);
			else if (!version.Article.IsArticleChangingActionsAllowed(boundToExternal))
				return MessageResult.Error(ContentStrings.ArticleChangingIsProhibited);
			else
			{
				foreach (int id in IDs)
					article.RemoveVersionFolder(id);
				ArticleVersionRepository.MultipleDelete(IDs);
				return null;
			}
        }

        /// <summary>
        /// Восстанавливает версию статьи
        /// </summary>
        /// <param name="id">ID версии</param>
        /// <returns>восстановленная версия</returns>
		public static Article Restore(ArticleVersion version, bool? boundToExternal, bool disableNotifications)
        {
			if (!version.Article.IsArticleChangingActionsAllowed(boundToExternal))
				throw ActionNotAllowedException.CreateNotAllowedForArticleChangingActionException();
			Article result = version.Article.Persist(disableNotifications);
			result.RestoreCurrentFiles(version.Id);
			return result;
        }
    }
}

