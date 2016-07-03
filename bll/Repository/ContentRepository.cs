using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL.Mappers;
using Quantumart.QP8.Constants;
using Quantumart.QP8.DAL;
using Quantumart.QP8.DAL.DTO;
using Quantumart.QP8.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Objects;
using System.Linq;
using System.Linq.Expressions;

namespace Quantumart.QP8.BLL.Repository
{
    internal class ContentRepository
    {
        #region lists
        /// <summary>
        /// Возвращает список контентов сайта
        /// </summary>
        /// <returns>список контентов</returns>
        internal static ListResult<ContentListItem> GetList(ContentListFilter filter, ListCommand cmd, int[] selectedContentIDs = null, int workflowId = 0)
        {
            using (var scope = new QPConnectionScope())
            {
                int totalRecords;
                var options = new ContentPageOptions
                {
                    ContentName = filter.ContentName,
                    SiteId = filter.SiteId,
                    IsVirtual = filter.IsVirtual,
                    GroupId = filter.GroupId,
                    Mode = filter.Mode,
                    SelectedIDs = selectedContentIDs,
                    UserId = QPContext.CurrentUserId,
                    UseSecurity = !QPContext.IsAdmin,
                    IsAdmin = QPContext.IsAdmin,
                    SortExpression = cmd.SortExpression,
                    StartRecord = cmd.StartRecord,
                    PageSize = cmd.PageSize,
                    LanguageId = QPContext.CurrentUserIdentity.LanguageId,
                    CustomFilter = filter.CustomFilter
                };

                var rows = Common.GetContentsPage(scope.DbConnection, options, out totalRecords);
                return new ListResult<ContentListItem> { Data = MappersRepository.ContentListItemRowMapper.GetBizList(rows.ToList()), TotalRecords = totalRecords };
            }
        }

        /// <summary>
        /// Возвращает список контентов по спику id
        /// </summary>
        internal static IEnumerable<Content> GetList(IEnumerable<int> IDs, bool loadSite = false, bool loadFields = false)
        {
            if (IDs != null && IDs.Any())
            {
                ObjectQuery<ContentDAL> context = QPContext.EFContext.ContentSet;
                if (loadSite)
                    context = context.Include("Site");
                if (loadFields)
                    context = context.Include("Fields");


                IEnumerable<decimal> decIDs = Converter.ToDecimalCollection(IDs).Distinct().ToArray();
                return MappersRepository.ContentMapper.GetBizList(context.Where(c => decIDs.Contains(c.Id)).ToList());
            }
            return Enumerable.Empty<Content>();
        }

        internal static IEnumerable<Content> GetAll()
        {
            return MappersRepository.ContentMapper.GetBizList(QPContext.EFContext.ContentSet.ToList());
        }

        internal static IEnumerable<Content> GetListBySiteId(int siteId)
        {
            var context = QPContext.EFContext.ContentSet;
            return MappersRepository.ContentMapper.GetBizList(context.Include("WorkflowBinding").Where(c => c.SiteId == siteId).ToList());
        }

        /// <summary>
        /// Простой список контентов сайта
        /// </summary>
        /// <param name="currentSiteId">сайт</param>
        /// <param name="id">id текущего контента</param>
        /// <returns></returns>
        internal static IEnumerable<ListItem> GetSimpleList(int currentSiteId, int id)
        {
            var contents = GetListBySiteId(currentSiteId);
            return contents.Select(c => new ListItem(c.Id.ToString(), c.Name));
        }

        /// <summary>
        /// Простой список контентов по выбранному списку ID (для MultipeItemPicker)
        /// </summary>
        /// <param name="currentSiteId">текущий сайт (если контент не с текущего сайта, то возвращается полное имя)</param>
        /// <param name="IDs">список выбранных ID</param>
        /// <returns></returns>
        internal static IEnumerable<ListItem> GetSimpleList(int currentSiteId, IEnumerable<int> IDs)
        {
            var contents = GetList(IDs, true);
            return contents.Select(c => new ListItem(c.Id.ToString(), c.SiteId == currentSiteId ? c.Name : string.Format("{0}.{1}", c.Site.Name, c.Name)));
        }

        /// <summary>
        /// Возвращает ID контентов для которых запрещены операции редактирования
        /// у которых DisableChangingActions == true
        /// </summary>
        /// <returns></returns>
        internal static IEnumerable<int> GetChangeDisabledIDs()
        {
            return Converter.ToInt32Collection(QPContext.EFContext.ContentSet.Where(c => c.DisableChangingActions).Select(c => c.Id).ToArray());
        }
        #endregion

        #region CRUD
        /// <summary>
        /// Возвращает контент по идентификатору
        /// </summary>
        /// <param name="id">идентификатор контента</param>
        /// <returns>информация о контенте</returns>
        internal static Content GetById(int id)
        {
            var result = GetByIdFromCache(id);
            if (result != null)
            {
                return result;
            }

            var content = MappersRepository.ContentMapper.GetBizObject(QPContext.EFContext.ContentSet.Include("LastModifiedByUser").SingleOrDefault(n => n.Id == id));
            return content;
        }

        private static Content GetByIdFromCache(int id)
        {
            Content result = null;
            var cache = QPContext.GetContentCache();
            if (cache != null && cache.ContainsKey(id))
            {
                result = cache[id];
            }

            return result;
        }

