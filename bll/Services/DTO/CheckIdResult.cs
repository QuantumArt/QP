using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Resources;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Repository.ArticleRepositories;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL.Services.DTO
{
    public class CheckIdResult<T>
        where T : EntityObject
    {
        #region private

        /// <summary>
        /// Статьи, успешно прошедшие проверку
        /// </summary>
        private readonly List<EntityObject> _ValidItems = new List<EntityObject>();

        /// <summary>
        /// Статьи, заблокированные другими пользователями
        /// </summary>
        private readonly List<EntityObject> _LockedItems = new List<EntityObject>();

        /// <summary>
        /// ID не найденных статей
        /// </summary>
        private readonly List<int> _NotFoundIds = new List<int>();

        /// <summary>
        /// ID статей, недоступных для заданной операции
        /// </summary>
        private readonly List<int> _NotAccessedIds = new List<int>();

        /// <summary>
        /// ID статей, недоступных из-за workflow
        /// </summary>
        private readonly List<int> _BlockedByWorkflowIds = new List<int>();

        /// <summary>
        /// ID статей, недоступных из-за relation security
        /// </summary>
        private readonly List<int> _BlockedByRelationSecurityIds = new List<int>();

        /// <summary>
        /// ID статей, для которых не нужна операция
        /// </summary>
        private readonly List<int> _RedundantIds = new List<int>();

        private ResourceManager GetResourceManager()
        {
            if (typeof(T) == typeof(Article))
            {
                return new ResourceManager("Quantumart.QP8.Resources.ArticleStrings", typeof(ArticleStrings).Assembly);
            }

            if (typeof(T) == typeof(Site))
            {
                return new ResourceManager("Quantumart.QP8.Resources.SiteStrings", typeof(SiteStrings).Assembly);
            }

            if (typeof(T) == typeof(Field))
            {
                return new ResourceManager("Quantumart.QP8.Resources.FieldStrings", typeof(SiteStrings).Assembly);
            }

            throw new Exception("Resource type is not supported");
        }

        private string ErrorMessage(string resourceKeyString, int[] ids)
        {
            var format = GetResourceManager().GetString(resourceKeyString, CultureInfo.CurrentUICulture);
            return string.Format(format, string.Join(", ", ids));
        }

        private int[] LockedIds
        {
            get { return _LockedItems.Select(n => n.Id).ToArray(); }
        }

        #endregion

        public static CheckIdResult<Article> CreateForPublish(int contentId, int[] ids, bool disableSecurityCheck)
        {
            var result = new CheckIdResult<Article>();
            var list = EntityObjectRepository.GetList(EntityTypeCode.Article, ids).Cast<Article>();
            if (list == null)
            {
                result._NotFoundIds.AddRange(ids);
            }
            else
            {
                result._NotFoundIds.AddRange(ids.Except(list.Select(n => n.Id)));
                var checkResult = ArticleRepository.CheckSecurity(contentId, ids, false, disableSecurityCheck);
                var relationCheckResult = ArticleRepository.CheckRelationSecurity(contentId, ids, false);
                foreach (var article in list)
                {
                    if (article.StatusTypeId == article.Workflow.MaxStatus.Id && !article.Splitted)
                    {
                        result._RedundantIds.Add(article.Id);
                    }
                    else if (article.LockedByAnyoneElse && !QPContext.CanUnlockItems)
                    {
                        result._LockedItems.Add(article);
                    }
                    else if (!checkResult[article.Id])
                    {
                        result._NotAccessedIds.Add(article.Id);
                    }
                    else if (!article.IsPublishableWithWorkflow)
                    {
                        result._BlockedByWorkflowIds.Add(article.Id);
                    }
                    else if (!relationCheckResult[article.Id])
                    {
                        result._BlockedByRelationSecurityIds.Add(article.Id);
                    }
                    else
                    {
                        result._ValidItems.Add(article);
                    }
                }
            }

            return result;
        }

        public static CheckIdResult<Article> CreateForRemove(int contentId, int[] ids, bool disableSecurityCheck)
        {
            var result = new CheckIdResult<Article>();
            var list = EntityObjectRepository.GetList(EntityTypeCode.Article, ids).Cast<Article>();
            if (list == null)
            {
                result._NotFoundIds.AddRange(ids);
            }
            else
            {
                result._NotFoundIds.AddRange(ids.Except(list.Select(n => n.Id)));
                var checkResult = ArticleRepository.CheckSecurity(contentId, ids, true, disableSecurityCheck);
                var relationCheckResult = ArticleRepository.CheckRelationSecurity(contentId, ids, true);

                foreach (var article in list)
                {
                    if (article.LockedByAnyoneElse && !QPContext.CanUnlockItems)
                    {
                        result._LockedItems.Add(article);
                    }
                    else if (!checkResult[article.Id])
                    {
                        result._NotAccessedIds.Add(article.Id);
                    }
                    else if (!article.IsRemovableWithWorkflow)
                    {
                        result._BlockedByWorkflowIds.Add(article.Id);
                    }
                    else if (!relationCheckResult[article.Id])
                    {
                        result._BlockedByRelationSecurityIds.Add(article.Id);
                    }
                    else
                    {
                        result._ValidItems.Add(article);
                    }
                }
            }

            return result;
        }

        public static CheckIdResult<Article> CreateForUpdate(int contentId, int[] ids, bool disableSecurityCheck)
        {
            var result = new CheckIdResult<Article>();
            var list = EntityObjectRepository.GetList(EntityTypeCode.Article, ids).Cast<Article>();
            if (list == null)
            {
                result._NotFoundIds.AddRange(ids);
            }
            else
            {
                result._NotFoundIds.AddRange(ids.Except(list.Select(n => n.Id)));

                var checkResult = ArticleRepository.CheckSecurity(contentId, ids, false, disableSecurityCheck);
                var relationCheckResult = ArticleRepository.CheckRelationSecurity(contentId, ids, false);

                foreach (var article in list)
                {
                    if (article.LockedByAnyoneElse && !QPContext.CanUnlockItems)
                    {
                        result._LockedItems.Add(article);
                    }
                    else if (!checkResult[article.Id])
                    {
                        result._NotAccessedIds.Add(article.Id);
                    }
                    else if (!article.IsUpdatableWithWorkflow)
                    {
                        result._BlockedByWorkflowIds.Add(article.Id);
                    }
                    else if (!relationCheckResult[article.Id])
                    {
                        result._BlockedByRelationSecurityIds.Add(article.Id);
                    }
                    else
                    {
                        result._ValidItems.Add(article);
                    }
                }
            }

            return result;
        }

        public static CheckIdResult<T> Create(int[] ids, string actionTypeCode)
        {
            var result = new CheckIdResult<T>();
            foreach (var id in ids)
            {
                var item = EntityObjectRepository.GetById<T>(id);
                var lockableItem = item as LockableEntityObject;
                if (item == null)
                {
                    result._NotFoundIds.Add(id);
                }
                else if (item != null && !SecurityRepository.IsEntityAccessible(item.EntityTypeCode, id, actionTypeCode))
                {
                    result._NotAccessedIds.Add(id);
                }
                else if (lockableItem != null && lockableItem.LockedByAnyoneElse && !QPContext.CanUnlockItems)
                {
                    result._LockedItems.Add(item);
                }
                else
                {
                    result._ValidItems.Add(item);
                }
            }

            return result;
        }

        public MessageResult GetServiceResult()
        {
            var strings = new List<string>();

            if (_NotFoundIds.Any())
            {
                strings.Add(ErrorMessage("SomeoneNotFound", _NotFoundIds.ToArray()));
            }
            if (_RedundantIds.Any())
            {
                strings.Add(ErrorMessage("SomeoneRedundant", _RedundantIds.ToArray()));
            }
            if (_LockedItems.Any())
            {
                strings.Add(ErrorMessage("SomeoneLocked", LockedIds));
            }
            if (_NotAccessedIds.Any())
            {
                strings.Add(ErrorMessage("SomeoneNotAccessed", _NotAccessedIds.ToArray()));
            }
            if (_BlockedByWorkflowIds.Any())
            {
                strings.Add(ErrorMessage("SomeoneBlockedByWorkflow", _BlockedByWorkflowIds.ToArray()));
            }
            if (_BlockedByRelationSecurityIds.Any())
            {
                strings.Add(ErrorMessage("SomeoneBlockedByRelationSecurity", _BlockedByRelationSecurityIds.ToArray()));
            }

            var failedIds = _NotFoundIds.Union(LockedIds).Union(_NotAccessedIds).Union(_BlockedByWorkflowIds).Union(_BlockedByRelationSecurityIds);
            return !strings.Any() ? null : MessageResult.Info(string.Join(".\n", strings), failedIds.ToArray());
        }

        public int[] ValidIds
        {
            get { return _ValidItems.Select(n => n.Id).ToArray(); }
        }

        public EntityObject[] ValidItems => _ValidItems.ToArray();
    }
}
