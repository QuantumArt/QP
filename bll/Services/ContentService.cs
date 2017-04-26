using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Quantumart.QP8.BLL.Exceptions;
using Quantumart.QP8.BLL.Factories;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL.Services
{
    public class ContentService
    {
        public static Content New(int siteId, int? groupId)
        {
            return InternalNew(siteId, groupId);
        }

        internal static Content InternalNew(int siteId, int? groupId)
        {
            var site = SiteRepository.GetById(siteId);
            if (site == null)
            {
                throw new Exception(string.Format(SiteStrings.SiteNotFound, siteId));
            }

            var content = new Content(site);
            if (groupId.HasValue)
            {
                content.GroupId = groupId.Value;
            }

            return content;
        }

        public static Content Read(int id)
        {
            return InternalRead(id);
        }

        internal static Content InternalRead(int id)
        {
            var content = ContentRepository.GetById(id);
            if (content == null)
            {
                throw new Exception(string.Format(ContentStrings.ContentNotFound, id));
            }

            if (!content.IsUpdatable)
            {
                content.IsReadOnly = true;
            }

            return content;
        }

        public static Content ReadForUpdate(int id)
        {
            return Read(id);
        }

        public static Content Save(Content item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            return item.Persist();
        }

        public static Content Update(Content item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            if (!ContentRepository.Exists(item.Id))
            {
                throw new Exception(string.Format(ContentStrings.ContentNotFound, item.Id));
            }

            if (!item.IsContentChangingActionsAllowed)
            {
                throw new ActionNotAllowedException(ContentStrings.ContentChangingIsProhibited);
            }

            return item.Persist();
        }

        public static MessageResult SimpleRemove(int id)
        {
            var item = ContentRepository.GetById(id);
            if (item == null)
            {
                throw new ApplicationException(string.Format(FieldStrings.FieldNotFound, id));
            }

            var violationMessages = item.Die().ToList();
            return violationMessages.Any() ? MessageResult.Error(string.Join(Environment.NewLine, violationMessages), new[] { id }) : null;
        }

        public static ContentGroup ReadGroup(int id, int siteId)
        {
            var group = ContentRepository.GetGroupById(id);
            if (group == null)
            {
                throw new Exception(string.Format(ContentStrings.GroupNotFound, id));
            }

            if (!group.IsUpdatable || group.IsDefault)
            {
                group.IsReadOnly = true;
            }

            return group;
        }

        public static ContentGroup ReadGroupForUpdate(int id, int siteId)
        {
            return ReadGroup(id, siteId);
        }

        public static ContentGroup NewGroup(int siteId)
        {
            return new ContentGroup(siteId);
        }

        public static ContentGroup NewGroupForSave(int siteId)
        {
            return NewGroup(siteId);
        }

        public static ContentGroup SaveGroup(ContentGroup item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            return ContentRepository.SaveGroup(item);
        }

        public static ContentGroup UpdateGroup(ContentGroup item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            return ContentRepository.UpdateGroup(item);
        }

        public static ContentCopyResult Copy(int id, int? forceId, int[] forceFieldIds, int[] forceLinkIds)
        {
            var result = new ContentCopyResult();
            var content = ContentRepository.GetById(id);
            if (content == null)
            {
                throw new Exception(string.Format(ContentStrings.ContentNotFound, id));
            }

            if (!content.Site.IsUpdatable || !content.IsAccessible(ActionTypeCode.Read))
            {
                result.Message = MessageResult.Error(ContentStrings.CannotCopyBecauseOfSecurity);
            }

            if (!content.IsContentChangingActionsAllowed)
            {
                throw new ActionNotAllowedException(ContentStrings.ContentChangingIsProhibited);
            }

            if (result.Message == null)
            {
                content = ContentRepository.Copy(content, forceId, forceFieldIds, forceLinkIds, false);
                result.FieldIds = content.Fields.Select(n => n.Id).ToArray();
                result.LinkIds = ContentRepository.GetContentLinks(content.Id).Select(n => n.LinkId).ToArray();
                result.Id = content.Id;
            }

            return result;
        }

        public static ContentInitListResult InitList(int siteId, bool isVirtual = false)
        {
            var isActionAccessable = !isVirtual && SecurityRepository.IsActionAccessible(ActionCode.AddNewContent);
            if (siteId > 0)
            {
                var site = SiteRepository.GetById(siteId);
                if (site == null)
                {
                    throw new Exception(string.Format(SiteStrings.SiteNotFound, siteId));
                }

                var isSiteAccessable = !isVirtual && SecurityRepository.IsEntityAccessible(EntityTypeCode.Site, siteId, ActionTypeCode.Update);
                return new ContentInitListResult
                {
                    ParentName = site.Name,
                    IsVirtual = isVirtual,
                    IsAddNewAccessable = isActionAccessable && isSiteAccessable
                };
            }

            return new ContentInitListResult
            {
                IsVirtual = isVirtual,
                IsAddNewAccessable = isActionAccessable
            };
        }

        public static ContentInitListResult InitListForObject()
        {
            return new ContentInitListResult { IsAddNewAccessable = false };
        }

        public static ContentInitListResult InitList(string parentName)
        {
            return new ContentInitListResult
            {
                ParentName = parentName,
                IsAddNewAccessable = false
            };
        }

        public static IEnumerable<ListItem> GetContentsForUnion(int currentSiteId, IEnumerable<int> ids)
        {
            return ContentRepository.GetSimpleList(currentSiteId, ids);
        }

        public static IEnumerable<ListItem> GetContentsForParentContent(int currentSiteId, int id)
        {
            return ContentRepository.GetSimpleList(currentSiteId, id);
        }

        public static ListResult<ContentListItem> List(ContentListFilter filter, ListCommand cmd)
        {
            return ContentRepository.GetList(filter, cmd);
        }

        public static ListResult<ContentListItem> List(ContentListFilter filter, ListCommand cmd, int id)
        {
            return ContentRepository.GetList(filter, cmd, new[] { id });
        }

        public static ListResult<ContentListItem> List(ContentListFilter filter, ListCommand cmd, int[] selectedContentIds)
        {
            return ContentRepository.GetList(filter, cmd, selectedContentIds);
        }

        public static ListResult<ContentListItem> ListForContainer(ContentListFilter filter, ListCommand cmd, int id)
        {
            filter.Mode = ContentSelectMode.ForContainer;
            return List(filter, cmd, id);
        }

        public static ListResult<ContentListItem> ListForForm(ContentListFilter filter, ListCommand cmd, int id)
        {
            filter.Mode = ContentSelectMode.ForForm;
            return List(filter, cmd, id);
        }

        public static ListResult<ContentListItem> ListForField(ContentListFilter filter, ListCommand cmd, int id)
        {
            filter.Mode = ContentSelectMode.ForField;
            return List(filter, cmd, id);
        }

        public static ListResult<ContentListItem> ListForJoin(ContentListFilter filter, ListCommand cmd, int id)
        {
            filter.Mode = ContentSelectMode.ForJoin;
            return List(filter, cmd, id);
        }

        public static ListResult<ContentListItem> ListForWorkflow(ContentListFilter filter, ListCommand cmd, int[] ids)
        {
            filter.Mode = ContentSelectMode.ForWorkflow;
            return List(filter, cmd, ids);
        }

        public static ListResult<ContentListItem> ListForCustomAction(ContentListFilter filter, ListCommand cmd, int[] ids)
        {
            filter.Mode = ContentSelectMode.ForCustomAction;
            return List(filter, cmd, ids);
        }

        public static ListResult<ContentListItem> ListForUnion(ContentListFilter filter, ListCommand cmd, int[] ids)
        {
            filter.Mode = ContentSelectMode.ForUnion;
            return List(filter, cmd, ids);
        }

        public static IEnumerable<ListItem> GetSiteContentGroupsForFilter(int siteId)
        {
            return ContentRepository.GetGroupSimpleList(siteId);
        }

        public static LibraryResult Library(int id, string subFolder)
        {
            if (!ContentRepository.Exists(id))
            {
                throw new Exception(string.Format(ContentStrings.ContentNotFound, id));
            }

            var factory = new ContentFolderFactory();
            var repository = factory.CreateRepository();
            var folder = repository.GetBySubFolder(id, subFolder);
            return new LibraryResult { Folder = folder };
        }

        public static ListResult<FolderFile> GetFileList(ListCommand command, int parentFolderId, LibraryFileFilter filter)
        {
            var factory = new ContentFolderFactory();
            var repository = factory.CreateRepository();
            var folder = repository.GetById(parentFolderId);
            if (folder == null)
            {
                throw new Exception(string.Format(LibraryStrings.ContentFolderNotExists, parentFolderId));
            }

            return folder.GetFiles(command, filter);
        }

        public static PathInfo GetPathInfo(int folderId)
        {
            return ContentFolder.GetPathInfo(folderId);
        }

        public static IEnumerable<ListItem> GetAcceptableContentForRelation(int contentId)
        {
            if (!ContentRepository.Exists(contentId))
            {
                throw new ArgumentException(string.Format(ContentStrings.ContentNotFound, contentId));
            }

            return ContentRepository.GetAcceptableContentForRelation(contentId);
        }


        public static MessageResult EnableArticlePermissions(int id)
        {
            var content = ReadForUpdate(id);
            if (!content.AllowItemsPermission)
            {
                content.AllowItemsPermission = true;
                Update(content);
                return MessageResult.Info(ContentStrings.ArticlePermissionsComplete);
            }
            return null;
        }

        public static IEnumerable<ListItem> GetRelateableFields(int contentId, int currentId)
        {
            return ContentRepository.GetById(contentId).RelateableFields
                .Where(f => !f.IsNew && f.Id != currentId)
                .Select(f => new ListItem(f.Id.ToString(), f.Name));
        }

        public static IEnumerable<ArticleContextSearchBlockItem> GetContentsForContextSwitching(int id)
        {
            return ContentRepository.GetById(id).GetContextSearchBlockItems();
        }

        public static string GetNameById(int contentId)
        {
            return EntityObjectRepository.GetName(EntityTypeCode.Content, contentId);
        }

        public static void CopyContentLinks(int sourceSiteId, int destinationSiteId)
        {
            string relBetweenLinks;
            ContentRepository.CopyContentLinks(sourceSiteId, destinationSiteId, out relBetweenLinks);
            FieldRepository.UpdateAttributeLinkIdAndDefaultValue(sourceSiteId, destinationSiteId, relBetweenLinks);
        }

        public static void UpdateContents(int sourceSiteId, int destinationSiteId)
        {
            var relBetweenAttributes = FieldRepository.GetRelationsBetweenAttributesXml(sourceSiteId, destinationSiteId, string.Empty, false, true);
            FieldRepository.UpdateAttributes(sourceSiteId, destinationSiteId, relBetweenAttributes, string.Empty);
            FieldRepository.CopyCommandFieldBind(relBetweenAttributes);
            FieldRepository.CopyStyleFieldBind(relBetweenAttributes);
            FieldRepository.UpdateAttributesOrder(destinationSiteId, relBetweenAttributes, string.Empty);

            var relBetweenContents = ContentRepository.GetRelationsBetweenContentsXml(sourceSiteId, destinationSiteId, string.Empty);
            ContentRepository.UpdateContentsParentContentId(destinationSiteId, relBetweenContents);
            ContentRepository.CopyContentAccess(destinationSiteId, relBetweenContents);
            ContentRepository.CopyContentsCustomActions(relBetweenContents);
            ContentRepository.CopyContentFolders(relBetweenContents);
            ContentRepository.UpdateContentFolders(sourceSiteId, destinationSiteId, relBetweenContents);
        }

        public static int CopyContents(int sourceSiteId, int destinationSiteId, int startFrom, int endOn)
        {
            string newContentIds;
            var count = ContentRepository.CopyContents(sourceSiteId, destinationSiteId, startFrom, endOn, out newContentIds);

            var relBetweenContents = ContentRepository.GetRelationsBetweenContentsXml(sourceSiteId, destinationSiteId, newContentIds);
            var relBetweenStatuses = ContentRepository.GetRelationsBetweenStatuses(sourceSiteId, destinationSiteId);

            FieldRepository.CopyContentsAttributes(sourceSiteId, destinationSiteId, newContentIds, false);
            var relBetweenAttributes = FieldRepository.GetRelationsBetweenAttributesXml(sourceSiteId, destinationSiteId, newContentIds, false, true);
            NotificationRepository.CopyContentNotifications(relBetweenContents, relBetweenStatuses, relBetweenAttributes);

            string relBetweenConstraints;
            ContentRepository.CopyContentConstraints(relBetweenContents, out relBetweenConstraints);
            ContentConstraintRepository.CopyContentConstrainRules(relBetweenConstraints, relBetweenAttributes);

            FieldRepository.CopyDynamicImageAttributes(relBetweenAttributes);
            ContentRepository.CopyContentWorkflowBind(sourceSiteId, destinationSiteId, relBetweenContents);
            return count;
        }

        public static IEnumerable<DataRow> CopyContentsData(int sourceSiteId, int destinationSiteId, string contentsToCopy, int startFrom, int endBy)
        {
            var relBetweenContents = ContentRepository.GetRelationsBetweenContentsXml(sourceSiteId, destinationSiteId, string.Empty);
            var relBetweenStatuses = ContentRepository.GetRelationsBetweenStatuses(sourceSiteId, destinationSiteId);

            var items = ContentRepository.CopyContentItems(sourceSiteId, destinationSiteId, contentsToCopy, startFrom, endBy, relBetweenContents, relBetweenStatuses).ToList();
            var relBetweenItems = MultistepActionHelper.GetXmlFromDataRows(items, "item");

            ContentRepository.CopyContentItemSchedule(relBetweenContents);
            WorkflowRepository.CopyArticleWorkflowBind(sourceSiteId, destinationSiteId, relBetweenItems);
            ContentRepository.CopyContentItemAccess(relBetweenItems);

            var relBetweenAttributes = FieldRepository.GetRelationsBetweenAttributesXml(sourceSiteId, destinationSiteId, string.Empty, false, true);
            ContentRepository.UpdateContentData(relBetweenAttributes, relBetweenItems);
            return items;
        }

        public static void UpdateArticlesLinks(int sourceSiteId, int destinationSiteId, string relationsBetweenArticles)
        {
            ContentRepository.UpdateO2MValues(sourceSiteId, destinationSiteId, relationsBetweenArticles);
            ContentRepository.CopyArticleWorkflowBind(sourceSiteId, destinationSiteId, relationsBetweenArticles);

            var relBetweenLinks = ContentRepository.GetRelationsBetweenLinks(sourceSiteId, destinationSiteId);
            ContentRepository.UpdateContentDataAfterCopyingArticles(relationsBetweenArticles, relBetweenLinks);
            ContentRepository.CopyItemToItems(relationsBetweenArticles, relBetweenLinks);
            ContentRepository.UpdateItemToItem(relationsBetweenArticles, relBetweenLinks);
            ContentRepository.UpdateAttributesAfterCopyingArticles(destinationSiteId, relationsBetweenArticles);
        }

        public static void FillLinkTables(int sourceSiteId, int destinationSiteId)
        {
            var relBetweenLinks = ContentRepository.GetRelationsBetweenLinks(sourceSiteId, destinationSiteId);
            ContentRepository.FillLinksTables(relBetweenLinks);
        }
    }
}