        /// <summary>
        /// Возвращает контент по идентификатору
        /// </summary>
        /// <param name="id">идентификатор контента</param>
        /// <returns>информация о контенте</returns>
        internal static Content GetByIdWithFields(int id)
        {
            var content = MappersRepository.ContentMapper.GetBizObject(QPContext.EFContext.ContentSet.Include("LastModifiedByUser").Include("Fields").SingleOrDefault(n => n.Id == id));
            return content;
        }

        internal static int GetSiteId(int id)
        {
            return (int)QPContext.EFContext.ContentSet.Where(n => n.Id == (decimal)id).Select(n => n.SiteId).Single();
        }

        /// <summary>
        /// Добавляет новый контент
        /// </summary>
        /// <param name="content">информация о контенте</param>
        /// <param name="createDefaultField"></param>
        /// <returns>информация о контенте</returns>
        internal static Content Save(Content content, bool createDefaultField)
        {
            var binding = content.WorkflowBinding;

            FieldRepository.ChangeCreateFieldsTriggerState(false);
            DefaultRepository.TurnIdentityInsertOn(EntityTypeCode.Content, content);
            var newContent = DefaultRepository.Save<Content, ContentDAL>(content);
            DefaultRepository.TurnIdentityInsertOff(EntityTypeCode.Content);
            FieldRepository.ChangeCreateFieldsTriggerState(true);

            if (createDefaultField)
            {
                CreateDefaultField(newContent, content.ForceFieldIds);
            }

            binding.SetContent(newContent);
            WorkflowRepository.UpdateContentWorkflowBind(binding);
            return GetById(newContent.Id);
        }

        private static void CreateDefaultField(Content newContent, int[] forceFieldIds)
        {
            var field = new Field(newContent).Init();

            field.ExactType = FieldExactTypes.String;
            field.Name = "Title";
            field.Description = "&nbsp;";
            field.Indexed = true;
            field.ViewInList = true;
            field.StringSize = 255;
            field.Order = 1;
            if (forceFieldIds != null && forceFieldIds.Length > 0)
                field.ForceId = forceFieldIds[0];
            field.PersistWithVirtualRebuild(true);
        }

        /// <summary>
        /// Обновляет информацию о контенте
        /// </summary>
        /// <param name="content">информация о контенте</param>
        /// <returns>информация о контенте</returns>
        internal static Content Update(Content content)
        {
            var binding = content.WorkflowBinding;
            var newContent = DefaultRepository.Update<Content, ContentDAL>(content);
            binding.SetContent(newContent);
            WorkflowRepository.UpdateContentWorkflowBind(binding);

            // Обновить свойства у агрегированных контентов
            foreach (var aggregated in content.AggregatedContents)
            {
                var localAggregated = aggregated;
                localAggregated.AutoArchive = content.AutoArchive;
                localAggregated = DefaultRepository.Update<Content, ContentDAL>(localAggregated);

                binding.SetContent(localAggregated);
                WorkflowRepository.UpdateContentWorkflowBind(binding);
            }

            return GetById(newContent.Id);
        }

        /// <summary>
        /// Удаляет контент по его идентификатору
        /// </summary>
        /// <param name="id">идентификатор контента</param>
        internal static void Delete(int id)
        {
            DefaultRepository.Delete<ContentDAL>(id);
        }

        /// <summary>
        /// Удаляет контенты
        /// </summary>
        /// <param name="IDs">идентификаторы контентов</param>
        internal static void MultipleDelete(int[] IDs)
        {
            if (IDs.Length > 0)
            {
                DefaultRepository.Delete<ContentDAL>(IDs);
            }
        }

        /// <summary>
        /// Копирует контент
        /// </summary>
        /// <param name="content">контент</param>
        internal static Content Copy(Content content, int? forceId, int[] forceFieldIds, int[] forceLinkIds, bool forHierarchy)
        {
            var oldId = content.Id;
            content.LoadWorkflowBinding();
            content.Id = 0;
            if (forceId.HasValue)
                content.ForceId = forceId.Value;
            if (!forHierarchy)
            {
                content.MutateNames();
            }
            var newContent = Save(content, false);
            var newId = newContent.Id;
            var helper = new ContentCopyHelper(oldId, newId, forHierarchy)
            {
                ForceIds = forceFieldIds,
                ForceLinkIds = forceLinkIds
            };

            helper.Proceed();
            return GetById(newId);
        }
        #endregion

        #region content groups
        /// <summary>
        /// Получить список групп контентов для сайта
        /// </summary>
        /// <param name="siteId"></param>
        /// <returns></returns>
        public static IEnumerable<ContentGroup> GetSiteContentGroups(int siteId)
        {
            var defaultGroupId = GetDefaultGroupId(siteId);
            var result = MappersRepository.ContentGroupMapper.GetBizList(QPContext.EFContext.ContentGroupSet.Where(g => g.SiteId == siteId).ToList());

            foreach (var resultItem in result)
            {
                if (resultItem.Id == defaultGroupId)
                {
                    resultItem.Name = Translator.Translate(resultItem.Name);
                }
            }

            return result.OrderBy(g => g.Name).ToArray();
        }

        internal static ContentGroup GetGroupById(int id)
        {
            return MappersRepository.ContentGroupMapper.GetBizObject(DefaultRepository.GetById<ContentGroupDAL>(id));
        }

        internal static ContentGroup GetContentGroup(int contentId)
        {
            var content = QPContext.EFContext.ContentSet.Include("Group").SingleOrDefault(n => n.Id == contentId);
            return content != null ? MappersRepository.ContentGroupMapper.GetBizObject(content.Group) : null;
        }

