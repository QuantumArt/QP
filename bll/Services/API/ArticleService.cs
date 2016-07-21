using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Repository.Articles;
using Quantumart.QP8.BLL.Services.API.Models;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Quantumart.QP8.BLL.Services.API
{
    public class ArticleService : ServiceBase
    {
        public ArticleService(string connectionString, int userId) : base(connectionString, userId)
        { }

        public ArticleService(int userId) : base(userId)
        {

        }

        public Article New(int contentId)
        {
            using (new QPConnectionScope(ConnectionString))
            {
                return Article.CreateNewForSave(contentId);
            }
        }

        public Article Read(int id, bool forceLoadFieldValues = false)
        {
            using (new QPConnectionScope(ConnectionString))
            {
                var article = ArticleRepository.GetById(id);
                if (article != null && forceLoadFieldValues)
                {
                    article.LoadFieldValues();
                    article.LoadAggregatedArticles();
                }

                return article;
            }
        }

        public Article Read(int id, int contentId, bool forceLoadFieldValues = false)
        {
            using (new QPConnectionScope(ConnectionString))
            {
                var content = ContentRepository.GetById(contentId);
                if (content == null)
                {
                    return null;
                }

                if (content.VirtualType == 3)
                {
                    return ArticleRepository.GetVirtualById(id, contentId);
                }
                else
                {
                    return Read(id, forceLoadFieldValues);
                }
            }
        }

        public IEnumerable<Article> List(int contentId, int[] ids)
        {
            using (new QPConnectionScope(ConnectionString))
            {
                if (ids == null)
                {
                    return ArticleRepository.GetList(contentId);
                }
                else
                {
                    return ArticleRepository.GetList(ids, true);
                }
            }
        }

        public string GetRelatedItems(int fieldId, int? id)
        {
            using (new QPConnectionScope(ConnectionString))
            {
                return ArticleRepository.GetRelatedItems(fieldId, id);
            }
        }

        public string GetLinkedItems(int linkId, int id)
        {
            using (new QPConnectionScope(ConnectionString))
            {
                return ArticleRepository.GetLinkedItems(linkId, id);
            }
        }

        public Dictionary<int, string> GetRelatedItemsMultiple(int fieldId, IEnumerable<int> ids)
        {
            using (new QPConnectionScope(ConnectionString))
            {
                return ArticleRepository.GetRelatedItemsMultiple(fieldId, ids);
            }
        }

        public Dictionary<int, string> GetLinkedItemsMultiple(int linkId, IEnumerable<int> ids)
        {
            using (new QPConnectionScope(ConnectionString))
            {
                return ArticleRepository.GetLinkedItemsMultiple(linkId, ids);
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
                var result = BLL.Services.ArticleService.Copy(article.Id, true, false);
                QPContext.CurrentUserId = 0;
                return result;
            }
        }

        public Article Save(Article article, bool disableNotifications = false)
        {
            using (new QPConnectionScope(ConnectionString))
            {
                QPContext.CurrentUserId = TestedUserId;
                Article result = article.Persist(disableNotifications);
                QPContext.CurrentUserId = 0;
                return result;
            }
        }

        public MessageResult Delete(int articleId)
        {
            using (new QPConnectionScope(ConnectionString))
            {
                MessageResult result = null;
                QPContext.CurrentUserId = TestedUserId;
                Article articleToRemove = ArticleRepository.GetById(articleId);
                if (articleToRemove == null)
                {
                    throw new ApplicationException(string.Format(ArticleStrings.ArticleNotFound, articleId));
                }
                else
                {
                    result = Services.ArticleService.Remove(articleToRemove.ContentId, articleId, false, true, false);
                }

                QPContext.CurrentUserId = 0;
                return result;
            }
        }

        public MessageResult Delete(int contentId, int[] articleIds)
        {
            using (new QPConnectionScope(ConnectionString))
            {
                QPContext.CurrentUserId = TestedUserId;
                MessageResult result = BLL.Services.ArticleService.Remove(contentId, articleIds, false, true, false);
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
                MessageResult result;
                if (flag)
                {
                    result = Services.ArticleService.MoveToArchive(contentId, articleIds, true, false);
                }
                else
                {
                    result = Services.ArticleService.RestoreFromArchive(contentId, articleIds, true, false);
                }

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
                MessageResult result = BLL.Services.ArticleService.Publish(contentId, articleIds, true, false);
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

        #region BatchUpdate
        public InsertData[] BatchUpdate(IEnumerable<ArticleData> articles)
        {
            return BatchUpdate(articles, true);
        }

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
        #endregion

        public IList<int> GetParentIds(int id, int fieldId)
        {
            return GetParentIds(new int[] { id }, fieldId);
        }

        public IList<int> GetParentIds(IList<int> ids, int fieldId)
        {
            using (new QPConnectionScope(ConnectionString))
            {
                var treeField = FieldRepository.GetById(fieldId);
                return ArticleRepository.GetParentIds(ids, treeField.Id, treeField.Name);
            }
        }
    }
}
