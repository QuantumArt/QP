using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Text;
using Quantumart.QP8;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Utils;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Exceptions;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.Constants;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.BLL.Repository.Articles;
using Quantumart.QP8.Utils.FullTextSearch;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.ListItems;

namespace Quantumart.QP8.BLL.Services
{
    public class ArticleService
    {
        #region Private Members
        private static Article Read(int id, int contentId, bool withAutoLock)
        {
            var article = ArticleRepository.GetById(id);
            if (article == null)
            {
                throw new Exception(string.Format(ArticleStrings.ArticleNotFound, id));
            }

            article.DisplayContentId = contentId;
            article.DefineViewType();

            if (withAutoLock && article.ViewType == ArticleViewType.Normal)
            {
                article.AutoLock();
            }

            article.LoadLockedByUser();
            return article;
        }

        private static ArticleInitListResult InitList(int contentId, bool isArchive, bool? boundToExternal)
        {
            var result = new ArticleInitListResult();
            var content = ContentRepository.GetById(contentId);
            if (content == null)
            {
                throw new Exception(string.Format(ContentStrings.ContentNotFound, contentId));
            }

            result.IsVirtual = content.IsVirtual;
            result.PageSize = content.PageSize;
            result.ContentName = content.Name;
            result.IsUpdatable = content.IsUpdatable;

            var titleField = FieldRepository.GetTitleField(contentId);
            result.TitleFieldName = (titleField == null) ? FieldName.CONTENT_ITEM_ID : titleField.FormName;
            result.DisplayFields = FieldRepository.GetList(contentId, true);
            result.IsAddNewAccessable = !isArchive && SecurityRepository.IsActionAccessible(ActionCode.AddNewArticle) && SecurityRepository.IsEntityAccessible(EntityTypeCode.Content, contentId, ActionTypeCode.Update);
            result.IsArticleChangingActionsAllowed = content.IsArticleChangingActionsAllowed(boundToExternal);
            result.ContentDisableChangingActions = content.DisableChangingActions;

            return result;
        }

        private static MessageResult ConfirmHasChildren(int articleId, bool countArchived)
        {
            var count = ArticleRepository.CountChildren(articleId, countArchived);
            var format = (countArchived) ? ArticleStrings.WarningHasChildren : ArticleStrings.WarningHasNonArchiveChildren;
            var message = (count == 0) ? null : string.Format(format, count);
            return (string.IsNullOrEmpty(message)) ? null : MessageResult.Confirm(message);
        }

        private static MessageResult MultipleConfirmHasChildren(int[] IDs, bool countArchived)
        {
            var parentIds = IDs.Where(id => ArticleRepository.CountChildren(id, countArchived) != 0).ToList();
            var format = (countArchived) ? ArticleStrings.WarningHasChildrenMultiple : ArticleStrings.WarningHasNonArchiveChildrenMultiple;
            var message = (!parentIds.Any()) ? null : string.Format(format, string.Join(", ", parentIds.ToArray()));
            return (string.IsNullOrEmpty(message)) ? null : MessageResult.Confirm(message);
        }
        #endregion

        /// <summary>
        /// Инициализация списка статей
        /// </summary>
        /// <param name="contentId">ID контента</param>
        /// <returns>DTO</returns>
		public static ArticleInitListResult InitList(int contentId, bool? boundToExternal)
        {
            return InitList(contentId, false, boundToExternal);
        }

        /// <summary>
        /// Инициализация списка архивных статей
        /// </summary>
        /// <param name="contentId">ID контента</param>
        /// <returns>DTO</returns>
        public static ArticleInitListResult InitArchiveList(int contentId)
        {
            return InitList(contentId, true);
        }

        public static int Count(int contentId, string filter)
        {
            return ArticleRepository.GetCount(contentId);
        }