        internal static ContentGroup SaveGroup(ContentGroup group)
        {
            var dal = MappersRepository.ContentGroupMapper.GetDalObject(group);
            DefaultRepository.TurnIdentityInsertOn(EntityTypeCode.ContentGroup, group);
            if (group.ForceId != 0)
                dal.Id = group.ForceId;
            var newDal = DefaultRepository.SimpleSave(dal);
            DefaultRepository.TurnIdentityInsertOff(EntityTypeCode.ContentGroup);
            return MappersRepository.ContentGroupMapper.GetBizObject(newDal);
        }

        internal static ContentGroup UpdateGroup(ContentGroup group)
        {
            var dal = MappersRepository.ContentGroupMapper.GetDalObject(group);
            var newDal = DefaultRepository.SimpleUpdate(dal);
            return MappersRepository.ContentGroupMapper.GetBizObject(newDal);
        }

        #endregion

        #region content links

        internal static ContentLink GetContentLinkById(int linkId)
        {
            return MappersRepository.ContentLinkMapper.GetBizObject(
                QPContext.EFContext.ContentToContentSet
                .SingleOrDefault(n => n.LinkId == linkId)
            );
        }

        internal static List<ContentLink> GetContentLinks(int contentId)
        {
            return MappersRepository.ContentLinkMapper.GetBizList(QPContext.EFContext.ContentToContentSet.Where(n => n.LContentId == contentId || n.RContentId == contentId).OrderBy(n => n.LinkId).ToList());
        }

        internal static ContentLink SaveLink(ContentLink link)
        {
            EntityObject.VerifyIdentityInserting(EntityTypeCode.ContentLink, link.LinkId, link.ForceLinkId);

            if (link.ForceLinkId != 0)
                link.LinkId = link.ForceLinkId;

            DefaultRepository.TurnIdentityInsertOn(EntityTypeCode.ContentLink);

            var result = MappersRepository.ContentLinkMapper.GetBizObject(
                DefaultRepository.SimpleSave(
                    MappersRepository.ContentLinkMapper.GetDalObject(link)
                )
            );

            DefaultRepository.TurnIdentityInsertOff(EntityTypeCode.ContentLink);

            result.WasNew = true;

            return result;

        }

        internal static ContentLink UpdateLink(ContentLink link)
        {
            var result = MappersRepository.ContentLinkMapper.GetBizObject(DefaultRepository.SimpleUpdate(MappersRepository.ContentLinkMapper.GetDalObject(link)));
            result.WasNew = false;
            return result;
        }


        #endregion

        #region check existence

        internal static bool Exists(int id)
        {
            return QPContext.EFContext.ContentSet.Any(n => n.Id == id);
        }

        internal static bool GroupExists(int id)
        {
            return QPContext.EFContext.ContentGroupSet.Any(n => n.Id == id);
        }

        internal static bool NameExists(Content content)
        {
            return QPContext.EFContext.ContentSet
                .Any(n => n.Name == content.Name && n.Id != content.Id && n.SiteId == content.SiteId);
        }

        private static bool ContentNetNameExists(Content content)
        {
            return
                QPContext.EFContext.ContentSet
                    .Where(n => n.NetName == content.NetName && n.Id != content.Id)
                    .Any(GetScopeExpression(content));
        }

        private static bool LinkNetNameExists(Content content)
        {
            return
                QPContext.EFContext.ContentToContentSet
                    .Include("Content")
                    .Where(n => n.NetLinkName == content.NetName)
                    .Any(GetLinkScopeExpression(content));
        }

        internal static bool NetNameExists(Content content)
        {
            return ContentNetNameExists(content) || LinkNetNameExists(content);
        }

        internal static bool NetPluralNameExists(Content content)
        {
            return ContentNetPluralNameExists(content) || LinkNetPluralNameExists(content);
        }

        private static bool ContentNetPluralNameExists(Content content)
        {
            return QPContext.EFContext.ContentSet
                .Where(n => n.NetPluralName == content.NetPluralName && n.Id != content.Id)
                .Any(GetScopeExpression(content));
        }

        private static bool LinkNetPluralNameExists(Content content)
        {
            return
                QPContext.EFContext.ContentToContentSet
                    .Include("Content")
                    .Where(n => n.NetPluralLinkName == content.NetPluralName)
                    .Any(GetLinkScopeExpression(content));
        }

        internal static bool ContextClassNameExists(Content content)
        {
            return content.AdditionalContextClassName == content.Site.FullyQualifiedContextClassName;
        }

        internal static bool GroupNameExists(ContentGroup group)
        {
            return QPContext.EFContext.ContentGroupSet
                .Any(n => n.Name == group.Name && n.Id != group.Id && n.SiteId == group.Id);
        }

        private static Expression<Func<ContentDAL, bool>> GetScopeExpression(Content content)
        {
            Expression<Func<ContentDAL, bool>> scopeExpression = n => n.SiteId == content.SiteId;
            return scopeExpression;
        }

        private static Expression<Func<ContentToContentDAL, bool>> GetLinkScopeExpression(Content content)
        {
            Expression<Func<ContentToContentDAL, bool>> scopeExpression = n => n.Content.SiteId == content.SiteId;
            return scopeExpression;
        }
        #endregion

