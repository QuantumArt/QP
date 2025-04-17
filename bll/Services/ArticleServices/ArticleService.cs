using System;
using System.Collections.Generic;
using System.Linq;
using QP8.Infrastructure;
using Quantumart.QP8.BLL.Exceptions;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Repository.ArticleRepositories;
using Quantumart.QP8.BLL.Repository.ArticleRepositories.SearchParsers;
using Quantumart.QP8.BLL.Repository.ContentRepositories;
using Quantumart.QP8.BLL.Repository.FieldRepositories;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.BLL.Services.ArticleServices
{
    public class ArticleService : IArticleService
    {
        private readonly IArticleRepository _articleRepository;
        private readonly PathHelper _pathHelper;

        public ArticleService(IArticleRepository articleRepository, PathHelper pathHelper)
        {
            _articleRepository = articleRepository;
            _pathHelper = pathHelper;
        }

        internal ArticleService(PathHelper pathHelper)
        {
            _pathHelper = pathHelper;
        }

        public int GetArticleIdByGuid(string rawGuid)
        {
            if (!Guid.TryParse(rawGuid, out var guid))
            {
                throw new Exception($"Неверный формат GUID: {rawGuid}");
            }

            return GetArticleIdByGuid(guid);
        }

        public int GetArticleIdByGuid(Guid guid)
        {
            var articleId = GetArticleIdByGuidOrDefault(guid);
            if (articleId == 0)
            {
                throw new Exception($"Не найдена статья с заданным Id: {guid}");
            }

            return articleId;
        }

        public int GetArticleIdByGuidOrDefault(Guid guid) => _articleRepository.GetIdByGuid(guid);

        public Guid GetArticleGuidById(string rawId)
        {
            if (!int.TryParse(rawId, out var id))
            {
                throw new Exception($"Неверный формат Id: {rawId}");
            }

            return GetArticleGuidById(id);
        }

        public Guid GetArticleGuidById(int id)
        {
            var articleGuid = GetArticleGuidByIdOrDefault(id);
            return articleGuid ?? Guid.Empty;
        }

        public Guid[] GetArticleGuidsByIds(int[] ids) => _articleRepository.GetGuidsByIds(ids);

        public int[] GetArticleIdsByGuids(Guid[] ids) => _articleRepository.GetIdsByGuids(ids);

        public Guid? GetArticleGuidByIdOrDefault(int id) => _articleRepository.GetById(id)?.UniqueId;

        private static Article Read(int id, int contentId, bool withAutoLock)
        {
            var article = ArticleRepository.GetById(id);
            if (article == null)
            {
                throw new Exception(string.Format(ArticleStrings.ArticleNotFound, id));
            }

            article.DisplayContentId = contentId;
            article.DefineViewType();

            if (withAutoLock && article.ViewType == ArticleViewType.Normal && !article.IsAggregated)
            {
                article.AutoLock();
            }

            article.LoadCollaborativePublishedArticle(article.Id);
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
            result.TitleFieldName = titleField == null ? FieldName.ContentItemId : titleField.FormName;
            result.DisplayFields = FieldRepository.GetList(contentId, true);
            result.IsAddNewAccessable = !isArchive && SecurityRepository.IsActionAccessible(ActionCode.AddNewArticle) && SecurityRepository.IsEntityAccessible(EntityTypeCode.Content, contentId, ActionTypeCode.Update);
            result.IsArticleChangingActionsAllowed = content.IsArticleChangingActionsAllowed(boundToExternal);
            result.ContentDisableChangingActions = content.DisableChangingActions;

            return result;
        }

        private static MessageResult ConfirmHasChildren(int articleId, bool countArchived)
        {
            var count = ArticleRepository.CountChildren(articleId, countArchived);
            var format = countArchived ? ArticleStrings.WarningHasChildren : ArticleStrings.WarningHasNonArchiveChildren;
            var message = count == 0 ? null : string.Format(format, count);
            return string.IsNullOrEmpty(message) ? null : MessageResult.Confirm(message);
        }

        private static MessageResult MultipleConfirmHasChildren(IEnumerable<int> ids, bool countArchived)
        {
            var parentIds = ids.Where(id => ArticleRepository.CountChildren(id, countArchived) != 0).ToList();
            var format = countArchived ? ArticleStrings.WarningHasChildrenMultiple : ArticleStrings.WarningHasNonArchiveChildrenMultiple;
            var message = !parentIds.Any() ? null : string.Format(format, string.Join(", ", parentIds.ToArray()));
            return string.IsNullOrEmpty(message) ? null : MessageResult.Confirm(message);
        }

        public static ArticleInitListResult InitList(int contentId, bool? boundToExternal) => InitList(contentId, false, boundToExternal);

        public static ArticleInitListResult InitArchiveList(int contentId) => InitList(contentId, true);

        public static int Count(int contentId) => ArticleRepository.GetCount(contentId);
        public static int Count(int contentId, bool includeArchive) => ArticleRepository.GetCount(contentId, includeArchive);

        public static bool IsCountOverflow(int contentId, bool includeArchive, int overflowLimit) => ArticleRepository.IsCountOverflow(contentId, includeArchive, overflowLimit);

        public static ListResult<SimpleDataRow> List(int contentId, int[] selectedArticleIDs, ListCommand cmd, IList<ArticleSearchQueryParam> searchQueryParams, IList<ArticleContextQueryParam> contextQueryParams, CustomFilterItem[] filter, ArticleFullTextSearchQueryParser ftsParser, bool? onlyIds = null, int[] filterIds = null)
        {
            var dt = ArticleRepository.GetList(contentId, selectedArticleIDs, cmd, searchQueryParams, contextQueryParams, filter, ftsParser, onlyIds, filterIds ?? new int[] { }, out var totalRecords).ToList();
            var result = ArticleListHelper.GetResult(dt, FieldRepository.GetList(contentId, true), onlyIds).ToList();
            return new ListResult<SimpleDataRow> { Data = result, TotalRecords = totalRecords };
        }

        public static ListResult<ArticleListItem> ListLocked(ListCommand cmd)
        {
            var dt = ArticleRepository.GetLockedList(cmd, out var totalRecords);
            return new ListResult<ArticleListItem> { Data = dt, TotalRecords = totalRecords };
        }

        public static ListResult<ArticleListItem> ArticlesForApproval(ListCommand cmd)
        {
            var dt = ArticleRepository.GetArticlesForApprovalList(cmd, out var totalRecords);
            return new ListResult<ArticleListItem> { Data = dt, TotalRecords = totalRecords };
        }

        public static ListResult<StatusHistoryListItem> ArticleStatusHistory(ListCommand cmd, int articleId)
        {
            var dt = ArticleRepository.GetStatusHistoryListItems(cmd, articleId, out var totalRecords);
            return new ListResult<StatusHistoryListItem> { Data = dt, TotalRecords = totalRecords };
        }

        public static List<ListItem> SimpleList(SimpleListQuery query) => ArticleRepository.GetSimpleList(query);

        public static ArticleInitTreeResult InitTree(int contentId, bool isMultipleSelection, bool? boundToExternal)
        {
            var content = ContentRepository.GetById(contentId);
            if (content == null)
            {
                throw new Exception(string.Format(ContentStrings.ContentNotFound, contentId));
            }

            return new ArticleInitTreeResult(content, isMultipleSelection)
            {
                IsArticleChangingActionsAllowed = content.IsArticleChangingActionsAllowed(boundToExternal),
                ContentDisableChangingActions = content.DisableChangingActions
            };
        }

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

        public static Article ReadLive(int id, int contentId)
        {
            var article = Read(id, contentId, false);
            article.ViewType = ArticleViewType.ViewLive;
            article.LastModifiedByUser = article.LiveLastModifiedBy;
            article.LastModifiedBy = article.LastModifiedByUser?.Id ?? 0;
            article.Modified = article.LiveModified;
            article.StatusTypeId = article.LiveStatusTypeId;
            return article;
        }

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

        public static Article ReadArchive(int id, int contentId)
        {
            var result = Read(id, contentId, false);
            result.ViewType = ArticleViewType.Archived;
            return result;
        }

        public static Article ReadForUpdate(int id, int contentId) => Read(id, contentId, false);

        public CopyResult Copy(int id, bool? boundToExternal, bool disableNotifications) => Copy(ArticleRepository.GetById(id), boundToExternal, disableNotifications, null);

        public CopyResult Copy(Article article, bool? boundToExternal, bool disableNotifications, Guid? guidForSubstitution)
        {
            var result = new CopyResult();
            Ensure.NotNull(article, string.Format(ArticleStrings.ArticleNotFound, article.Id));

            if (article.IsAggregated)
            {
                return new CopyResult { Message = MessageResult.Error(ArticleStrings.OperationIsNotAllowedForAggregated) };
            }

            if (!article.IsArticleChangingActionsAllowed(boundToExternal))
            {
                return new CopyResult { Message = MessageResult.Error(ContentStrings.ArticleChangingIsProhibited) };
            }

            article.LoadFieldValues();
            if (!article.Content.IsUpdatable || !article.IsAccessible(ActionTypeCode.Read))
            {
                return new CopyResult { Message = MessageResult.Error(ArticleStrings.CannotCopyBecauseOfSecurity) };
            }

            if (!article.IsUpdatableWithWorkflow)
            {
                return new CopyResult { Message = MessageResult.Error(ArticleStrings.CannotAddBecauseOfWorkflow) };
            }

            if (!article.IsUpdatableWithRelationSecurity)
            {
                return new CopyResult { Message = MessageResult.Error(ArticleStrings.CannotAddBecauseOfRelationSecurity) };
            }

            article.UniqueId = guidForSubstitution ?? Guid.NewGuid();
            result.UniqueId = article.UniqueId.Value;

            var previousAggregatedArticles = article.AggregatedArticles;
            article.ReplaceAllUrlsToPlaceHolders();

            try
            {
                article = ArticleRepository.Copy(article);
                result.Id = article.Id;
                article.CopyAggregates(previousAggregatedArticles);

                var repo = new NotificationPushRepository(_pathHelper.S3Options);
                repo.PrepareNotifications(article, new[] { NotificationCode.Create }, disableNotifications);
                repo.SendNotifications();
            }
            catch (UnsupportedConstraintException)
            {
                result.Message = MessageResult.Error(ArticleStrings.UnsupportedConstraint);
            }

            return result;
        }

        public static Article New(int contentId, int? fieldId, int? articleId, bool? isChild, bool? boundToExternal)
        {
            var article = Article.CreateNew(contentId, fieldId, articleId, isChild);
            Ensure.That<ActionNotAllowedException>(article.IsArticleChangingActionsAllowed(boundToExternal), ContentStrings.ArticleChangingIsProhibited);
            return article;
        }

        public static Article NewForSave(int contentId) => Article.CreateNewForSave(contentId);

        public Article Create(Article article, string backendActionCode, bool? boundToExternal, bool disableNotifications)
        {
            Ensure.NotNull(article);
            Ensure.Not<ActionNotAllowedException>(article.IsAggregated, ArticleStrings.OperationIsNotAllowedForAggregated);
            Ensure.That<ActionNotAllowedException>(article.IsArticleChangingActionsAllowed(boundToExternal), ContentStrings.ArticleChangingIsProhibited);
            article.PathHelper = _pathHelper;
            return article.Persist(disableNotifications);
        }

        public Article Update(Article article, string backendActionCode, bool? boundToExternal, bool disableNotifications)
        {
            Ensure.NotNull(article);
            Ensure.Not<ActionNotAllowedException>(article.IsAggregated, ArticleStrings.OperationIsNotAllowedForAggregated);
            Ensure.That<ActionNotAllowedException>(article.IsArticleChangingActionsAllowed(boundToExternal), ContentStrings.ArticleChangingIsProhibited);
            article.PathHelper = _pathHelper;
            var result = article.Persist(disableNotifications);
            if (!string.IsNullOrWhiteSpace(backendActionCode) && backendActionCode.Equals(ActionCode.UpdateArticleAndUp, StringComparison.InvariantCultureIgnoreCase))
            {
                result.AutoUnlock();
            }

            return result;
        }

        public MessageResult Remove(int contentId, int id, bool fromArchive, bool? boundToExternal, bool disableNotifications)
        {
            var articleToRemove = ArticleRepository.GetById(id);
            return Remove(contentId, articleToRemove, fromArchive, boundToExternal, disableNotifications);
        }

        public MessageResult Remove(int contentId, Article articleToRemove, bool fromArchive, bool? boundToExternal, bool disableNotifications)
        {
            if (ContentRepository.IsAnyAggregatedFields(contentId))
            {
                if (!AllowRemoveAggregatedArticle(articleToRemove))
                {
                    return MessageResult.Error(ArticleStrings.OperationIsNotAllowedForAggregated);
                }
            }

            if (articleToRemove == null)
            {
                throw new Exception(string.Format(ArticleStrings.ArticleNotFound, articleToRemove.Id));
            }

            if (!articleToRemove.IsArticleChangingActionsAllowed(boundToExternal))
            {
                return MessageResult.Error(ContentStrings.ArticleChangingIsProhibited);
            }

            var content = ContentRepository.GetById(contentId);
            if (content == null)
            {
                throw new Exception(string.Format(ContentStrings.ContentNotFound, articleToRemove.ContentId));
            }

            if (articleToRemove.LockedByAnyoneElse)
            {
                return MessageResult.Error(string.Format(ArticleStrings.LockedByAnyoneElse, articleToRemove.LockedByDisplayName));
            }

            if (!articleToRemove.IsAccessible(ActionTypeCode.Remove))
            {
                return MessageResult.Error(ArticleStrings.CannotRemoveBecauseOfSecurity);
            }

            if (!articleToRemove.IsRemovableWithWorkflow)
            {
                return MessageResult.Error(ArticleStrings.CannotRemoveBecauseOfWorkflow);
            }

            if (!articleToRemove.IsRemovableWithRelationSecurity)
            {
                return MessageResult.Error(ArticleStrings.CannotRemoveBecauseOfRelationSecurity);
            }

            var idsToProceed = articleToRemove.SelfAndChildIds;

            var isUpdate = content.AutoArchive && !fromArchive;
            var code = isUpdate ? NotificationCode.Update : NotificationCode.Delete;
            var repo = new NotificationPushRepository(_pathHelper.S3Options);
            repo.PrepareNotifications(articleToRemove, new[] { code }, disableNotifications);
            if (isUpdate)
            {
                ArticleRepository.SetArchiveFlag(idsToProceed, true);
                repo.SendNotifications();
            }
            else
            {
                articleToRemove.RemoveAllVersionFolders();
                repo.SendNonServiceNotifications(true);
                ArticleRepository.MultipleDelete(idsToProceed);
                repo.SendServiceNotifications();
            }

            return null;
        }

        public MessageResult RemoveInternal(int contentId, int[] ids, bool fromArchive, bool? boundToExternal, bool disableNotifications)
        {
            if (ContentRepository.IsAnyAggregatedFields(contentId))
            {
                List<Article> articles = ArticleRepository.GetByIds(ids);

                if (articles.Any(x => !AllowRemoveAggregatedArticle(x)))
                {
                    return MessageResult.Error(ArticleStrings.OperationIsNotAllowedForAggregated);
                }
            }

            if (ids == null)
            {
                throw new ArgumentNullException(nameof(ids));
            }

            var content = ContentRepository.GetById(contentId);
            if (content == null)
            {
                throw new Exception(string.Format(ContentStrings.ContentNotFound, contentId));
            }

            if (!content.IsArticleChangingActionsAllowed(boundToExternal))
            {
                return MessageResult.Error(ContentStrings.ArticleChangingIsProhibited);
            }

            if (!content.AllowItemsPermission && !SecurityRepository.IsEntityAccessible(EntityTypeCode.Content, contentId, ActionTypeCode.Remove))
            {
                return MessageResult.Error(ArticleStrings.CannotRemoveBecauseOfSecurity);
            }

            var disableSecurityCheck = !content.AllowItemsPermission;
            var result = CheckIdResult<Article>.CreateForRemove(contentId, ids, disableSecurityCheck);

            var idsToProceed = result.ValidItems.Cast<Article>().SelectMany(a => a.SelfAndChildIds).ToArray();
            var idsToNotify = result.ValidItems.Select(n => n.Id).ToArray();

            var isUpdate = content.AutoArchive && !fromArchive;
            var code = isUpdate ? NotificationCode.Update : NotificationCode.Delete;
            var repo = new NotificationPushRepository(_pathHelper.S3Options);
            repo.PrepareNotifications(contentId, idsToNotify, code, disableNotifications);

            if (content.AutoArchive && !fromArchive)
            {
                ArticleRepository.SetArchiveFlag(idsToProceed, true);
                repo.SendNotifications();
            }
            else
            {
                repo.SendNonServiceNotifications(true);
                foreach (var entry in result.ValidItems)
                {
                    var article = (Article)entry;
                    article.RemoveAllVersionFolders();
                }

                ArticleRepository.MultipleDelete(idsToProceed);
                repo.SendServiceNotifications();
            }

            return result.GetServiceResult();
        }

        public MessageResult Remove(int contentId, int[] ids, bool fromArchive, bool? boundToExternal, bool disableNotifications) => RemoveInternal(contentId, ids, fromArchive, boundToExternal, disableNotifications);

        public MessageResult MultistepRemove(int contentId, int[] ids, bool fromArchive, bool? boundToExternal) => RemoveInternal(contentId, ids, fromArchive, boundToExternal, false);

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

        private bool AllowRemoveAggregatedArticle(Article article)
        {
            IEnumerable<int[]> values = article.FieldValues.Where(x => x.Field.Aggregated).Select(x => x.RelatedItems);

            return values.All(x => x.Length == 0);
        }

        private MessageResult PublishInternal(int contentId, int[] ids, bool? boundToExternal, bool disableNotifications)
        {
            if (ids == null)
            {
                throw new ArgumentNullException(nameof(ids));
            }

            if (ContentRepository.IsAnyAggregatedFields(contentId))
            {
                return MessageResult.Error(ArticleStrings.OperationIsNotAllowedForAggregated);
            }

            var content = ContentRepository.GetById(contentId);
            if (!content.IsArticleChangingActionsAllowed(boundToExternal))
            {
                return MessageResult.Error(ContentStrings.ArticleChangingIsProhibited);
            }

            if (!content.AllowItemsPermission && !SecurityRepository.IsEntityAccessible(EntityTypeCode.Content, contentId, ActionTypeCode.Update))
            {
                return MessageResult.Error(ArticleStrings.CannotUpdateBecauseOfSecurity);
            }

            var disableSecurityCheck = !content.AllowItemsPermission;
            var result = CheckIdResult<Article>.CreateForPublish(contentId, ids, disableSecurityCheck);
            var idsToProceed = result.ValidItems.Cast<Article>().SelectMany(a => a.SelfAndChildIds).ToArray();
            var idsToNotify = result.ValidItems.Cast<Article>().Select(n => n.Id).ToArray();

            var repo = new NotificationPushRepository(_pathHelper.S3Options);
            repo.PrepareNotifications(contentId, idsToNotify, new[] { NotificationCode.Update, NotificationCode.ChangeStatus }, disableNotifications);
            ArticleRepository.Publish(idsToProceed);
            repo.SendNotifications();

            return result.GetServiceResult();
        }

        public MessageResult Publish(int contentId, int[] ids, bool? boundToExternal, bool disableNotifications) => PublishInternal(contentId, ids, boundToExternal, disableNotifications);

        public MessageResult MultistepPublish(int contentId, int[] ids, bool? boundToExternal) => PublishInternal(contentId, ids, boundToExternal, false);

        public MessageResult MoveToArchive(int id, bool? boundToExternal, bool disableNotifications)
        {
            var articleToArchive = ArticleRepository.GetById(id);
            return MoveToArchive(articleToArchive, boundToExternal, disableNotifications);
        }

        public MessageResult MoveToArchive(Article articleToArchive, bool? boundToExternal, bool disableNotifications)
        {
            if (articleToArchive == null)
            {
                throw new Exception(string.Format(ArticleStrings.ArticleNotFound, articleToArchive.Id));
            }

            if (articleToArchive.IsAggregated)
            {
                return MessageResult.Error(ArticleStrings.OperationIsNotAllowedForAggregated);
            }

            if (!articleToArchive.IsArticleChangingActionsAllowed(boundToExternal))
            {
                return MessageResult.Error(ContentStrings.ArticleChangingIsProhibited);
            }

            if (articleToArchive.LockedByAnyoneElse)
            {
                return MessageResult.Error(string.Format(ArticleStrings.LockedByAnyoneElse, articleToArchive.LockedByDisplayName));
            }

            if (!articleToArchive.IsAccessible(ActionTypeCode.Archive))
            {
                return MessageResult.Error(ArticleStrings.CannotUpdateBecauseOfSecurity);
            }

            if (!articleToArchive.IsUpdatableWithWorkflow)
            {
                return MessageResult.Error(ArticleStrings.CannotRemoveBecauseOfWorkflow);
            }

            if (!articleToArchive.IsUpdatableWithRelationSecurity)
            {
                return MessageResult.Error(ArticleStrings.CannotUpdateBecauseOfRelationSecurity);
            }

            var idsToProceed = articleToArchive.SelfAndChildIds;
            var repo = new NotificationPushRepository(_pathHelper.S3Options);
            repo.PrepareNotifications(articleToArchive, new[] { NotificationCode.Update }, disableNotifications);

            ArticleRepository.SetArchiveFlag(idsToProceed, true);
            repo.SendNotifications();

            return null;
        }

        public MessageResult MoveToArchiveInternal(int contentId, int[] ids, bool? boundToExternal, bool disableNotifications)
        {
            if (ids == null)
            {
                throw new ArgumentNullException(nameof(ids));
            }

            if (ContentRepository.IsAnyAggregatedFields(contentId))
            {
                return MessageResult.Error(ArticleStrings.OperationIsNotAllowedForAggregated);
            }

            var content = ContentRepository.GetById(contentId);
            if (!content.IsArticleChangingActionsAllowed(boundToExternal))
            {
                return MessageResult.Error(ContentStrings.ArticleChangingIsProhibited);
            }

            if (!content.AllowItemsPermission && !SecurityRepository.IsEntityAccessible(EntityTypeCode.Content, contentId, ActionTypeCode.Update))
            {
                return MessageResult.Error(ArticleStrings.CannotUpdateBecauseOfSecurity);
            }

            var disableSecurityCheck = !content.AllowItemsPermission;
            var result = CheckIdResult<Article>.CreateForUpdate(contentId, ids, disableSecurityCheck);
            var idsToProceed = result.ValidItems.Cast<Article>().SelectMany(a => a.SelfAndChildIds).ToArray();
            var idsToNotify = result.ValidItems.Cast<Article>().Select(n => n.Id).ToArray();

            var repo = new NotificationPushRepository(_pathHelper.S3Options);
            repo.PrepareNotifications(contentId, idsToNotify, new[] { NotificationCode.Update }, disableNotifications);
            ArticleRepository.SetArchiveFlag(idsToProceed, true);
            repo.SendNotifications();

            return result.GetServiceResult();
        }

        public MessageResult MoveToArchive(int contentId, int[] ids, bool? boundToExternal, bool disableNotifications) => MoveToArchiveInternal(contentId, ids, boundToExternal, disableNotifications);

        public MessageResult MultistepMoveToArchive(int contentId, int[] ids, bool? boundToExternal) => MoveToArchiveInternal(contentId, ids, boundToExternal, false);

        public MessageResult RestoreFromArchive(int id, bool? boundToExternal, bool disableNotifications)
        {
            var articleToRestore = ArticleRepository.GetById(id);
            return RestoreFromArchive(articleToRestore, boundToExternal, disableNotifications);
        }

        public MessageResult RestoreFromArchive(Article articleToRestore, bool? boundToExternal, bool disableNotifications)
        {
            if (articleToRestore == null)
            {
                throw new Exception(string.Format(ArticleStrings.ArticleNotFound, articleToRestore.Id));
            }

            if (articleToRestore.IsAggregated)
            {
                return MessageResult.Error(ArticleStrings.OperationIsNotAllowedForAggregated);
            }

            if (!articleToRestore.IsUpdatableWithRelationSecurity)
            {
                return MessageResult.Error(ArticleStrings.CannotUpdateBecauseOfRelationSecurity);
            }

            if (!SecurityRepository.IsEntityAccessible(EntityTypeCode.Article, articleToRestore.Id, ActionTypeCode.Restore))
            {
                return MessageResult.Error(ArticleStrings.CannotUpdateBecauseOfSecurity);
            }

            if (!articleToRestore.IsArticleChangingActionsAllowed(boundToExternal))
            {
                return MessageResult.Error(ContentStrings.ArticleChangingIsProhibited);
            }

            var idsToProceed = articleToRestore.SelfAndChildIds;
            var repo = new NotificationPushRepository(_pathHelper.S3Options);
            repo.PrepareNotifications(articleToRestore, new[] { NotificationCode.Update }, disableNotifications);

            ArticleRepository.SetArchiveFlag(idsToProceed, false);
            repo.SendNotifications();

            return null;
        }

        private MessageResult RestoreFromArchiveInternal(int contentId, int[] ids, bool? boundToExternal, bool disableNotifications)
        {
            if (ids == null)
            {
                throw new ArgumentNullException(nameof(ids));
            }

            if (ContentRepository.IsAnyAggregatedFields(contentId))
            {
                return MessageResult.Error(ArticleStrings.OperationIsNotAllowedForAggregated);
            }

            var content = ContentRepository.GetById(contentId);
            if (!content.IsArticleChangingActionsAllowed(boundToExternal))
            {
                return MessageResult.Error(ContentStrings.ArticleChangingIsProhibited);
            }

            if (!content.AllowItemsPermission && !SecurityRepository.IsEntityAccessible(EntityTypeCode.Content, contentId, ActionTypeCode.Update))
            {
                return MessageResult.Error(ArticleStrings.CannotUpdateBecauseOfSecurity);
            }

            var disableSecurityCheck = !content.AllowItemsPermission;
            var result = CheckIdResult<Article>.CreateForUpdate(contentId, ids, disableSecurityCheck);

            var idsToProceed = result.ValidItems.Cast<Article>().SelectMany(a => a.SelfAndChildIds).ToArray();
            var idsToNotify = result.ValidItems.Cast<Article>().Select(n => n.Id).ToArray();

            var repo = new NotificationPushRepository(_pathHelper.S3Options);
            repo.PrepareNotifications(contentId, idsToNotify, new[] { NotificationCode.Update }, disableNotifications);
            ArticleRepository.SetArchiveFlag(idsToProceed, false);
            repo.SendNotifications();

            return result.GetServiceResult();
        }

        public MessageResult RestoreFromArchive(int contentId, int[] ids, bool? boundToExternal, bool disableNotifications) => RestoreFromArchiveInternal(contentId, ids, boundToExternal, disableNotifications);

        public MessageResult MultistepRestoreFromArchive(int contentId, int[] ids, bool? boundToExternal) => RestoreFromArchiveInternal(contentId, ids, boundToExternal, false);

        public static MessageResult RemovePreAction(int parentId, int id) => ConfirmHasChildren(id, true);

        public static MessageResult MultipleRemovePreAction(int parentId, int[] ids) => MultipleConfirmHasChildren(ids, true);

        public static MessageResult MultistepRemovePreAction(int parentId, int[] ids) => MultipleConfirmHasChildren(ids, true);

        public static MessageResult MoveToArchivePreAction(int id) => ConfirmHasChildren(id, false);

        public static MessageResult MultipleMoveToArchivePreAction(int[] ids) => MultipleConfirmHasChildren(ids, false);

        public static MessageResult MultistepMoveToArchivePreAction(int[] ids) => MultipleConfirmHasChildren(ids, false);

        public static IEnumerable<ListItem> GetAggregetableContentsForClassifier(Field classifier, string excludeValue) => FieldRepository.GetAggregatableContentListItemsForClassifier(classifier, excludeValue);

        public static Article GetAggregatedArticle(int rootArticleId, int rootContentId, int aggregatedContentId)
        {
            if (aggregatedContentId <= 0)
            {
                return null;
            }

            var rootArticle = rootArticleId == 0 ? Article.CreateNew(rootContentId) : Read(rootArticleId, rootContentId, false);
            return rootArticle.GetAggregatedArticleByClassifier(aggregatedContentId);
        }

        public static IEnumerable<ArticleContextQueryParam> GetContextQuery(int contentId, string contextString)
        {
            var parsed = contextString.Split(",".ToCharArray()).Select(int.Parse).ToDictionary(n => ArticleRepository.GetById(n).ContentId, n => n);
            return ContentRepository.GetById(contentId).GetContextSearchBlockItems().Select(n => new ArticleContextQueryParam
            {
                Name = "content_" + n.ContentId,
                Value = parsed.ContainsKey(n.ContentId) ? parsed[n.ContentId].ToString() : string.Empty,
                FieldId = n.FieldId
            });
        }

        public static void UnlockArticles(int[] ids)
        {
            ArticleRepository.UnlockArticlesByUser(ids);
        }

        public static List<ListItem> GetListOfFieldsForImport(int contentId)
        {
            return FieldRepository.GetList(contentId, false).Where(n => n.ExactType != FieldExactTypes.M2ORelation).Select(f => new ListItem { Text = f.Name, Value = f.Id.ToString() }).ToList();
        }

        public static List<ListItem> GetListOfFieldsToSort(int contentId) => new List<ListItem> { new ListItem { Text = FieldName.Id, Value = FieldName.Id } };

        public static Dictionary<string, List<string>> GetM2MValuesBatch(IEnumerable<int> ids, int linkId, string displayFieldName, int contentId) => ArticleRepository.GetM2MValuesBatch(ids.ToList(), linkId, displayFieldName, contentId);

        public static Dictionary<string, List<string>> GetM2OValuesBatch(IEnumerable<int> ids, int contentId, int fieldId, string fieldName, string displayFieldName) => ArticleRepository.GetM2OValuesBatch(ids.ToList(), contentId, fieldId, fieldName, displayFieldName);

        public static string GetTitleName(int contentId) => ContentRepository.GetTitleName(contentId);

        public static IList<int> GetParentIds(int id, int fieldId)
        {
            var treeField = FieldRepository.GetById(fieldId);
            return ArticleRepository.GetParentIds(new[] { id }, treeField.Id, treeField.Name);
        }

        public static IList<int> GetParentIds(IList<int> ids, int fieldId)
        {
            var relatedField = FieldRepository.GetById(fieldId).RelatedToContent.Fields.Single(f => f.UseForTree);
            return ArticleRepository.GetParentIds(ids, relatedField.Id, relatedField.Name);
        }

        public static IList<int> GetChildArticles(IList<int> ids, int fieldId, string filter)
        {
            var relatedField = FieldRepository.GetById(fieldId).RelatedToContent.Fields.Single(f => f.UseForTree);
            return ArticleRepository.GetChildArticles(ids, relatedField.Name, relatedField.ContentId, filter);
        }
    }
}
