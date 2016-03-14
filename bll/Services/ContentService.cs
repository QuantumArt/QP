using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Quantumart.QP8.BLL.Exceptions;
using Quantumart.QP8.BLL.Factories;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;
using Quantumart.QP8.BLL.Helpers;


namespace Quantumart.QP8.BLL.Services
{
	public static class ContentService
	{
		/// <summary>
		/// Возвращает контент для добавления
		/// </summary>
		/// <param name="siteId">идентификатор сайта</param>
		/// <returns>контент</returns>
		public static Content New(int siteId, int? groupId)
		{
			return InternalNew(siteId, groupId);
		}

		internal static Content InternalNew(int siteId, int? groupId)
		{
			Site site = SiteRepository.GetById(siteId);
			if (site == null)
				throw new Exception(String.Format(SiteStrings.SiteNotFound, siteId));
			Content content = new Content(site);
			if (groupId.HasValue)
				content.GroupId = groupId.Value;
			return content;
		}

		public static Content NewForSave(int siteId)
		{
			return New(siteId, null);
		}

		/// <summary>
		/// Возвращает контент для редактирования или просмотра
		/// </summary>
		/// <param name="id">идентификатор контента</param>
		/// <returns>контент</returns>
		public static Content Read(int id)
		{
			return InternalRead(id);
		}

		internal static Content InternalRead(int id)
		{
			Content content = ContentRepository.GetById(id);
			if (content == null)
				throw new Exception(String.Format(ContentStrings.ContentNotFound, id));
			if (!content.IsUpdatable)
				content.IsReadOnly = true;
			return content;
		}

		public static Content ReadForUpdate(int id)
		{
			return Read(id);
		}

		public static Content Save(Content item)
		{
			if (item == null)
				throw new ArgumentNullException("item");
			item = item.Persist();

			return item;
		}

		public static Content Update(Content item)
		{
			if (item == null)
				throw new ArgumentNullException("item");
			if (!ContentRepository.Exists(item.Id))
				throw new Exception(String.Format(ContentStrings.ContentNotFound, item.Id));
			if (!item.IsContentChangingActionsAllowed)
				throw ActionNotAllowedException.CreateNotAllowedForContentChangingActionException();
			item = item.Persist();
			return item;
		}

		public static MessageResult SimpleRemove(int id)
		{
			Content item = ContentRepository.GetById(id);
			if (item == null)
				throw new ApplicationException(String.Format(FieldStrings.FieldNotFound, id));

			var violationMessages = item.Die();

			if (violationMessages.Any())
				return MessageResult.Error(String.Join(Environment.NewLine, violationMessages), new[] { id });
			else
				return null;
		}

		public static ContentGroup ReadGroup(int id, int siteId)
		{
			ContentGroup group = ContentRepository.GetGroupById(id);
			if (group == null)
				throw new Exception(String.Format(ContentStrings.GroupNotFound, id));
			if (!group.IsUpdatable || group.IsDefault)
				group.IsReadOnly = true;
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
				throw new ArgumentNullException("item");
			return ContentRepository.SaveGroup(item);
		}

		public static ContentGroup UpdateGroup(ContentGroup item)
		{
			if (item == null)
				throw new ArgumentNullException("item");
			return ContentRepository.UpdateGroup(item);
		}


		/// <summary>
		/// Копирует статью
		/// </summary>
		/// <param name="id">идентификатор статьи</param>
		public static ContentCopyResult Copy(int id, int? forceId, int[] forceFieldIds, int[] forceLinkIds)
		{
			ContentCopyResult result = new ContentCopyResult();
			Content content = ContentRepository.GetById(id);
			if (content == null)
				throw new Exception(String.Format(ContentStrings.ContentNotFound, id));

			if (!content.Site.IsUpdatable || !content.IsAccessible(ActionTypeCode.Read))
				result.Message = MessageResult.Error(ContentStrings.CannotCopyBecauseOfSecurity);

			if (!content.IsContentChangingActionsAllowed)
				throw ActionNotAllowedException.CreateNotAllowedForContentChangingActionException();

			if (result.Message == null)
			{
				content = ContentRepository.Copy(content, forceId, forceFieldIds, forceLinkIds, false);
				result.FieldIds = content.Fields.Select(n => n.Id).ToArray();
				result.LinkIds = ContentRepository.GetContentLinks(content.Id).Select(n => n.LinkId).ToArray();
				result.Id = content.Id;
			}
			return result;
		}