        /// <summary>
        /// Получение списка статей
        /// </summary>
        /// <param name="contentId">идентификатор контента</param>
        /// <param name="selectedArticleIDs">идентификаторы выбранных статей</param>
        /// <param name="cmd"></param>
        /// <param name="searchQueryParams"></param>
        /// <param name="contextQueryParams"></param>
        /// <param name="filter"></param>
        /// <param name="ftsParser"></param>
        /// <param name="onlyIds"></param>
        /// <param name="filterIds"></param>
        /// <returns>список статей</returns>
        public static ListResult<SimpleDataRow> List(int contentId, int[] selectedArticleIDs, ListCommand cmd, IList<ArticleSearchQueryParam> searchQueryParams, IList<ArticleContextQueryParam> contextQueryParams, string filter, ArticleFullTextSearchQueryParser ftsParser, bool? onlyIds = null, int[] filterIds = null)
        {
            int totalRecords;
            var dt = ArticleRepository.GetList(contentId, selectedArticleIDs, cmd, searchQueryParams, contextQueryParams, filter, ftsParser, onlyIds, filterIds, out totalRecords);
            var result = ArticleListHelper.GetResult(dt, FieldRepository.GetList(contentId, true), onlyIds).ToList();
            return new ListResult<SimpleDataRow> { Data = result, TotalRecords = totalRecords };
        }

        public static ListResult<ArticleListItem> ListLocked(ListCommand cmd)
        {
            int totalRecords;
            var dt = ArticleRepository.GetLockedList(cmd, out totalRecords);
            return new ListResult<ArticleListItem> { Data = dt, TotalRecords = totalRecords };
        }

        public static ListResult<ArticleListItem> ArticlesForApproval(ListCommand cmd)
        {
            int totalRecords;
            var dt = ArticleRepository.GetArticlesForApprovalList(cmd, out totalRecords);
            return new ListResult<ArticleListItem> { Data = dt, TotalRecords = totalRecords };
        }

        public static ListResult<StatusHistoryListItem> ArticleStatusHistory(ListCommand cmd, int articleId)
        {
            int totalRecords;
            var dt = ArticleRepository.GetStatusHistoryListItems(cmd, articleId, out totalRecords);
            return new ListResult<StatusHistoryListItem> { Data = dt, TotalRecords = totalRecords };
        }

        /// <summary>
        /// Возвращает упрощенный список статей
        /// </summary>
        /// <param name="contentId">идентификатор контента</param>
        /// <param name="articleId">идентификатор статьи, в котором используется данный список</param>
        /// <param name="fieldId">идентификатор поля, в котором выводится данный список</param>
        /// <param name="selectionMode">режим выделения списка</param>
        /// <param name="selectedArticleIDs">идентификаторы выбранных статей</param>
        /// <param name="filter"></param>
        /// <returns>упрощенный список статей</returns>
        public static List<ListItem> SimpleList(int contentId, int articleId, int fieldId, ListSelectionMode selectionMode, int[] selectedArticleIDs, string filter)
        {
            return ArticleRepository.GetSimpleList(contentId, articleId, fieldId, selectionMode, selectedArticleIDs, filter, 0);
        }

        /// <summary>
        /// Получения ветки дерева статей
        /// </summary>
        /// <param name="contentId">ID контента</param>
        /// <param name="isMultipleSelection"></param>
        /// <param name="boundToExternal"></param>
        /// <returns>DTO</returns>
        public static ArticleInitTreeResult InitTree(int contentId, bool isMultipleSelection, bool? boundToExternal)
        {

            var content = ContentRepository.GetById(contentId);
            if (content == null)
                throw new Exception(string.Format(ContentStrings.ContentNotFound, contentId));
            return new ArticleInitTreeResult(content, isMultipleSelection)
            {
                IsArticleChangingActionsAllowed = content.IsArticleChangingActionsAllowed(boundToExternal),
                ContentDisableChangingActions = content.DisableChangingActions
            };
        }

        /// <summary>
        /// Возвращает статью для просмотра и редактирования
        /// </summary>
        /// <param name="id">ID статьи</param>
        /// <param name="contentId">ID контента</param>
        /// <param name="backendActionCode"></param>
        /// <returns>статья</returns>
        public static Article Read(int id, int contentId, string backendActionCode)
        {
            bool withAutoLock;
            if (string.IsNullOrWhiteSpace(backendActionCode))
            {
                withAutoLock = true;
            }
            else if (backendActionCode != ActionCode.UpdateArticleAndUp && backendActionCode != ActionCode.SaveArticleAndUp)
            {
                withAutoLock = true;
            }
            else
            {
                withAutoLock = false;
            }

            var article = Read(id, contentId, withAutoLock);
            article.FixNonUsedStatus(false);

            return article;
        }