        internal static IEnumerable<ListItem> GetArticleTitleList(int contentId, string titleName, string filterIds)
        {
            using (new QPConnectionScope())
            {
                var rows = Common.GetArticleTitleList(
                    QPConnectionScope.Current.DbConnection,
                    contentId,
                    string.IsNullOrEmpty(titleName) ? GetTitleName(contentId) : titleName,
                    filterIds
                );
                return rows.Select(n => new ListItem(n["id"].ToString(), n["title"].ToString()));
            }
        }

        internal static Field GetTitleField(int contentId)
        {
            return FieldRepository.GetByName(contentId, GetTitleName(contentId));
        }

        internal static string GetTitleName(int contentId)
        {
            using (new QPConnectionScope())
            {
                return Common.GetTitleName(QPConnectionScope.Current.DbConnection, contentId);
            }
        }

        internal static IEnumerable<int> GetDisplayFieldIds(int contentId, bool withRelations = false, int excludeId = 0)
        {
            using (new QPConnectionScope())
            {
                IEnumerable<DataRow> rows = Common.GetDisplayFields(QPConnectionScope.Current.DbConnection, contentId, withRelations).AsEnumerable();
                return rows
                    .Select(n =>
                        new
                        {
                            id = Converter.ToInt32(n.Field<decimal>("ATTRIBUTE_ID")),
                            viewInList = n.Field<bool>("view_in_list"),
                            priority = n.Field<int>("attribute_priority"),
                            order = n.Field<decimal>("attribute_order")
                        })
                    .Where(n => n.id != excludeId)
                    .OrderByDescending(n => n.viewInList)
                    .OrderByDescending(n => n.priority) //TODO: check then by
                    .OrderBy(n => n.order)
                    .Select(n => n.id);
            }
        }

        internal static IEnumerable<Field> GetDisplayFields(int contentId, Field field = null)
        {
            var excludeId = field != null && field.ExactType == FieldExactTypes.M2ORelation ? field.BackRelationId.Value : 0;

            var fields = field == null || field.ListFieldTitleCount <= 1 && field.ExactType == FieldExactTypes.O2MRelation || field.ListFieldTitleCount <= 0 ? null : GetDisplayFieldIds(contentId, field.IncludeRelationsInTitle, excludeId)
                .Take(field.ListFieldTitleCount)
                .Select(FieldRepository.GetById);

            var displayField = field != null && field.Relation != null && field.ExactType == FieldExactTypes.O2MRelation ?
                field.Relation :
                GetTitleField(contentId);

            return fields ?? new[] { displayField };
        }

        internal static int GetLinkedContentId(int contentId, int linkId)
        {
            var link = GetContentLinkById(linkId);
            var linkedContentId = 0;
            if (link.LContentId == contentId)
                linkedContentId = link.RContentId;
            else if (link.RContentId == contentId)
                linkedContentId = link.LContentId;
            return linkedContentId;
        }

        internal static bool HasVariationField(int contentId, int exceptId = 0)
        {
            return QPContext.EFContext.FieldSet.Any(n => n.ContentId == contentId && n.UseForVariations && n.Id != exceptId);
        }

        internal static int GetVariationFieldId(int contentId, int exceptId = 0)
        {
            return QPContext.EFContext.FieldSet
                .Where(n => n.ContentId == contentId && n.UseForVariations && n.Id != exceptId)
                .Select(n => (int)n.Id)
                .SingleOrDefault();
        }

        internal static bool HasContentTreeField(int contentId, int exceptId = 0)
        {
            return QPContext.EFContext.FieldSet.Any(n => n.ContentId == contentId && n.UseForTree && n.Id != exceptId);
        }

        internal static int GetTreeFieldId(int contentId, int exceptId = 0)
        {
            return QPContext.EFContext.FieldSet
                .Where(n => n.ContentId == contentId && n.UseForTree && n.Id != exceptId)
                .Select(n => (int)n.Id)
                .FirstOrDefault();
        }

        internal static string GetTreeFieldName(int contentId, int exceptId = 0)
        {
            var treeId = GetTreeFieldId(contentId, exceptId);
            return treeId != 0 ? FieldRepository.GetById(treeId).Name : string.Empty;
        }

        internal static int GetOldStyleSelfRelationFieldId(int contentId)
        {
            using (new QPConnectionScope())
            {
                return Common.GetSelfRelationFieldId(QPConnectionScope.Current.DbConnection, contentId);
            }
        }

        internal static bool HasNotifications(int id, string code)
        {
            Expression<Func<NotificationsDAL, bool>> expression;
            switch (code)
            {
                case NotificationCode.Create:
                    expression = n => n.ForCreate;
                    break;
                case NotificationCode.Update:
                    expression = n => n.ForModify;
                    break;
                case NotificationCode.Delete:
                    expression = n => n.ForRemove;
                    break;
                case NotificationCode.ChangeStatus:
                    expression = n => n != null && (bool)n.ForStatusChanged;
                    break;
                case NotificationCode.Custom:
                    expression = n => n != null && (bool)n.ForFrontend;
                    break;
                default:
                    expression = n => false;
                    break;
            }

            return QPContext.EFContext.NotificationsSet.Where(n => n.ContentId == id).Any(expression);
        }

        internal static bool HasAnyNotifications(int contentId)
        {
            return QPContext.EFContext.NotificationsSet.Any(n => n.ContentId == contentId);
        }

