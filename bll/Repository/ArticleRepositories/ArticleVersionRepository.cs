using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Dynamic;
using Microsoft.EntityFrameworkCore;
using Quantumart.QP8.BLL.Facades;
using Quantumart.QP8.DAL;
using Quantumart.QP8.DAL.Entities;

namespace Quantumart.QP8.BLL.Repository.ArticleRepositories
{
    internal class ArticleVersionRepository
    {
        /// <summary>
        /// Возвращает список версий статей
        /// </summary>
        /// <param name="articleId">ID статьи</param>
        /// <param name="command"></param>
        /// <returns>список версий статей</returns>
        internal static List<ArticleVersion> GetList(int articleId, ListCommand command)
        {


            var result = QPContext.EFContext
                .ArticleVersionSet
                .Where(x => x.ArticleId == articleId)
                .Include(x => x.LastModifiedByUser)
                .Include(x => x.CreatedByUser)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(command.SortExpression))
            {
                result = result.OrderBy(command.SortExpression);
            }


            return MapperFacade.ArticleVersionMapper.GetBizList(result.ToList());
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

        internal static void Create(int[] articleIds)
        {
            using (new QPConnectionScope())
            {
                Common.CreateArticleVersions(QPConnectionScope.Current.DbConnection, QPContext.CurrentUserId, articleIds);
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
            ArticleVersion articleVersion;

            if (id == ArticleVersion.CurrentVersionId)
            {
                if (articleId == 0)
                {
                    throw new Exception("Article id is not specified!");
                }

                var article = ArticleRepository.GetById(articleId);
                articleVersion = new ArticleVersion
                {
                    ArticleId = articleId,
                    Id = id,
                    Modified = article.Modified,
                    LastModifiedBy = article.LastModifiedBy,
                    LastModifiedByUser = article.LastModifiedByUser,
                    Article = article
                };
            }
            else
            {
                var articleVersionDal = QPContext.EFContext.ArticleVersionSet.Include(n => n.LastModifiedByUser).SingleOrDefault(n => n.Id == id);
                articleVersion = MapperFacade.ArticleVersionMapper.GetBizObject(articleVersionDal);
                if (articleVersion != null)
                {
                    articleVersion.Article = ArticleRepository.GetById(articleVersion.ArticleId);
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
                return Common.GetArticleVersionRow(QPContext.EFContext, QPConnectionScope.Current.DbConnection, articleId, id);
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
        /// <param name="ids">массив ID версий</param>
        internal static void MultipleDelete(int[] ids)
        {
            DefaultRepository.Delete<ArticleVersionDAL>(ids);
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
        /// <param name="versionId"></param>
        /// <param name="fieldId">ID поля</param>
        /// <returns>строка связанных ID через запятую</returns>
        internal static string GetRelatedItems(int versionId, int fieldId) => GetLinkedItems(versionId, fieldId);
    }
}
