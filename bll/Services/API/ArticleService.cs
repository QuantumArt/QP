using System;
using System.Collections.Generic;
using System.Linq;
using Quantumart.QP8.BLL.Repository.ArticleRepositories;
using Quantumart.QP8.BLL.Repository.ContentRepositories;
using Quantumart.QP8.BLL.Repository.FieldRepositories;
using Quantumart.QP8.BLL.Services.API.Models;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL.Services.API
{
    public class ArticleService : ServiceBase, IBatchUpdateService
    {
        public ArticleService(int userId)
            : base(userId)
        {
        }

        public ArticleService(string connectionString, int userId)
            : base(connectionString, userId)
        {
        }

        public Article New(int contentId)
        {
            using (new QPConnectionScope(ConnectionString))
            {
                return Article.CreateNewForSave(contentId);
            }
        }

        public Article Read(int id, bool forceLoadFieldValues = false, bool excludeArchive = false)
        {
            using (new QPConnectionScope(ConnectionString))
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
            using (new QPConnectionScope(ConnectionString))
            {
                var content = ContentRepository.GetById(contentId);
                if (content == null)
                {
                    return null;
                }

                return content.VirtualType == 3 ? ArticleRepository.GetVirtualById(id, contentId) : Read(id, forceLoadFieldValues, excludeArchive);
            }
        }
        

        public IEnumerable<Article> List(int contentId, int[] ids, bool excludeArchive = false)
        {
            using (new QPConnectionScope(ConnectionString))
            {
                return ids == null ? ArticleRepository.GetList(contentId, excludeArchive) : ArticleRepository.GetList(ids, true, excludeArchive);
            }
        }

        public string GetRelatedItems(int fieldId, int? id, bool excludeArchive = false)
        {
            using (new QPConnectionScope(ConnectionString))
            {
                return ArticleRepository.GetRelatedItems(new [] {fieldId}, id, excludeArchive)[fieldId];
            }
        }

        public string GetLinkedItems(int linkId, int id, bool excludeArchive = false)
        {
            using (new QPConnectionScope(ConnectionString))
            {
                return ArticleRepository.GetLinkedItems(new [] {linkId}, id, excludeArchive)[linkId];
            }
        }

        public Dictionary<int, string> GetRelatedItemsMultiple(int fieldId, IEnumerable<int> ids, bool excludeArchive = false)
        {
            using (new QPConnectionScope(ConnectionString))
            {
                return ArticleRepository.GetRelatedItemsMultiple(fieldId, ids,excludeArchive);
            }
        }

        public Dictionary<int, string> GetLinkedItemsMultiple(int linkId, IEnumerable<int> ids, bool excludeArchive = false)
        {
            using (new QPConnectionScope(ConnectionString))
            {
                return ArticleRepository.GetLinkedItemsMultiple(linkId, ids, excludeArchive);
            }
        }

        public Article CopyAndRead(Article article)
        {
            using (new QPConnectionScope(ConnectionString))
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
            using (new QPConnectionScope(ConnectionString))
            {
                QPContext.CurrentUserId = TestedUserId;
                var result = ArticleServices.ArticleService.Copy(article.Id, true, false);
                QPContext.CurrentUserId = 0;
                return result;
            }
        }

        public Article Save(Article article, bool disableNotifications = false)
        {
            using (new QPConnectionScope(ConnectionString))
            {
                QPContext.CurrentUserId = TestedUserId;
                var result = article.Persist(disableNotifications);
                QPContext.CurrentUserId = 0;
                return result;
            }
        }

        public MessageResult Delete(int articleId)
        {
            using (new QPConnectionScope(ConnectionString))
            {
                QPContext.CurrentUserId = TestedUserId;
                var articleToRemove = ArticleRepository.GetById(articleId);
                if (articleToRemove == null)
                {
                    throw new ApplicationException(string.Format(ArticleStrings.ArticleNotFound, articleId));
                }

                var result = ArticleServices.ArticleService.Remove(articleToRemove.ContentId, articleId, false, true, false);

                QPContext.CurrentUserId = 0;
                return result;
            }
        }

        public MessageResult Delete(int contentId, int[] articleIds)
        {
            using (new QPConnectionScope(ConnectionString))
            {
                QPContext.CurrentUserId = TestedUserId;
                var result = ArticleServices.ArticleService.Remove(contentId, articleIds, false, true, false);
                QPContext.CurrentUserId = 0;
                return result;
            }
        }

        public void SimpleDelete(int[] articleIds)
        {
            using (new QPConnectionScope(ConnectionString))
            {
                QPContext.CurrentUserId = TestedUserId;
                ArticleRepository.MultipleDelete(articleIds, true, true);
                QPContext.CurrentUserId = 0;
            }
        }

        public MessageResult SetArchiveFlag(int contentId, int[] articleIds, bool flag)
        {
            using (new QPConnectionScope(ConnectionString))
            {
                QPContext.CurrentUserId = TestedUserId;
                var result = flag
                    ? ArticleServices.ArticleService.MoveToArchive(contentId, articleIds, true, false)
                    : ArticleServices.ArticleService.RestoreFromArchive(contentId, articleIds, true, false);

                QPContext.CurrentUserId = 0;
                return result;
            }
        }

        public void SimpleSetArchiveFlag(int[] articleIds, bool flag)
        {
            using (new QPConnectionScope(ConnectionString))
            {
                QPContext.CurrentUserId = TestedUserId;
                ArticleRepository.SetArchiveFlag(articleIds, flag, true);
                QPContext.CurrentUserId = 0;
            }
        }

        public MessageResult Publish(int contentId, int[] articleIds)
        {
            using (new QPConnectionScope(ConnectionString))
            {
                QPContext.CurrentUserId = TestedUserId;
                var result = ArticleServices.ArticleService.Publish(contentId, articleIds, true, false);
                QPContext.CurrentUserId = 0;
                return result;
            }
        }

        public void SimplePublish(int[] articleIds)
        {
            using (new QPConnectionScope(ConnectionString))
            {
                QPContext.CurrentUserId = TestedUserId;
                ArticleRepository.Publish(articleIds, true);
                QPContext.CurrentUserId = 0;
            }
        }

        public string[] GetFieldValues(int[] ids, int contentId, int fieldId)
        {
            using (new QPConnectionScope(ConnectionString))
            {
                return ArticleRepository.GetFieldValues(ids, contentId, fieldId);
            }
        }

        public string[] GetFieldValues(int[] ids, int contentId, string fieldName)
        {
            using (new QPConnectionScope(ConnectionString))
            {
                return ArticleRepository.GetFieldValues(ids, contentId, fieldName);
            }
        }

        public Dictionary<int, bool> CheckRelationSecurity(int contentId, int[] ids, bool isDeletable)
        {
            using (new QPConnectionScope(ConnectionString))
            {
                QPContext.CurrentUserId = TestedUserId;
                var result = ArticleRepository.CheckRelationSecurity(contentId, ids, isDeletable);
                QPContext.CurrentUserId = 0;
                return result;
            }
        }

        public InsertData[] BatchUpdate(IEnumerable<Article> articles)
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

            return BatchUpdate(articlesData, false);
        }

        public InsertData[] BatchUpdate(IEnumerable<ArticleData> articles) => BatchUpdate(articles, true);

        private InsertData[] BatchUpdate(IEnumerable<ArticleData> articles, bool formatArticleData)
        {
            using (new QPConnectionScope(ConnectionString))
            {
                QPContext.CurrentUserId = TestedUserId;
                var result = ArticleRepository.BatchUpdate(articles.ToArray(), formatArticleData);
                QPContext.CurrentUserId = 0;
                return result;
            }
        }

        public IList<int> GetParentIds(int id, int fieldId) => GetParentIds(new[] { id }, fieldId);

        public IList<int> GetParentIds(IList<int> ids, int fieldId)
        {
            using (new QPConnectionScope(ConnectionString))
            {
                var treeField = FieldRepository.GetById(fieldId);
                return ArticleRepository.GetParentIds(ids, treeField.Id, treeField.Name);
            }
        }

        public RulesException ValidateXamlById(int articleId, bool persistChanges = false) => ValidateXamlById(articleId, null, persistChanges);

        public RulesException ValidateXamlById(int articleId, string customerCode, bool persistChanges = false)
        {
            var errors = new RulesException();
            using (new QPConnectionScope(ConnectionString))
            {
                QPContext.CurrentUserId = TestedUserId;
                Article.ValidateXamlById(articleId, errors, customerCode, persistChanges);
                QPContext.CurrentUserId = 0;
            }
            return errors;
        }
    }
}