        /// <summary>
        /// Вернуть список контентов доступных для связи с текущим контентом
        /// </summary>
        /// <param name="contentId"></param>
        /// <returns></returns>
        internal static IEnumerable<ListItem> GetAcceptableContentForRelation(int contentId)
        {
            var content = QPContext.EFContext.ContentSet.SingleOrDefault(n => n.Id == contentId);
            if (content == null)
                return Enumerable.Empty<ListItem>();

            return QPContext.EFContext.ContentSet
                .Where(c => c.VirtualType != VirtualType.Join && c.SiteId == content.SiteId)
                .Select(c => new { c.Id, Text = c.Name })
                .ToArray()
                .OrderBy(c => c.Text, StringComparer.InvariantCultureIgnoreCase)
                .Select(c => new ListItem { Value = c.Id.ToString(), Text = c.Text })
                .ToArray();
        }

        internal static void CopyAccess(int oldId, int newId)
        {
            using (new QPConnectionScope())
            {
                Common.CopyContentAccess(QPConnectionScope.Current.DbConnection, oldId, newId, QPContext.CurrentUserId);
            }
        }


        /// <summary>
        /// Переключает RelationId равный currentRelationFieldId на значение newRelationFieldId
        /// </summary>
        /// <param name="currentRelationFieldId"></param>
        /// <param name="newRelationFieldId"></param>
        internal static void ChangeRelationIdToNewOne(int currentRelationFieldId, int newRelationFieldId)
        {
            using (new QPConnectionScope())
            {
                Common.ChangeRelationIdToNewOne(QPConnectionScope.Current.DbConnection, currentRelationFieldId, newRelationFieldId);
            }
        }

        /// <summary>
        /// Возвращает контенты у которых хотя бы одно поле ссылается на контент через M2M
        /// </summary>
        public static IEnumerable<Content> GetRelatedM2MContents(Content content)
        {
            if (!content.IsNew)
            {
                var externalLinks = QPContext.EFContext.ContentToContentSet
                    .Where(l => l.LContentId == content.Id && l.RContentId != content.Id || l.RContentId == content.Id && l.LContentId != content.Id)
                    .Select(l => l.LinkId)
                    .ToArray();

                IEnumerable<int> relatedM2MContentsIDs = QPContext.EFContext.FieldSet
                    .Where(c => c.LinkId.HasValue && externalLinks.Contains(c.LinkId.Value))
                    .Select(c => c.ContentId)
                    .Where(c => c != content.Id)
                    .Distinct()
                    .ToArray()
                    .Select(Convert.ToInt32)
                    .ToArray();

                return GetList(relatedM2MContentsIDs);
            }

            return new Content[0];
        }

        public static IEnumerable<Field> GetRelatedM2MFields(int contentId)
        {
            var externalLinks = QPContext.EFContext.ContentToContentSet
                .Where(l => l.LContentId == contentId && l.RContentId != contentId || l.RContentId == contentId && l.LContentId != contentId)
                .Select(l => l.LinkId)
                .ToArray();

            var fieldIds = QPContext.EFContext.FieldSet
                    .Where(c => c.LinkId.HasValue && externalLinks.Contains(c.LinkId.Value))
                    .Where(c => c.ContentId != contentId)
                    .Select(c => (int)c.Id)
                    .ToArray();

            return FieldRepository.GetList(fieldIds);
        }

        /// <summary>
        /// Возвращает контенты у которых хотя бы одно поле ссылается на контент через O2M
        /// </summary>
        public static IEnumerable<Content> GetRelatedO2MContents(Content content)
        {
            if (!content.IsNew)
            {
                var context = QPContext.EFContext;
                var contentDal = (from f1 in context.FieldSet
                                  join f2 in context.FieldSet on f1.Id equals f2.RelationId
                                  where f1.ContentId == content.Id && f2.ContentId != content.Id
                                  select f2.Content).ToList();

                return MappersRepository.ContentMapper.GetBizList(contentDal)
                    .Distinct(new LambdaEqualityComparer<Content>((c1, c2) => c1.Id == c2.Id, c => c.Id))
                    .ToArray();
            }

            return new Content[0];
        }


        public static IEnumerable<Field> GetRelatedO2MFields(int contentId)
        {
            var context = QPContext.EFContext;
            var fieldIds = (from f1 in context.FieldSet
                            join f2 in context.FieldSet on f1.Id equals f2.RelationId
                            where f1.ContentId == contentId && f2.ContentId != contentId
                            select (int)f2.Id).ToArray();

            return FieldRepository.GetList(fieldIds);
        }

        public static IEnumerable<Field> GetRelatedM2OFields(int contentId)
        {
            var context = QPContext.EFContext;
            var fieldIds = (from f1 in context.FieldSet
                            join f2 in context.FieldSet on f1.Id equals f2.BackRelationId
                            where f1.ContentId == contentId && f2.ContentId != contentId
                            select (int)f2.Id).ToArray();

            return FieldRepository.GetList(fieldIds);
        }

        /// <summary>
        /// Возвращает список имен контентов сайта, которые являются базовыми для union-контентов други сайтов
        /// </summary>
        /// <param name="siteId"></param>
        /// <returns></returns>
        internal static IEnumerable<string> GetSharedUnionBaseContentNames(int siteId)
        {
            using (new QPConnectionScope())
            {
                var rows = Common.GetSharedUnionBaseContentInfo(siteId, QPConnectionScope.Current.DbConnection);
                return rows.Select(r => r.Field<string>("BASE_CONTENT_NAME")).ToArray();
            }
        }

