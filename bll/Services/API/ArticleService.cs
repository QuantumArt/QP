using System;
using System.Collections.Generic;
using System.Linq;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Repository.ArticleRepositories;
using Quantumart.QP8.BLL.Repository.ContentRepositories;
using Quantumart.QP8.BLL.Repository.FieldRepositories;
using Quantumart.QP8.BLL.Services.API.Models;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Configuration;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL.Services.API
{
    public class ArticleService : ServiceBase, IBatchUpdateService
    {
        private readonly IBackendActionLogRepository _logRepo = new AuditRepository();

        public ArticleService(int userId)
            : base(userId)
        {
        }

        public ArticleService(string connectionString, int userId)
            : base(connectionString, userId)
        {
        }

        public ArticleService(QpConnectionInfo info, int userId)
            : base(info, userId)
        {
        }

        public Article New(int contentId)
        {
            using (new QPConnectionScope(ConnectionInfo))
            {
                return Article.CreateNewForSave(contentId);
            }
        }

        public S3Options S3Options { get; set; } = new();

        public Article Read(int id, bool forceLoadFieldValues = false, bool excludeArchive = false)
        {
            using (new QPConnectionScope(ConnectionInfo))
            {
                var article = ArticleRepository.GetById(id);
                if (article == null || excludeArchive && article.Archived)
                {
                    return null;
                }

                if (forceLoadFieldValues)
                {
                    article.LoadFieldValues(excludeArchive);
                    article.LoadAggregatedArticles(excludeArchive);
                }

                return article;
            }
        }

        public Article Read(int id, int contentId, bool forceLoadFieldValues = false, bool excludeArchive = false)
        {
            using (new QPConnectionScope(ConnectionInfo))
            {
                var content = ContentRepository.GetById(contentId);
                if (content == null)
                {
                    return null;
                }

                return content.VirtualType == 3 ? ArticleRepository.GetVirtualById(id, contentId) : Read(id, forceLoadFieldValues, excludeArchive);
            }
        }


        public IEnumerable<Article> List(int contentId, int[] ids, bool excludeArchive = false, string filter = "")
        {
            using (new QPConnectionScope(ConnectionInfo))
            {
                return ArticleRepository.GetList(ids, true, excludeArchive, contentId, filter);
            }
        }

        public IEnumerable<int> Ids(int contentId, int[] ids, bool excludeArchive = false, string filter = "")
        {
            using (new QPConnectionScope(ConnectionInfo))
            {
                return ArticleRepository.GetIds(ids, excludeArchive, contentId, filter);
            }
        }

        public string GetRelatedItems(int fieldId, int? id, bool excludeArchive = false)
        {
            using (new QPConnectionScope(ConnectionInfo))
            {
                return ArticleRepository.GetRelatedItems(new [] {fieldId}, id, excludeArchive)[fieldId];
            }
        }

        public Dictionary<int, string> GetRelatedItemsMultiple(int fieldId, int[] ids, bool excludeArchive = false)
        {
            using (new QPConnectionScope(ConnectionInfo))
            {
                return ArticleRepository.GetRelatedItemsMultiple(fieldId, ids, excludeArchive);
            }
        }

        public Dictionary<int, Dictionary<int, List<int>>> GetRelatedItemsMultiple(int[] fieldIds, int[] ids, bool excludeArchive = false)
        {
            using (new QPConnectionScope(ConnectionInfo))
            {
                return ArticleRepository.GetRelatedItemsMultiple(fieldIds, ids, excludeArchive);
            }
        }


        public string GetLinkedItems(int linkId, int id, bool excludeArchive = false)
        {
            using (new QPConnectionScope(ConnectionInfo))
            {
                return ArticleRepository.GetLinkedItems(new [] {linkId}, id, excludeArchive)[linkId];
            }
        }

        public Dictionary<int, string> GetLinkedItemsMultiple(int linkId, int[] ids, bool excludeArchive = false)
        {
            using (new QPConnectionScope(ConnectionInfo))
            {
                return ArticleRepository.GetLinkedItemsMultiple(linkId, ids, excludeArchive);
            }
        }

        public Dictionary<int, Dictionary<int, List<int>>> GetLinkedItemsMultiple(int[] linkIds, int[] ids, bool excludeArchive = false)
        {
            using (new QPConnectionScope(ConnectionInfo))
            {
                return ArticleRepository.GetLinkedItemsMultiple(linkIds, ids, excludeArchive);
            }
        }


        public Article CopyAndRead(Article article)
        {
            using (new QPConnectionScope(ConnectionInfo))
            {
                var result = Copy(article);
                if (result.Message.Type == ActionMessageType.Error)
                {
                    throw new ApplicationException(result.Message.Text);
                }

                return Read(result.Id);
            }
        }

        public CopyResult Copy(Article article)
        {
            using (new QPConnectionScope(ConnectionInfo))
            {
                QPContext.CurrentUserId = TestedUserId;
                var pathHelper = new PathHelper(new DbServices.DbService(S3Options));
                var service = new ArticleServices.ArticleService(pathHelper);
                var result = service.Copy(article.Id, true, false);
                BackendActionContext.CreateLogs(ActionCode.CreateLikeArticle, new[] { article.Id }, article.ContentId, _logRepo, true);
                QPContext.CurrentUserId = 0;
                return result;
            }
        }

        public Article Save(Article article, bool disableNotifications = false)
        {
            using (new QPConnectionScope(ConnectionInfo))
            {
                QPContext.CurrentUserId = TestedUserId;
                var pathHelper = new PathHelper(new DbServices.DbService(S3Options));
                article.PathHelper = pathHelper;
                var result = article.Persist(disableNotifications);
                var code = article.Id == 0 ? ActionCode.SaveArticle : ActionCode.UpdateArticle;
                BackendActionContext.CreateLogs(code, new[] { result.Id }, result.ContentId, _logRepo, true);
                QPContext.CurrentUserId = 0;
                return result;
            }
        }

        public MessageResult Delete(int articleId)
        {
            using (new QPConnectionScope(ConnectionInfo))
            {
                QPContext.CurrentUserId = TestedUserId;
                var articleToRemove = ArticleRepository.GetById(articleId);
                if (articleToRemove == null)
                {
                    throw new ApplicationException(string.Format(ArticleStrings.ArticleNotFound, articleId));
                }
                var pathHelper = new PathHelper(new DbServices.DbService(S3Options));
                var service = new ArticleServices.ArticleService(pathHelper);
                var contentId = articleToRemove.ContentId;
                var result = service.Remove(contentId, articleId, false, true, false);
                BackendActionContext.CreateLogs(ActionCode.RemoveArticle, new[] { articleId }, contentId, _logRepo, true);
                QPContext.CurrentUserId = 0;
                return result;
            }
        }

        public MessageResult Delete(int contentId, int[] articleIds)
        {
            using (new QPConnectionScope(ConnectionInfo))
            {
                QPContext.CurrentUserId = TestedUserId;
                var pathHelper = new PathHelper(new DbServices.DbService(S3Options));
                var service = new ArticleServices.ArticleService(pathHelper);
                var result = service.Remove(contentId, articleIds, false, true, false);
                BackendActionContext.CreateLogs(ActionCode.MultipleRemoveArticle, articleIds, contentId, _logRepo, true);
                QPContext.CurrentUserId = 0;
                return result;
            }
        }

        public void SimpleDelete(int[] articleIds)
        {
            using (new QPConnectionScope(ConnectionInfo))
            {
                QPContext.CurrentUserId = TestedUserId;
                ArticleRepository.MultipleDelete(articleIds, true, true);
                QPContext.CurrentUserId = 0;
            }
        }

        public MessageResult SetArchiveFlag(int contentId, int[] articleIds, bool flag)
        {
            using (new QPConnectionScope(ConnectionInfo))
            {
                QPContext.CurrentUserId = TestedUserId;
                var pathHelper = new PathHelper(new DbServices.DbService(S3Options));
                var service = new ArticleServices.ArticleService(pathHelper);
                var result = flag
                    ? service.MoveToArchive(contentId, articleIds, true, false)
                    : service.RestoreFromArchive(contentId, articleIds, true, false);
                var code = flag ? ActionCode.MultipleMoveArticleToArchive : ActionCode.MultipleRestoreArticleFromArchive;
                BackendActionContext.CreateLogs(code, articleIds, contentId, _logRepo, true);
                QPContext.CurrentUserId = 0;
                return result;
            }
        }

        public void SimpleSetArchiveFlag(int[] articleIds, bool flag)
        {
            using (new QPConnectionScope(ConnectionInfo))
            {
                QPContext.CurrentUserId = TestedUserId;
                ArticleRepository.SetArchiveFlag(articleIds, flag, true);
                QPContext.CurrentUserId = 0;
            }
        }

        public MessageResult Publish(int contentId, int[] articleIds)
        {
            using (new QPConnectionScope(ConnectionInfo))
            {
                QPContext.CurrentUserId = TestedUserId;
                var pathHelper = new PathHelper(new DbServices.DbService(S3Options));
                var service = new ArticleServices.ArticleService(pathHelper);
                var result = service.Publish(contentId, articleIds, true, false);
                BackendActionContext.CreateLogs(ActionCode.MultiplePublishArticles, articleIds, contentId, _logRepo, true);
                QPContext.CurrentUserId = 0;
                return result;
            }
        }

        public void SimplePublish(int[] articleIds)
        {
            using (new QPConnectionScope(ConnectionInfo))
            {
                QPContext.CurrentUserId = TestedUserId;
                ArticleRepository.Publish(articleIds, true);
                QPContext.CurrentUserId = 0;
            }
        }

        public string[] GetFieldValues(int[] ids, int contentId, int fieldId)
        {
            using (new QPConnectionScope(ConnectionInfo))
            {
                return ArticleRepository.GetFieldValues(ids, contentId, fieldId);
            }
        }

        public string[] GetFieldValues(int[] ids, int contentId, string fieldName)
        {
            using (new QPConnectionScope(ConnectionInfo))
            {
                return ArticleRepository.GetFieldValues(ids, contentId, fieldName);
            }
        }

        public Dictionary<int, bool> CheckRelationSecurity(int contentId, int[] ids, bool isDeletable)
        {
            using (new QPConnectionScope(ConnectionInfo))
            {
                QPContext.CurrentUserId = TestedUserId;
                var result = ArticleRepository.CheckRelationSecurity(contentId, ids, isDeletable);
                QPContext.CurrentUserId = 0;
                return result;
            }
        }

        public InsertData[] BatchUpdate(IEnumerable<Article> articles, bool createVersions = false)
        {
            var articlesData = articles.Select(article => new ArticleData
            {
                Id = article.Id,
                ContentId = article.ContentId,
                Fields = article.FieldValues.Select(fv => new FieldData
                {
                    Id = fv.Field.Id,
                    Value = fv.Field.TypeInfo.FormatFieldValue(fv.Value),
                    ArticleIds = fv.RelatedItems
                }).ToList()
            });

            return BatchUpdate(new BatchUpdateModel
            {
                Articles = articlesData.ToArray(),
                CheckSecurity = false,
                CreateVersions = createVersions
            }).InsertData;
        }

        public InsertData[] BatchUpdate(IEnumerable<ArticleData> articlesData, bool createVersions = false)
        {
            return BatchUpdate(new BatchUpdateModel
            {
                Articles = articlesData.ToArray(),
                FormatArticleData = true,
                CheckSecurity = false,
                CreateVersions = createVersions
            }).InsertData;
        }

        public BatchUpdateResult BatchUpdate(BatchUpdateModel model)
        {
            using (new QPConnectionScope(ConnectionInfo))
            {
                QPContext.CurrentUserId = TestedUserId;
                if (model.CheckSecurity && !CheckBatchUpdateModelSecurity(model, out var checkResult))
                {
                    return checkResult;
                }
                model.PathHelper ??= new PathHelper(new DbServices.DbService(S3Options));
                var result = ArticleRepository.BatchUpdate(model);
                CreateLogs(model.Articles, result.InsertData);
                QPContext.CurrentUserId = 0;
                return result;
            }
        }

        private static bool CheckBatchUpdateModelSecurity(BatchUpdateModel model, out BatchUpdateResult result)
        {
            var contentIds = model.Articles.Select(n => n.ContentId).Distinct().ToArray();
            foreach (var contentId in contentIds)
            {
                var content = ContentRepository.GetById(contentId);
                if (content == null)
                {
                    result = BatchUpdateResult.Error(string.Format(ContentStrings.ContentNotFound, contentId));
                    return false;
                }
                var contentForCheck = content.BaseAggregationContent ?? content;
                if (!contentForCheck.IsArticleChangingActionsAllowed(false))
                {
                    result = BatchUpdateResult.Error(ContentStrings.ArticleChangingIsProhibited + $" (Id = {contentId})");
                    return false;
                }

                if (!contentForCheck.AllowItemsPermission && !SecurityRepository.IsEntityAccessible(EntityTypeCode.Content, contentId, ActionTypeCode.Update))
                {
                    result = BatchUpdateResult.Error(ContentStrings.CannotUpdateBecauseOfSecurity + $" (Id = {contentId})");
                    return false;
                }
                var disableSecurityCheck = !content.AllowItemsPermission;
                var ids = model.Articles.Where(n => n.ContentId == contentId).Select(n => n.Id).ToArray();
                var checkResult = CheckIdResult<Article>.CreateForUpdate(contentId, ids, disableSecurityCheck).GetServiceResult();
                if (checkResult is { FailedIds: not null } && checkResult.FailedIds.Any())
                {
                    result = BatchUpdateResult.Error(checkResult.Text, checkResult.FailedIds);
                    return false;
                }
            }

            result = null;
            return true;
        }

        private void CreateLogs(IEnumerable<ArticleData> articles, InsertData[] result)
        {
            var originalIds = new HashSet<int>(result.Select(n => n.OriginalArticleId).ToArray());
            var insertedIds = result.GroupBy(n => n.ContentId)
                .ToDictionary(
                    n => n.Key,
                    m => m.Select(e => e.CreatedArticleId).ToArray()
                );
            var updatedIds = articles.Where(n => !originalIds.Contains(n.Id))
                .GroupBy(n => n.ContentId)
                .ToDictionary(
                    n => n.Key,
                    m => m.Select(e => e.Id).ToArray()
                );
            foreach (var key in insertedIds.Keys)
            {
                BackendActionContext.CreateLogs(ActionCode.MultipleSaveArticles, insertedIds[key], key, _logRepo, true);
            }
            foreach (var key in updatedIds.Keys)
            {
                BackendActionContext.CreateLogs(ActionCode.MultipleUpdateArticles, updatedIds[key], key, _logRepo, true);
            }
        }

        public IList<int> GetParentIds(int id, int fieldId) => GetParentIds(new[] { id }, fieldId);

        public IList<int> GetParentIds(IList<int> ids, int fieldId)
        {
            using (new QPConnectionScope(ConnectionInfo))
            {
                var treeField = FieldRepository.GetById(fieldId);
                return ArticleRepository.GetParentIds(ids, treeField.Id, treeField.Name);
            }
        }

        public RulesException ValidateXamlById(int articleId, bool persistChanges = false) => ValidateXamlById(articleId, null, persistChanges);

        public RulesException ValidateXamlById(int articleId, string customerCode, bool persistChanges = false)
        {
            var errors = new RulesException();
            using (new QPConnectionScope(ConnectionInfo))
            {
                QPContext.CurrentUserId = TestedUserId;
                Article.ValidateXamlById(articleId, errors, customerCode, persistChanges);
                QPContext.CurrentUserId = 0;
            }
            return errors;
        }
    }
}