        /// <summary>
        /// Возвращает виртуальную статью для просмотра
        /// </summary>
        /// <param name="id">ID статьи</param>
        /// <param name="contentId">ID контента</param>
        /// <returns>статья</returns>
        public static Article ReadVirtual(int id, int contentId)
        {
            var result = ArticleRepository.GetVirtualById(id, contentId);
            if (result == null)
            {
                throw new Exception(string.Format(ArticleStrings.ArticleNotFoundInTheContent, id, contentId));
            }

            result.ViewType = ArticleViewType.Virtual;
            result.DisplayContentId = contentId;
            return result;
        }

        /// <summary>
        /// Возвращает архивную статью для просмотра
        /// </summary>
        /// <param name="id">ID статьи</param>
        /// <param name="contentId">ID контента</param>
        /// <returns>статья</returns>
        public static Article ReadArchive(int id, int contentId)
        {
            var result = Read(id, contentId, false);
            result.ViewType = ArticleViewType.Archived;
            return result;
        }


        /// <summary>
        /// Возвращает статью для обновления
        /// </summary>
        /// <param name="id">ID статьи</param>
        /// <param name="contentId">ID контента</param>
        /// <returns>статья</returns>
        public static Article ReadForUpdate(int id, int contentId)
        {
            return Read(id, contentId, false);
        }

        /// <summary>
        /// Генерирует пустую статью для показа
        /// </summary>
        /// <param name="contentId">идентификатор контента</param>
        /// <returns>пустая статья</returns>
		public static Article New(int contentId, int? fieldId, int? articleId, bool? isChild, bool? boundToExternal)
        {
            var article = Article.CreateNew(contentId, fieldId, articleId, isChild);
            if (!article.IsArticleChangingActionsAllowed(boundToExternal))
            {
                throw ActionNotAllowedException.CreateNotAllowedForArticleChangingActionException();
            }

            return article;
        }


        /// <summary>
        /// Генерирует пустую статью для сохранения
        /// </summary>
        /// <param name="contentId">идентификатор контента</param>
        /// <returns>пустая статья</returns>
        public static Article NewForSave(int contentId)
        {
            return Article.CreateNewForSave(contentId);
        }

        /// <summary>
        /// Копирует статью
        /// </summary>
        /// <param name="id">идентификатор статьи</param>
		public static CopyResult Copy(int id, bool? boundToExternal, bool disableNotifications)
        {
            var result = new CopyResult();
            var article = ArticleRepository.GetById(id);
            if (article == null)
            {
                throw new Exception(string.Format(ArticleStrings.ArticleNotFound, id));
            }

            if (article.IsAggregated)
            {
                return new CopyResult() { Message = MessageResult.Error(ArticleStrings.OperationIsNotAllowedForAggregated) };
            }

            if (!article.IsArticleChangingActionsAllowed(boundToExternal))
            {
                return new CopyResult() { Message = MessageResult.Error(ContentStrings.ArticleChangingIsProhibited) };
            }

            article.LoadFieldValues();
            if (!article.Content.IsUpdatable || !article.IsAccessible(ActionTypeCode.Read))
            {
                return new CopyResult() { Message = MessageResult.Error(ArticleStrings.CannotCopyBecauseOfSecurity) };
            }

            if (!article.IsUpdatableWithWorkflow)
            {
                return new CopyResult() { Message = MessageResult.Error(ArticleStrings.CannotAddBecauseOfWorkflow) };
            }

            if (!article.IsUpdatableWithRelationSecurity)
            {
                return new CopyResult() { Message = MessageResult.Error(ArticleStrings.CannotAddBecauseOfRelationSecurity) };
            }

            var previousAggregatedArticles = article.AggregatedArticles;
            article.ReplaceAllUrlsToPlaceHolders();

            try
            {
                article = ArticleRepository.Copy(article);
                result.Id = article.Id;
                article.CopyAggregates(previousAggregatedArticles);
                article.SendNotificationOneWay(NotificationCode.Create, disableNotifications);
            }
            catch (UnsupportedConstraintException)
            {
                result.Message = MessageResult.Error(ArticleStrings.UnsupportedConstraint);
            }

            return result;
        }