        /// <summary>
        /// Возвращает имен список контентов сайта, на которые есть O2M-ссылки из контентов других сайтов
        /// </summary>
        internal static IEnumerable<string> GetSharedRelatedContentNames(int siteId)
        {
            using (new QPConnectionScope())
            {
                var rows = Common.GetSharedRelatedContentInfo(siteId, QPConnectionScope.Current.DbConnection);
                return rows.Select(r => r.Field<string>("CONTENT_NAME")).ToArray();
            }
        }

        internal static IEnumerable<int> BatchRemoveContents(int siteId, int contentsToRemove)
        {
            using (var scope = new QPConnectionScope())
            {
                return Common.RemovingActions_BatchRemoveContents(siteId, contentsToRemove, scope.DbConnection);
            }
        }

        /// <summary>
        /// Получить список дочерних виртуальных контентов
        /// </summary>
        /// <param name="contentId"></param>
        /// <returns></returns>
        public static IEnumerable<Content> GetVirtualSubContents(int contentId)
        {
            if (contentId > 0)
            {
                IEnumerable<int> subContentIDs;
                using (var scope = new QPConnectionScope())
                {
                    subContentIDs = Common.GetVirtualSubContentIDs(scope.DbConnection, contentId);
                }

                return GetList(subContentIDs);
            }

            return Enumerable.Empty<Content>();
        }

        public static IEnumerable<Content> GetAggregatedContents(int contentId)
        {
            if (contentId > 0)
            {
                return MappersRepository.ContentMapper.GetBizList(QPContext.EFContext.FieldSet
                    .Where(f => f.Classifier.ContentId == contentId)
                    .Select(f => f.Content)
                    .ToList()
                );
            }

            return Enumerable.Empty<Content>();
        }

        internal static IEnumerable<ListItem> GetGroupSimpleList(int siteId, int[] selectedIds = null)
        {
            var groups = GetSiteContentGroups(siteId).Select(g => new ListItem(g.Id.ToString(), g.Name));
            if (selectedIds != null)
            {
                foreach (var @group in groups.Where(@group => selectedIds.Contains(int.Parse(@group.Value))))
                {
                    @group.Selected = true;
                }
            }

            return groups;
        }

        /// <summary>
        /// Есть ли статьи у контента ?
        /// </summary>
        /// <param name="contentId"></param>
        /// <returns></returns>
        internal static bool IsAnyArticle(int contentId)
        {
            return QPContext.EFContext.ArticleSet.Any(a => a.ContentId == contentId);
        }

        /// <summary>
        /// Есть ли у контента агрегированные статьи
        /// </summary>
        /// <returns></returns>
        internal static bool IsAnyAggregatedFields(int contentId)
        {
            return QPContext.EFContext.FieldSet.Any(f => f.ContentId == contentId && f.Aggregated);
        }

        /// <summary>
        /// Есть ли у контенты с агрегированными статьями
        /// </summary>
        /// <returns></returns>
        internal static IEnumerable<int> ContentsWithAggregatedFields(IEnumerable<int> contentIDs)
        {
            var ids = Converter.ToDecimalCollection(contentIDs);
            return QPContext.EFContext.ContentSet
                .Where(c => ids.Contains(c.Id) && c.Fields.Any(f => f.Aggregated))
                .Select(c => c.Id)
                .ToArray()
                .Select(id => Converter.ToInt32(id));
        }

        internal static bool IsArticlePermissionsAllowed(int contentId)
        {
            using (var scope = new QPConnectionScope())
            {
                return Common.IsArticlePermissionsAllowed(scope.DbConnection, contentId);
            }
        }

        internal static void CopyCustomActions(int sourceId, int destinationId)
        {
            using (var scope = new QPConnectionScope())
            {
                Common.CopyContentCustomActions(scope.DbConnection, sourceId, destinationId);
            }
        }

        internal static int GetDefaultGroupId(int siteId)
        {
            using (var scope = new QPConnectionScope())
            {
                return Common.GetDefaultGroupId(scope.DbConnection, siteId);
            }
        }

        internal static void UpdateContentModification(int contentId)
        {
            using (var scope = new QPConnectionScope())
            {
                Common.UpdateContentModification(scope.DbConnection, contentId);
            }
        }

        internal static void CopyContentWorkflowBind(int sourceSiteId, int destinationSiteId, string relationsBetweenContentsXml)
        {
            using (new QPConnectionScope())
            {
                Common.CopyContentWorkflowBind(QPConnectionScope.Current.DbConnection, sourceSiteId, destinationSiteId, relationsBetweenContentsXml);
            }
        }

        internal static IEnumerable<DataRow> GetRelationsBetweenContents(int sourceSiteId, int destinationSiteId, string newContentIds)
        {
            using (new QPConnectionScope())
            {
                return Common.GetRelationsBetweenContents(QPConnectionScope.Current.DbConnection, sourceSiteId, destinationSiteId, newContentIds);
            }
        }

        internal static string GetRelationsBetweenContentsXML(int sourceSiteId, int destinationSiteId, string newContentIds)
        {
            using (new QPConnectionScope())
            {
                var rows = Common.GetRelationsBetweenContents(QPConnectionScope.Current.DbConnection, sourceSiteId, destinationSiteId, newContentIds);
                return MultistepActionHelper.GetXmlFromDataRows(rows, "content");
            }
        }

        internal static void CopyContentItemAccess(string relationsBetweenItemsXml)
        {
            using (new QPConnectionScope())
            {
                Common.CopyContentItemAccess(QPConnectionScope.Current.DbConnection, relationsBetweenItemsXml);
            }
        }

