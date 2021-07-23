using System;
using System.Collections.Generic;
using Quantumart.QP8.BLL.Exceptions;
using Quantumart.QP8.BLL.Repository.ArticleRepositories;
using Quantumart.QP8.BLL.Repository.FieldRepositories;
using Quantumart.QP8.BLL.Services.ArticleServices;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL.Services
{
    public class ArticleVersionService
    {
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

            if (id1 > id2 && id2 != ArticleVersion.CurrentVersionId)
            {
                Exchange(ref id1, ref id2);
            }

            return Tuple.Create(id1, id2);
        }

        public static ArticleVersion Read(int id, int articleId = 0)
        {
            var result = ArticleVersionRepository.GetById(id, articleId);
            if (result == null)
            {
                throw new Exception(string.Format(ArticleStrings.ArticleVersionNotFoundForArticle, id, articleId));
            }

            result.Article.ViewType = ArticleViewType.PreviewVersion;
            return result;
        }

        public static PathInfo GetPathInfo(int fieldId, int id)
        {
            if (id != ArticleVersion.CurrentVersionId)
            {
                var result = ArticleVersionRepository.GetById(id);
                if (result == null)
                {
                    throw new Exception(string.Format(ArticleStrings.ArticleVersionNotFound, id));
                }

                return result.PathInfo;
            }

            var field = FieldRepository.GetById(fieldId);
            if (field == null)
            {
                throw new Exception(string.Format(FieldStrings.FieldNotFound, fieldId));
            }

            return field.Content.GetVersionPathInfo(ArticleVersion.CurrentVersionId);
        }

        public static List<ArticleVersion> List(int articleId, ListCommand command)
        {
            command.SortExpression = ArticleVersion.TranslateSortExpression(command.SortExpression);
            return ArticleVersionRepository.GetList(articleId, command);
        }

        public static ArticleVersion GetMergedVersion(int[] ids, int parentId)
        {
            if (ids == null)
            {
                throw new ArgumentNullException(nameof(ids));
            }
            if (ids.Length != 2)
            {
                throw new ArgumentException("Wrong ids length");
            }

            var parent = ArticleRepository.GetById(parentId);
            if (parent == null)
            {
                throw new Exception(string.Format(ArticleStrings.ArticleNotFound, parentId));
            }

            var (item1, item2) = GetOrderedIds(ids);
            ArticleVersion version1, version2;

            if (item1 == ArticleVersion.LiveVersionId)
            {
                var liveVersion = ArticleService.ReadLive(parentId, parent.ContentId);
                version1 = new ArticleVersion
                {
                    Id = ArticleVersion.LiveVersionId,
                    Article = liveVersion,
                    ArticleId = liveVersion.Id,
                    Modified = liveVersion.Modified,
                    LastModifiedBy = liveVersion.LastModifiedBy,
                    LastModifiedByUser = liveVersion.LastModifiedByUser,
                    StatusTypeId = liveVersion.StatusTypeId
                };
            }
            else
            {
                version1 = ArticleVersionRepository.GetById(item1, parentId);
                if (version1 == null)
                {
                    throw new Exception(string.Format(ArticleStrings.ArticleVersionNotFoundForArticle, item1, parentId));
                }
            }

            if (item2 == ArticleVersion.CurrentVersionId)
            {
                version2 = new ArticleVersion
                {
                    Id = ArticleVersion.CurrentVersionId,
                    Article = parent,
                    ArticleId = parent.Id,
                    Modified = parent.Modified,
                    LastModifiedBy = parent.LastModifiedBy,
                    LastModifiedByUser = parent.LastModifiedByUser,
                    StatusTypeId = parent.StatusTypeId
                };
            }
            else
            {
                version2 = ArticleVersionRepository.GetById(item2, parentId);
                if (version2 == null)
                {
                    throw new Exception(string.Format(ArticleStrings.ArticleVersionNotFoundForArticle, item2, parentId));
                }
            }

            version1.MergeToVersion(version2);
            version1.Article.ViewType = ArticleViewType.CompareVersions;
            version1.AggregatedArticles.ForEach(x => { x.ViewType = ArticleViewType.CompareVersions; });
            return version1;
        }

        public static MessageResult Remove(int id, bool? boundToExternal)
        {
            var version = ArticleVersionRepository.GetById(id);
            if (version == null)
            {
                throw new Exception(string.Format(ArticleStrings.ArticleVersionNotFound, id));
            }

            if (!version.Article.IsUpdatable)
            {
                return MessageResult.Error(ArticleStrings.CannotRemoveVersionsBecauseOfSecurity);
            }
            if (!version.Article.IsArticleChangingActionsAllowed(boundToExternal))
            {
                return MessageResult.Error(ContentStrings.ArticleChangingIsProhibited);
            }

            version.Article.RemoveVersionFolder(id);
            ArticleVersionRepository.Delete(id);
            return null;
        }

        public static MessageResult MultipleRemove(int[] ids, bool? boundToExternal)
        {
            if (ids == null)
            {
                throw new ArgumentNullException(nameof(ids));
            }

            if (ids.Length == 0)
            {
                throw new ArgumentException("ids is empty");
            }

            var version = ArticleVersionRepository.GetById(ids[0]);
            if (version == null)
            {
                throw new Exception(string.Format(ArticleStrings.ArticleVersionNotFound, ids[0]));
            }

            var article = version.Article;
            if (!article.IsUpdatable)
            {
                return MessageResult.Error(ArticleStrings.CannotRemoveVersionsBecauseOfSecurity);
            }

            if (!version.Article.IsArticleChangingActionsAllowed(boundToExternal))
            {
                return MessageResult.Error(ContentStrings.ArticleChangingIsProhibited);
            }

            foreach (var id in ids)
            {
                article.RemoveVersionFolder(id);
            }

            ArticleVersionRepository.MultipleDelete(ids);
            return null;
        }

        public static Article Restore(ArticleVersion version, bool? boundToExternal, bool disableNotifications)
        {
            if (!version.Article.IsArticleChangingActionsAllowed(boundToExternal))
            {
                throw new ActionNotAllowedException(ContentStrings.ArticleChangingIsProhibited);
            }

            var result = version.Article.Persist(disableNotifications);
            result.RestoreArticleFilesForVersion(version.Id);
            return result;
        }
    }
}