        /// <summary>
        /// Добавляет новую статью
        /// </summary>
        /// <param name="article">информация о статье</param>
        /// <returns>информация о статье</returns>
		public static Article Save(Article article, string backendActionCode, bool? boundToExternal, bool disableNotifications)
        {
            if (article == null)
            {
                throw new ArgumentNullException("article");
            }

            if (article.IsAggregated)
            {
                throw ActionNotAllowedException.CreateNotAllowedForAggregatedArticleException();
            }

            if (!article.IsArticleChangingActionsAllowed(boundToExternal))
            {
                throw ActionNotAllowedException.CreateNotAllowedForArticleChangingActionException();
            }

            return article.Persist(disableNotifications);
        }

        /// <summary>
        /// Обновляет информацию о статье
        /// </summary>
        /// <param name="article">информация о статье</param>
        /// <returns>информация о статье</returns>
		public static Article Update(Article article, string backendActionCode, bool? boundToExternal, bool disableNotifications)
        {

            if (article == null)
            {
                throw new ArgumentNullException("article");
            }

            if (article.IsAggregated)
            {
                throw ActionNotAllowedException.CreateNotAllowedForAggregatedArticleException();
            }

            if (!article.IsArticleChangingActionsAllowed(boundToExternal))
            {
                throw ActionNotAllowedException.CreateNotAllowedForArticleChangingActionException();
            }

            var result = article.Persist(disableNotifications);
            if (!string.IsNullOrWhiteSpace(backendActionCode) && backendActionCode.Equals(ActionCode.UpdateArticleAndUp, StringComparison.InvariantCultureIgnoreCase))
            {
                result.AutoUnlock();
            }

            return result;
        }

        #region Remove
        /// <summary>
        /// Удаляет статью
        /// </summary>
        /// <param name="id">идентификатор статьи</param>
        public static MessageResult Remove(int contentId, int id, bool fromArchive, bool? boundToExternal, bool disableNotifications)
        {
            if (ContentRepository.IsAnyAggregatedFields(contentId))
                return MessageResult.Error(ArticleStrings.OperationIsNotAllowedForAggregated);

            var articleToRemove = ArticleRepository.GetById(id);
            if (articleToRemove == null)
                throw new Exception(string.Format(ArticleStrings.ArticleNotFound, id));

            if (!articleToRemove.IsArticleChangingActionsAllowed(boundToExternal))
                return MessageResult.Error(ContentStrings.ArticleChangingIsProhibited);


            var content = ContentRepository.GetById(contentId);
            if (content == null)
                throw new Exception(string.Format(ContentStrings.ContentNotFound, id));

            if (articleToRemove.LockedByAnyoneElse)
                return MessageResult.Error(string.Format(ArticleStrings.LockedByAnyoneElse, articleToRemove.LockedByDisplayName));

            if (!articleToRemove.IsAccessible(ActionTypeCode.Remove))
                return MessageResult.Error(ArticleStrings.CannotRemoveBecauseOfSecurity);

            if (!articleToRemove.IsRemovableWithWorkflow)
                return MessageResult.Error(ArticleStrings.CannotRemoveBecauseOfWorkflow);

            if (!articleToRemove.IsRemovableWithRelationSecurity)
                return MessageResult.Error(ArticleStrings.CannotRemoveBecauseOfRelationSecurity);

            var idsToProceed = articleToRemove.SelfAndChildIds;

            if (content.AutoArchive && !fromArchive)
            {
                ArticleRepository.SetArchiveFlag(idsToProceed, true);
                articleToRemove.SendNotificationOneWay(NotificationCode.Update, disableNotifications);
            }
            else
            {
                articleToRemove.RemoveAllVersionFolders();
                articleToRemove.SendNotification(NotificationCode.Delete, disableNotifications);
                ArticleRepository.MultipleDelete(idsToProceed);
            }

            return null;
        }