        internal static void CopyContentConstraints(string relationsBetweenContentsXml, ref string result)
        {
            using (new QPConnectionScope())
            {
                var rows = Common.CopyContentConstraints(QPConnectionScope.Current.DbConnection, relationsBetweenContentsXml);
                result = MultistepActionHelper.GetXmlFromDataRows(rows, "constraint");
            }
        }

        internal static int CopyContents(int oldSiteId, int newSiteId, int startFrom, int endOn, ref string result)
        {
            using (new QPConnectionScope())
            {
                var rows = Common.CopyContents(QPConnectionScope.Current.DbConnection, oldSiteId, newSiteId, startFrom, endOn).ToList();
                result = string.Join(",", rows.Select(r => r.Field<int>("content_id")));
                return rows.Count;
            }
        }
        internal static IEnumerable<Content> GetChildList(int contentId)
        {
            return MappersRepository.ContentMapper.GetBizList(
                QPContext.EFContext.ContentSet
                    .Where(f => f.ParentContentId == contentId)
                    .ToList()
            );
        }

        public static IEnumerable<DataRow> CopyContentItems(int sourceSiteId, int destinationSiteId, string contentsToCopy, int startFrom, int endBy, string relationsBetweenContentsXml, string relationsBetweenStatusesXml)
        {
            using (new QPConnectionScope())
            {
                return Common.CopyContentItems(QPConnectionScope.Current.DbConnection, sourceSiteId, destinationSiteId, contentsToCopy, startFrom, endBy, relationsBetweenContentsXml, relationsBetweenStatusesXml);
            }
        }

        public static void UpdateContentData(string relationsBetweenAttributesXml, string relationsNewContentItemsIdXml)
        {
            using (new QPConnectionScope())
            {
                Common.UpdateContentData(QPConnectionScope.Current.DbConnection, relationsBetweenAttributesXml, relationsNewContentItemsIdXml);
            }
        }

        public static string GetRelationsBetweenStatuses(int sourceSiteId, int destinationSiteId)
        {
            using (new QPConnectionScope())
            {
                var rows = Common.GetRelationsBetweenStatuses(QPConnectionScope.Current.DbConnection, sourceSiteId, destinationSiteId);
                return MultistepActionHelper.GetXmlFromDataRows(rows, "status_type");
            }
        }

        internal static void CopyContentItemSchedule(string relationsNewContentItemsIdXml)
        {
            using (new QPConnectionScope())
            {
                Common.CopyContentItemSchedule(QPConnectionScope.Current.DbConnection, relationsNewContentItemsIdXml);
            }
        }

        internal static int GetArticlesCountOnSite(int siteId)
        {
            using (new QPConnectionScope())
            {
                return Common.GetArticlesCountInSite(QPConnectionScope.Current.DbConnection, siteId);
            }
        }

        internal static void CopyUnionContents(int sourceSiteId, int destinationSiteId, string newContentIds)
        {
            using (new QPConnectionScope())
            {
                Common.CopyUnionContents(QPConnectionScope.Current.DbConnection, sourceSiteId, destinationSiteId, newContentIds);
            }
        }

        internal static void UpdateVirtualContentAttributes(int sourceSiteId, int destinationSiteId)
        {
            using (new QPConnectionScope())
            {
                Common.UpdateVirtualContentAttributes(QPConnectionScope.Current.DbConnection, sourceSiteId, destinationSiteId);
            }
        }

        internal static IEnumerable<DataRow> CopyVirtualContents(int sourceSiteId, int destinationSiteId)
        {
            using (new QPConnectionScope())
            {
                return Common.CopyVirtualContents(QPConnectionScope.Current.DbConnection, sourceSiteId, destinationSiteId);
            }
        }

        internal static void UpdateVirtualContent(string newSqlQuery, int newContentId)
        {
            using (new QPConnectionScope())
            {
                Common.UpdateVirtualContent(QPConnectionScope.Current.DbConnection, newSqlQuery, newContentId);
            }
        }

        internal static void UpdateAttributesAfterCopyingArticles(int destinationSiteId, string relationsBetweenARticles)
        {
            using (new QPConnectionScope())
            {
                Common.UpdateAttributesAfterCopyingArticles(QPConnectionScope.Current.DbConnection, destinationSiteId, relationsBetweenARticles);
            }
        }

        internal static void UpdateContentsParentContentId(int destinationSiteId, string relationsBetweenContentsXml)
        {
            using (new QPConnectionScope())
            {
                Common.UpdateContentsParentContentId(QPConnectionScope.Current.DbConnection, destinationSiteId, relationsBetweenContentsXml);
            }
        }

        internal static void CopyContentAccess(int destinationSiteId, string relationsBetweenContentsXml)
        {
            using (new QPConnectionScope())
            {
                Common.CopyContentAccess(QPConnectionScope.Current.DbConnection, destinationSiteId, relationsBetweenContentsXml);
            }
        }

        internal static void CopyContentsCustomActions(string relationsBetweenContentsXml)
        {
            using (new QPConnectionScope())
            {
                Common.CopyContentsCustomActions(QPConnectionScope.Current.DbConnection, relationsBetweenContentsXml);
            }
        }

        internal static void CopyContentFolders(string relationsBetweenContentsXml)
        {
            using (new QPConnectionScope())
            {
                Common.CopyContentFolders(QPConnectionScope.Current.DbConnection, relationsBetweenContentsXml);
            }
        }

