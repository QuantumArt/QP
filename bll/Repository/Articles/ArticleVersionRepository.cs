using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.BLL.Mappers;
using Quantumart.QP8.DAL;
using System.Data.Objects;
using System.Data;
using Quantumart.QP8.BLL.Repository.Articles;


namespace Quantumart.QP8.BLL.Repository.Articles
{
    internal class ArticleVersionRepository
    {
		/// <summary>
		/// Возвращает список версий статей
		/// </summary>
		/// <param name="articleId">ID статьи</param>
		/// <param name="sortExpression">строка сортировки</param>
		/// <returns>список версий статей</returns>
        internal static List<ArticleVersion> GetList(int articleId, ListCommand command)
        {
            string eQuery = String.Format(@"select VALUE version from ArticleVersionSet as version where version.ArticleId = @id order by version.{0}", command.SortExpression);
            List<ArticleVersion> versionList = MappersRepository.ArticleVersionMapper.GetBizList(QPContext.EFContext.CreateQuery<ArticleVersionDAL>(eQuery, new ObjectParameter("id", articleId)).Include("CreatedByUser").ToList());
           
			return versionList;
        }


		/// <summary>
		/// Создает версию
		/// </summary>
		/// <param name="articleId">идентификатор статьи</param>
        internal static void Create(int articleId)
        {
            using (new QPConnectionScope())
            {
                Common.CreateArticleVersion(QPConnectionScope.Current.DbConnection, QPContext.CurrentUserId, articleId);
            }
        }


		/// <summary>
		/// Возвращает версию статьи
		/// </summary>
		/// <param name="id">ID версии</param>
        /// <param name="articleId">ID статьи (параметр необязательный, необходимо передавать только, если версия - текущая</param>
		/// <returns>информация о версии статьи</returns>
        internal static ArticleVersion GetById(int id, int articleId = 0)
        {
            ArticleVersion articleVersion = null;

            if (id == ArticleVersion.CurrentVersionId)
            {
				if (articleId == 0)
				{
					throw(new Exception("Article id is not specified!"));
				}

				Article article = ArticleRepository.GetById(articleId);
                articleVersion = new ArticleVersion { ArticleId = articleId, Id = id, Modified = article.Modified, LastModifiedBy = article.LastModifiedBy, LastModifiedByUser = article.LastModifiedByUser, Article = article };
            }
            else
            {
                ArticleVersionDAL articleVersionDal = DefaultRepository.GetById<ArticleVersionDAL>(id);
				if (articleVersionDal != null)
				{
					articleVersionDal.LastModifiedByUserReference.Load();

					articleVersion = MappersRepository.ArticleVersionMapper.GetBizObject(articleVersionDal);
					if (articleVersion != null)
					{
						articleVersion.Article = ArticleRepository.GetById(articleVersion.ArticleId);
					}
				}
            }

            return articleVersion;
        }


        /// <summary>
        /// Возвращает поля данных версии
        /// </summary>
        /// <param name="id">ID версии</param>
        /// <param name="articleId">ID статьи</param>
        /// <returns>DataRow с данныхми</returns>
        internal static DataRow GetData(int id, int articleId)
        {
            using (new QPConnectionScope())
            {
                return Common.GetArticleVersionRow(QPConnectionScope.Current.DbConnection, articleId, id);
            }
        }


		/// <summary>
		/// Удаляет версию статьи
		/// </summary>
		/// <param name="id">ID версии</param>
        internal static void Delete(int id)
        {
            DefaultRepository.Delete<ArticleVersionDAL>(id);
        }

		/// <summary>
		/// Удаляет версии статьи
		/// </summary>
		/// <param name="IDs">массив ID версий</param>
        internal static void MultipleDelete(int[] IDs)
        {
            DefaultRepository.Delete<ArticleVersionDAL>(IDs);
        }


        /// <summary>
        /// Получение связанных статей (для поля типа M2M)
        /// </summary>
        /// <param name="id">ID версии</param>
        /// <param name="fieldId">ID поля</param>
        /// <returns>строка связанных ID через запятую</returns>
        internal static string GetLinkedItems(int id, int fieldId)
        {
            using (new QPConnectionScope())
            {
                return Common.GetLinkedArticlesForVersion(QPConnectionScope.Current.DbConnection, fieldId, id);
            }
        }


        /// <summary>
        /// Восстанавливает версию
        /// </summary>
        /// <param name="id">ID версии</param>
        internal static void Restore(int id)
        {
            using (new QPConnectionScope())
            {
                Common.RestoreArticleVersion(QPConnectionScope.Current.DbConnection, QPContext.CurrentUserId, id);
            }
        }


        /// <summary>
        /// Возвращает самую свежую версию для статьи
        /// </summary>
        /// <param name="articleId">ID статьи</param>
        internal static ArticleVersion GetLatest(int articleId)
        {
            return GetById(QPContext.EFContext.ArticleVersionSet.Where(n => n.ArticleId == articleId).OrderByDescending(n => n.Id).Select(n => (int)n.Id).FirstOrDefault());
        }

        /// <summary>
        /// Возвращает самую старую версию для статьи
        /// </summary>
        /// <param name="articleId">ID статьи</param>
        internal static ArticleVersion GetEarliest(int articleId)
        {
            return GetById(QPContext.EFContext.ArticleVersionSet.Where(n => n.ArticleId == articleId).OrderBy(n => n.Id).Select(n => (int)n.Id).FirstOrDefault());
        }

        /// <summary>
        /// Возвращает общее число версий для статьи
        /// </summary>
        /// <param name="articleId">ID статьи</param>
        internal static int GetVersionsCount(int articleId)
        {
            return QPContext.EFContext.ArticleVersionSet.Count(n => n.ArticleId == articleId);
        }

        /// <summary>
        /// Возвращает список ID версий для статьи
        /// </summary>
        /// <param name="articleId">ID статьи</param>
        internal static IEnumerable<int> GetIds(int articleId)
        {
            return QPContext.EFContext.ArticleVersionSet.Where(n => n.ArticleId == articleId).Select(n => (int)n.Id).ToArray();
        }

        internal static IEnumerable<int> GetIds(int[] ids)
        {
            var decIds = ids.Select(n => (decimal)n).ToArray(); 
            return QPContext.EFContext.ArticleVersionSet.Where(n => decIds.Contains(n.ArticleId)).Select(n => (int)n.Id).ToArray();
        }

        /// <summary>
        /// Получение связанных статей (для поля типа M2O)
        /// </summary>
        /// <param name="id">ID версии</param>
        /// <param name="fieldId">ID поля</param>
        /// <returns>строка связанных ID через запятую</returns>
        internal static string GetRelatedItems(int versionId, int fieldId)
        {
            return GetLinkedItems(versionId, fieldId);
        }
    }
}