        public static MessageResult RemoveInternal(int contentId, int[] IDs, bool fromArchive, bool? boundToExternal, bool disableNotifications)
        {
            if (ContentRepository.IsAnyAggregatedFields(contentId))
                return MessageResult.Error(ArticleStrings.OperationIsNotAllowedForAggregated);

            if (IDs == null)
                throw new ArgumentNullException("IDs");

            var content = ContentRepository.GetById(contentId);

            if (!content.IsArticleChangingActionsAllowed(boundToExternal))
                return MessageResult.Error(ContentStrings.ArticleChangingIsProhibited);


            if (content == null)
                throw new Exception(string.Format(ContentStrings.ContentNotFound, contentId));

            if (!content.AllowItemsPermission && !SecurityRepository.IsEntityAccessible(EntityTypeCode.Content, contentId, ActionTypeCode.Remove))
                return MessageResult.Error(ArticleStrings.CannotRemoveBecauseOfSecurity);

            var disableSecurityCheck = !content.AllowItemsPermission;
            var result = CheckIdResult<Article>.CreateForRemove(contentId, IDs, disableSecurityCheck);

            var idsToProceed = result.ValidItems.Cast<Article>().SelectMany(a => a.SelfAndChildIds).ToArray();

            if (content.AutoArchive && !fromArchive)
            {
                ArticleRepository.SetArchiveFlag(idsToProceed, true);

                foreach (Article item in result.ValidItems)
                {
                    item.SendNotificationOneWay(NotificationCode.Update, disableNotifications);
                }
            }
            else
            {
                foreach (Article item in result.ValidItems)
                {
                    item.RemoveAllVersionFolders();
                    item.SendNotification(NotificationCode.Delete, disableNotifications);
                }

                ArticleRepository.MultipleDelete(idsToProceed);
            }

            return result.GetServiceResult();
        }

        /// <summary>
        /// Удаляет статьи
        /// </summary>
        /// <param name="IDs">идентификаторы статей</param>
        public static MessageResult Remove(int contentId, int[] IDs, bool fromArchive, bool? boundToExternal, bool disableNotifications)
        {
            return RemoveInternal(contentId, IDs, fromArchive, boundToExternal, disableNotifications);
        }

        public static MessageResult MultistepRemove(int contentId, int[] IDs, bool fromArchive, bool? boundToExternal)
        {
            return RemoveInternal(contentId, IDs, fromArchive, boundToExternal, false);
        }
        #endregion

        /// <summary>
        /// Снимает блокировку со статьи
        /// </summary>
        /// <param name="id">идентификатор статьи</param>
        public static void Cancel(int id)
        {
            var article = ArticleRepository.GetById(id);
            if (article == null)
            {
                throw new Exception(string.Format(ArticleStrings.ArticleNotFound, id));
            }

            article.AutoUnlock();
        }

        public static void CaptureLock(int id)
        {
            var article = ArticleRepository.GetById(id);
            if (article == null)
            {
                throw new Exception(string.Format(ArticleStrings.ArticleNotFound, id));
            }

            if (article.CanBeUnlocked)
            {
                EntityObjectRepository.CaptureLock(article);
            }
        }