        internal static void UpdateContentFolders(int sourceSiteId, int destinationSiteId, string relationsBetweenContentsXml)
        {
            using (new QPConnectionScope())
            {
                Common.UpdateContentFolders(QPConnectionScope.Current.DbConnection, sourceSiteId, destinationSiteId, relationsBetweenContentsXml);
            }
        }

        internal static void CopyContentLinks(int sourceSiteId, int destinationSiteId, ref string result)
        {
            using (new QPConnectionScope())
            {
                var rows = Common.CopyContentLinks(QPConnectionScope.Current.DbConnection, sourceSiteId, destinationSiteId);
                result = MultistepActionHelper.GetXmlFromDataRows(rows, "oldlink", "newlink");
            }
        }

        internal static int GetArticlesCountToCopy(int noMoreThanNArticles, int siteId)
        {
            using (new QPConnectionScope())
            {
                return Common.GetArticlesCountToCopy(QPConnectionScope.Current.DbConnection, noMoreThanNArticles, siteId);
            }
        }

        internal static string GetContentIdsToCopy(int noMoreThanNArticles, int sourceSiteId)
        {
            using (new QPConnectionScope())
            {
                return Common.GetContentIdsToCopy(QPConnectionScope.Current.DbConnection, noMoreThanNArticles, sourceSiteId);
            }
        }
        internal static string GetContentIdsBySiteId(int sourceSiteId)
        {
            using (new QPConnectionScope())
            {
                return Common.GetContentIdsBySiteId(QPConnectionScope.Current.DbConnection, sourceSiteId);
            }
        }
        internal static void CopyContentsGroups(int sourceSiteId, int destinationSiteId)
        {
            using (new QPConnectionScope())
            {
                Common.CopyContentsGroups(QPConnectionScope.Current.DbConnection, sourceSiteId, destinationSiteId);
            }
        }
        internal static void UpdateContentGroupIds(int sourceSiteId, int destinationSiteId)
        {
            using (new QPConnectionScope())
            {
                Common.UpdateContentGroupIds(QPConnectionScope.Current.DbConnection, sourceSiteId, destinationSiteId);
            }
        }

        internal static void UpdateO2MValues(int sourceSiteId, int destinationSiteId, string relationsBetweenArticles)
        {
            using (new QPConnectionScope())
            {
                Common.UpdateO2MValues(QPConnectionScope.Current.DbConnection, sourceSiteId, destinationSiteId, relationsBetweenArticles);
            }
        }

        internal static void CopyArticleWorkflowBind(int sourceSiteId, int destinationSiteId, string relationsBetweenArticles)
        {
            using (new QPConnectionScope())
            {
                Common.CopyArticleWorkflowBind(QPConnectionScope.Current.DbConnection, sourceSiteId, destinationSiteId, relationsBetweenArticles);
            }
        }
        internal static void UpdateContentDataAfterCopyingArticles(string relationsBetweenArticles, string relationsBetweenLinks)
        {
            using (new QPConnectionScope())
            {
                Common.UpdateContentDataAfterCopyingArticles(QPConnectionScope.Current.DbConnection, relationsBetweenArticles, relationsBetweenLinks);
            }
        }
        internal static void CopyItemToItems(string relationsBetweenArticles, string relationsBetweenLinks)
        {
            using (new QPConnectionScope())
            {
                Common.CopyItemToItems(QPConnectionScope.Current.DbConnection, relationsBetweenArticles, relationsBetweenLinks);
            }
        }
        internal static void UpdateItemToItem(string relationsBetweenArticles, string relationsBetweenLinks)
        {
            using (new QPConnectionScope())
            {
                Common.UpdateItemToItem(QPConnectionScope.Current.DbConnection, relationsBetweenArticles, relationsBetweenLinks);
            }
        }
        internal static string GetRelationsBetweenLinks(int sourceSiteId, int destinationSiteId)
        {
            using (new QPConnectionScope())
            {
                var rows = Common.GetRelationsBetweenLinks(QPConnectionScope.Current.DbConnection, sourceSiteId, destinationSiteId);
                return MultistepActionHelper.GetXmlFromDataRows(rows, "link");
            }
        }
        internal static void CopyUserQueryContents(string relationsBetweenContents)
        {
            using (new QPConnectionScope())
            {
                Common.CopyUserQueryContents(QPConnectionScope.Current.DbConnection, relationsBetweenContents);
            }
        }

        internal static void CopyUserQueryAttributes(string relationsBetweenContents, string relationsBetweenAttributes)
        {
            using (new QPConnectionScope())
            {
                Common.CopyUserQueryAttributes(QPConnectionScope.Current.DbConnection, relationsBetweenContents, relationsBetweenAttributes);
            }
        }

        #region Aggregated content
        internal static int[] GetReferencedAggregatedContentIds(int contentId, int[] articleIds)
        {
            using (var scope = new QPConnectionScope())
            {
                return Common.GetReferencedAggregatedContentIds(scope.DbConnection, contentId, articleIds);
            }
        }

        internal static int[] GetReferencedAggregatedContentIds(int contentId)
        {
            using (var scope = new QPConnectionScope())
            {
                return Common.GetReferencedAggregatedContentIds(scope.DbConnection, contentId);
            }
        }

        internal static Dictionary<int, Dictionary<int, int>> GetAggregatedArticleIdsMap(int contentId, int[] articleIds)
        {
            using (var scope = new QPConnectionScope())
            {
                return Common.GetAggregatedArticleIdsMap(scope.DbConnection, contentId, articleIds);
            }
        }
        #endregion
    }
}