		/// <summary>
		/// Инициализация списка контентов
		/// </summary>
		public static ContentInitListResult InitList(int siteId, bool isVirtual = false)
		{
			bool isActionAccessable = !isVirtual && SecurityRepository.IsActionAccessible(ActionCode.AddNewContent);
			if (siteId > 0)
			{
				Site site = SiteRepository.GetById(siteId);
				if (site == null)
					throw new Exception(String.Format(SiteStrings.SiteNotFound, siteId));

				bool isSiteAccessable = !isVirtual && SecurityRepository.IsEntityAccessible(EntityTypeCode.Site, siteId, ActionTypeCode.Update);
				return new ContentInitListResult
				{
					ParentName = site.Name,
					IsVirtual = isVirtual,
					IsAddNewAccessable = isActionAccessable && isSiteAccessable
				};
			}
			else
				return new ContentInitListResult()
				{
					IsVirtual = isVirtual,
					IsAddNewAccessable = isActionAccessable
				};
		}

		public static ContentInitListResult InitListForObject()
		{
			ContentInitListResult result = new ContentInitListResult { IsAddNewAccessable = false };
			return result;
		}

		public static ContentInitListResult InitList(string parentName)
		{
			return new ContentInitListResult
			{
				ParentName = parentName,
				IsAddNewAccessable = false
			};
		}

		public static IEnumerable<ListItem> GetContentsForUnion(int currentSiteId, IEnumerable<int> IDs)
		{
			return ContentRepository.GetSimpleList(currentSiteId, IDs);
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
			return ContentRepository.GetList(filter, cmd, new int[] { id });
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
				throw new Exception(String.Format(ContentStrings.ContentNotFound, id));
			ContentFolderFactory factory = new ContentFolderFactory();
			FolderRepository repository = factory.CreateRepository();
			Folder folder = repository.GetBySubFolder(id, subFolder);
			return new LibraryResult() { Folder = folder };
		}

		public static ListResult<FolderFile> GetFileList(ListCommand command, int parentFolderId, LibraryFileFilter filter)
		{
			ContentFolderFactory factory = new ContentFolderFactory();
			FolderRepository repository = factory.CreateRepository();
			Folder folder = repository.GetById(parentFolderId);
			if (folder == null)
				throw new Exception(String.Format(LibraryStrings.ContentFolderNotExists, parentFolderId));
			return folder.GetFiles(command, filter);
		}

		public static PathInfo GetPathInfo(int folderId)
		{
			return ContentFolder.GetPathInfo(folderId);
		}

		/// <summary>
		/// Вернуть список контентов доступных для связи с текущим контентом
		/// </summary>
		/// <param name="contentId"></param>
		/// <returns></returns>
		public static IEnumerable<ListItem> GetAcceptableContentForRelation(int contentId)
		{
			if (!ContentRepository.Exists(contentId))
				throw new ArgumentException(String.Format(ContentStrings.ContentNotFound, contentId));
			return ContentRepository.GetAcceptableContentForRelation(contentId);
		}


		public static MessageResult EnableArticlePermissions(int id)
		{
			Content content = ReadForUpdate(id);
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
			string relBetweenLinks = String.Empty;
			ContentRepository.CopyContentLinks(sourceSiteId, destinationSiteId, ref relBetweenLinks);
			FieldRepository.UpdateAttributeLinkIdAndDefaultValue(sourceSiteId, destinationSiteId, relBetweenLinks);
		}

		public static void UpdateContents(int sourceSiteId, int destinationSiteId)
		{
			string relBetweenAttributes = FieldRepository.GetRelationsBetweenAttributesXML(sourceSiteId, destinationSiteId, String.Empty, forVirtualContents: false, byNewContents: true);
			FieldRepository.UpdateAttributes(sourceSiteId, destinationSiteId, relBetweenAttributes, String.Empty);
			FieldRepository.CopyCommandFieldBind(relBetweenAttributes);
			FieldRepository.CopyStyleFieldBind(relBetweenAttributes);
			FieldRepository.UpdateAttributesOrder(destinationSiteId, relBetweenAttributes, String.Empty);

			string relBetweenContents = ContentRepository.GetRelationsBetweenContentsXML(sourceSiteId, destinationSiteId, String.Empty);
			ContentRepository.UpdateContentsParentContentId(destinationSiteId, relBetweenContents);
			ContentRepository.CopyContentAccess(destinationSiteId, relBetweenContents);
			ContentRepository.CopyContentsCustomActions(relBetweenContents);
			ContentRepository.CopyContentFolders(relBetweenContents);
			ContentRepository.UpdateContentFolders(sourceSiteId, destinationSiteId, relBetweenContents);
		}