        #region Publish
        private static MessageResult PublishInternal(int contentId, int[] IDs, bool? boundToExternal, bool disableNotifications)
        {
            if (IDs == null)
                throw new ArgumentNullException("IDs");

            if (ContentRepository.IsAnyAggregatedFields(contentId))
                return MessageResult.Error(ArticleStrings.OperationIsNotAllowedForAggregated);

            var content = ContentRepository.GetById(contentId);
            if (!content.IsArticleChangingActionsAllowed(boundToExternal))
                return MessageResult.Error(ContentStrings.ArticleChangingIsProhibited);

            if (!content.AllowItemsPermission && !SecurityRepository.IsEntityAccessible(EntityTypeCode.Content, contentId, ActionTypeCode.Update))
                return MessageResult.Error(ArticleStrings.CannotUpdateBecauseOfSecurity);

            var disableSecurityCheck = !content.AllowItemsPermission;
            var result = CheckIdResult<Article>.CreateForPublish(contentId, IDs, disableSecurityCheck);
            var idsToProceed = result.ValidItems.Cast<Article>().SelectMany(a => a.SelfAndChildIds);

            ArticleRepository.Publish(idsToProceed);
            foreach (Article item in result.ValidItems)
            {
                item.SendNotificationOneWay(string.Format("{0};{1}", NotificationCode.Update, NotificationCode.ChangeStatus), disableNotifications);
            }
            return result.GetServiceResult();
        }

        public static MessageResult Publish(int contentId, int[] IDs, bool? boundToExternal, bool disableNotifications)
        {
            return PublishInternal(contentId, IDs, boundToExternal, disableNotifications);
        }

        public static MessageResult MultistepPublish(int contentId, int[] IDs, bool? boundToExternal)
        {
            return PublishInternal(contentId, IDs, boundToExternal, false);
        }
        #endregion

        #region Archive
        public static MessageResult MoveToArchive(int id, bool? boundToExternal, bool disableNotifications)
        {
            var article = ArticleRepository.GetById(id);
            if (article == null)
                throw new Exception(string.Format(ArticleStrings.ArticleNotFound, id));

            if (article.IsAggregated)
                return MessageResult.Error(ArticleStrings.OperationIsNotAllowedForAggregated);

            if (!article.IsArticleChangingActionsAllowed(boundToExternal))
                return MessageResult.Error(ContentStrings.ArticleChangingIsProhibited);

            if (article.LockedByAnyoneElse)
                return MessageResult.Error(string.Format(ArticleStrings.LockedByAnyoneElse, article.LockedByDisplayName));

            if (!article.IsAccessible(ActionTypeCode.Archive))
                return MessageResult.Error(ArticleStrings.CannotUpdateBecauseOfSecurity);

            if (!article.IsUpdatableWithWorkflow)
                return MessageResult.Error(ArticleStrings.CannotRemoveBecauseOfWorkflow);

            if (!article.IsUpdatableWithRelationSecurity)
                return MessageResult.Error(ArticleStrings.CannotUpdateBecauseOfRelationSecurity);

            var idsToProceed = article.SelfAndChildIds;

            ArticleRepository.SetArchiveFlag(idsToProceed, true);

            article.SendNotificationOneWay(NotificationCode.Update, disableNotifications);
            return null;
        }

        public static MessageResult MoveToArchiveInternal(int contentId, int[] IDs, bool? boundToExternal, bool disableNotifications)
        {
            if (IDs == null)
                throw new ArgumentNullException("IDs");

            if (ContentRepository.IsAnyAggregatedFields(contentId))
                return MessageResult.Error(ArticleStrings.OperationIsNotAllowedForAggregated);

            var content = ContentRepository.GetById(contentId);
            if (!content.IsArticleChangingActionsAllowed(boundToExternal))
                return MessageResult.Error(ContentStrings.ArticleChangingIsProhibited);

            if (!content.AllowItemsPermission && !SecurityRepository.IsEntityAccessible(EntityTypeCode.Content, contentId, ActionTypeCode.Update))
                return MessageResult.Error(ArticleStrings.CannotUpdateBecauseOfSecurity);

            var disableSecurityCheck = !content.AllowItemsPermission;
            var result = CheckIdResult<Article>.CreateForUpdate(contentId, IDs, disableSecurityCheck);
            var idsToProceed = result.ValidItems.Cast<Article>().SelectMany(a => a.SelfAndChildIds);

            ArticleRepository.SetArchiveFlag(idsToProceed, true);

            foreach (Article item in result.ValidItems)
            {
                item.SendNotificationOneWay(NotificationCode.Update, disableNotifications);
            }
            return result.GetServiceResult();
        }

        public static MessageResult MoveToArchive(int contentId, int[] IDs, bool? boundToExternal, bool disableNotifications)
        {
            return MoveToArchiveInternal(contentId, IDs, boundToExternal, disableNotifications);
        }

        public static MessageResult MultistepMoveToArchive(int contentId, int[] IDs, bool? boundToExternal)
        {
            return MoveToArchiveInternal(contentId, IDs, boundToExternal, false);
        }
        #endregion

        #region Restore
        public static MessageResult RestoreFromArchive(int id, bool? boundToExternal, bool disableNotifications)
        {
            var article = ArticleRepository.GetById(id);
            if (article == null)
                throw new Exception(string.Format(ArticleStrings.ArticleNotFound, id));

            if (article.IsAggregated)
                return MessageResult.Error(ArticleStrings.OperationIsNotAllowedForAggregated);

            if (!article.IsUpdatableWithRelationSecurity)
                return MessageResult.Error(ArticleStrings.CannotUpdateBecauseOfRelationSecurity);

            if (!SecurityRepository.IsEntityAccessible(EntityTypeCode.Article, id, ActionTypeCode.Restore))
                return MessageResult.Error(ArticleStrings.CannotUpdateBecauseOfSecurity);

            if (!article.IsArticleChangingActionsAllowed(boundToExternal))
                return MessageResult.Error(ContentStrings.ArticleChangingIsProhibited);

            var idsToProceed = article.SelfAndChildIds;

            ArticleRepository.SetArchiveFlag(idsToProceed, false);

            article.SendNotificationOneWay(NotificationCode.Update, disableNotifications);
            return null;
        }

        private static MessageResult RestoreFromArchiveInternal(int contentId, int[] IDs, bool? boundToExternal, bool disableNotifications)
        {
            if (IDs == null)
                throw new ArgumentNullException("IDs");

            if (ContentRepository.IsAnyAggregatedFields(contentId))
                return MessageResult.Error(ArticleStrings.OperationIsNotAllowedForAggregated);

            var content = ContentRepository.GetById(contentId);
            if (!content.IsArticleChangingActionsAllowed(boundToExternal))
                return MessageResult.Error(ContentStrings.ArticleChangingIsProhibited);

            if (!content.AllowItemsPermission && !SecurityRepository.IsEntityAccessible(EntityTypeCode.Content, contentId, ActionTypeCode.Update))
                return MessageResult.Error(ArticleStrings.CannotUpdateBecauseOfSecurity);

            var disableSecurityCheck = !content.AllowItemsPermission;
            var result = CheckIdResult<Article>.CreateForUpdate(contentId, IDs, disableSecurityCheck);

            var idsToProceed = result.ValidItems.Cast<Article>().SelectMany(a => a.SelfAndChildIds);

            ArticleRepository.SetArchiveFlag(idsToProceed, false);
            foreach (Article item in result.ValidItems)
            {
                item.SendNotificationOneWay(NotificationCode.Update, disableNotifications);
            }
            return result.GetServiceResult();
        }

        public static MessageResult RestoreFromArchive(int contentId, int[] IDs, bool? boundToExternal, bool disableNotifications)
        {
            return RestoreFromArchiveInternal(contentId, IDs, boundToExternal, disableNotifications);
        }

        public static MessageResult MultistepRestoreFromArchive(int contentId, int[] IDs, bool? boundToExternal)
        {
            return RestoreFromArchiveInternal(contentId, IDs, boundToExternal, false);
        }
        #endregion

        #region Remove preaction
        public static MessageResult RemovePreAction(int parentId, int id)
        {
            return ConfirmHasChildren(id, true);
        }

        public static MessageResult MultipleRemovePreAction(int parentId, int[] IDs)
        {
            return MultipleConfirmHasChildren(IDs, true);
        }

        public static MessageResult MultistepRemovePreAction(int parentId, int[] IDs)
        {
            return MultipleConfirmHasChildren(IDs, true);
        }
        #endregion

        #region Archive preaction
        public static MessageResult MoveToArchivePreAction(int id)
        {
            return ConfirmHasChildren(id, false);
        }

        public static MessageResult MultipleMoveToArchivePreAction(int[] IDs)
        {
            return MultipleConfirmHasChildren(IDs, false);
        }