		public static int CopyContents(int sourceSiteId, int destinationSiteId, int startFrom, int endOn)
		{
			string newContentIds = String.Empty;

			int count = ContentRepository.CopyContents(sourceSiteId, destinationSiteId, startFrom, endOn, ref newContentIds);

			string relBetweenContents = ContentRepository.GetRelationsBetweenContentsXML(sourceSiteId, destinationSiteId, newContentIds);
			string relBetweenStatuses = ContentRepository.GetRelationsBetweenStatuses(sourceSiteId, destinationSiteId);

			FieldRepository.CopyContentsAttributes(sourceSiteId, destinationSiteId, newContentIds, isContentsVirtual: false);

			string relBetweenAttributes = FieldRepository.GetRelationsBetweenAttributesXML(sourceSiteId, destinationSiteId, newContentIds, forVirtualContents: false, byNewContents: true);

			NotificationRepository.CopyContentNotifications(relBetweenContents, relBetweenStatuses, relBetweenAttributes);

			string relBetweenConstraints = String.Empty;
			ContentRepository.CopyContentConstraints(relBetweenContents, ref relBetweenConstraints);
			ContentConstraintRepository.CopyContentConstrainRules(relBetweenConstraints, relBetweenAttributes);

			FieldRepository.CopyDynamicImageAttributes(relBetweenAttributes);
			ContentRepository.CopyContentWorkflowBind(sourceSiteId, destinationSiteId, relBetweenContents);

			return count;
		}

		public static IEnumerable<DataRow> CopyContentsData(int sourceSiteId, int destinationSiteId, string contentsToCopy, int startFrom, int endBy)
		{
			string relBetweenContents = ContentRepository.GetRelationsBetweenContentsXML(sourceSiteId, destinationSiteId, String.Empty);
			string relBetweenStatuses = ContentRepository.GetRelationsBetweenStatuses(sourceSiteId, destinationSiteId);

			IEnumerable<DataRow> items = ContentRepository.CopyContentItems(sourceSiteId, destinationSiteId, contentsToCopy, startFrom, endBy, relBetweenContents, relBetweenStatuses);
			string relBetweenItems = MultistepActionHelper.GetXmlFromDataRows(items, "item");

			ContentRepository.CopyContentItemSchedule(relBetweenContents);
			WorkflowRepository.CopyArticleWorkflowBind(sourceSiteId, destinationSiteId, relBetweenItems);
			ContentRepository.CopyContentItemAccess(relBetweenItems);

			string relBetweenAttributes = FieldRepository.GetRelationsBetweenAttributesXML(sourceSiteId, destinationSiteId, String.Empty, forVirtualContents: false, byNewContents: true);

			ContentRepository.UpdateContentData(relBetweenAttributes, relBetweenItems);

			return items;
		}

		public static void UpdateArticlesLinks(int sourceSiteId, int destinationSiteId, string relationsBetweenArticles)
		{
			ContentRepository.UpdateO2MValues(sourceSiteId, destinationSiteId, relationsBetweenArticles);
			ContentRepository.CopyArticleWorkflowBind(sourceSiteId, destinationSiteId, relationsBetweenArticles);

			string relBetweenLinks = ContentRepository.GetRelationsBetweenLinks(sourceSiteId, destinationSiteId);
			ContentRepository.UpdateContentDataAfterCopyingArticles(relationsBetweenArticles, relBetweenLinks);
			ContentRepository.CopyItemToItems(relationsBetweenArticles, relBetweenLinks);
			ContentRepository.UpdateItemToItem(relationsBetweenArticles, relBetweenLinks);
			ContentRepository.UpdateAttributesAfterCopyingArticles(destinationSiteId, relationsBetweenArticles);
		}
	}
}