        public static MessageResult MultistepMoveToArchivePreAction(int[] IDs)
        {
            return MultipleConfirmHasChildren(IDs, false);
        }
        #endregion

        /// <summary>
        /// Возвращает список контентов для классификатора
        /// </summary>
        public static IEnumerable<ListItem> GetAggregetableContentsForClassifier(Field classifier, string excludeValue)
        {
            return FieldRepository.GetAggregatableContentListItemsForClassifier(classifier, excludeValue);
        }

        /// <summary>
        /// Возврщает агрегированную статью
        /// </summary>
        /// <param name="id"></param>
        /// <param name="parentId"></param>
        /// <param name="aggregatedContentId"></param>
        /// <returns></returns>
        public static Article GetAggregatedArticle(int rootArticleId, int rootContentId, int aggregatedContentId)
        {
            if (aggregatedContentId > 0)
            {
                Article rootArticle = null;
                if (rootArticleId == 0)
                {
                    rootArticle = Article.CreateNew(rootContentId);
                }
                else
                {
                    rootArticle = Read(rootArticleId, rootContentId, false);
                }

                return rootArticle.GetAggregatedArticleByClassifier(aggregatedContentId);
            }
            else
            {
                return null;
            }
        }

        public static IEnumerable<ArticleContextQueryParam> GetContextQuery(int contentId, string contextString)
        {
            var parsed = contextString.Split(",".ToCharArray()).Select(n => int.Parse(n)).ToDictionary(n => ArticleRepository.GetById(n).ContentId, n => n);
            return ContentRepository.GetById(contentId).GetContextSearchBlockItems().Select(n => new ArticleContextQueryParam
            {
                Name = "content_" + n.ContentId,
                Value = parsed.ContainsKey(n.ContentId) ? parsed[n.ContentId].ToString() : string.Empty,
                FieldId = n.FieldId
            });
        }

        public static void UnlockArticles(int[] IDs)
        {
            ArticleRepository.UnlockArticlesByUser(IDs);
        }

        public static List<ListItem> GetListOfFieldsForImport(int contentId)
        {
            return FieldRepository.GetList(contentId, false).Where(n => n.ExactType != FieldExactTypes.M2ORelation).Select(f => new ListItem { Text = f.Name, Value = f.Id.ToString() }).ToList();
        }

        public static List<ListItem> GetListOfFieldsToSort(int contentId)
        {
            var list = new List<ListItem>();
            list.Add(new ListItem { Text = ArticleStrings.ID, Value = ArticleStrings.ID });
            list.AddRange(FieldRepository.GetList(contentId, false).Where(f => (!f.IsBlob && !f.IsClassifier && f.RelatedToContent == null)).Select(f => new ListItem { Text = f.Name, Value = f.Name }).ToList());
            return list;
        }

        public static Dictionary<string, List<string>> GetM2MValuesBatch(IEnumerable<int> ids, int linkId, string displayFieldName, int contentId)
        {
            return ArticleRepository.GetM2MValuesBatch(ids, linkId, displayFieldName, contentId);
        }

        public static Dictionary<string, List<string>> GetM2OValuesBatch(IEnumerable<int> ids, int contentId, int fieldId, string fieldName, string displayFieldName)
        {
            return ArticleRepository.GetM2OValuesBatch(ids, contentId, fieldId, fieldName, displayFieldName);
        }

        public static string GetTitleName(int contentId)
        {
            return ContentRepository.GetTitleName(contentId);
        }

        public static IList<int> GetParentIds(int id, int fieldId)
        {
            return ArticleRepository.GetParentIds(id, fieldId);
        }

        public static IList<int> GetParentIds(IList<int> ids, int fieldId)
        {
            return ArticleRepository.GetParentIds(ids, fieldId);
        }

        public static IList<KeyValuePair<int, string>> GetChildArticles(IList<int> ids, int fieldId, string filter)
        {
            var treeField = FieldRepository.GetById(fieldId);
            return ArticleRepository.GetChildArticles(ids, treeField.Name, treeField.ContentId, filter);
        }
    }
}
