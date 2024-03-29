﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using QP8.Plugins.Contract;
using Quantumart.QP8.BLL.Facades;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL.Repository.FieldRepositories;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Constants.DbColumns;
using Quantumart.QP8.DAL;
using Quantumart.QP8.DAL.DTO;
using Quantumart.QP8.DAL.Entities;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.BLL.Repository.ContentRepositories
{
    public class ContentRepository : IContentRepository
    {
        Content IContentRepository.GetById(int id) => GetById(id);

        ContentLink IContentRepository.GetContentLinkById(int linkId)
        {
            return GetBll(QPContext.EFContext.ContentToContentSet.SingleOrDefault(n => n.LinkId == linkId));
        }

        bool IContentRepository.IsAnyArticle(int contentId)
        {
            return QPContext.EFContext.ArticleSet.Any(a => a.ContentId == contentId);
        }

        int IContentRepository.CountArticles(int contentId)
        {
            return QPContext.EFContext.ArticleSet.Count(a => a.ContentId == contentId);
        }

        string IContentRepository.GetTreeFieldName(int contentId, int exceptId)
        {
            var treeId = GetTreeFieldId(contentId, exceptId);
            return treeId != 0 ? FieldRepository.GetById(treeId).Name : string.Empty;
        }

        IEnumerable<int> IContentRepository.GetDisplayFieldIds(int contentId, bool withRelations, int excludeId)
        {
            var fields = GetDisplayFields(contentId, withRelations);
            return fields
                .Where(x => x.Id != excludeId)
                .OrderByDescending(x => x.ViewInList)
                .ThenByDescending(x => x.FieldPriority(withRelations))
                .ThenBy(x => x.Order)
                .Select(x => x.Id);
        }

        internal static IEnumerable<Field> GetDisplayFields(int contentId, bool withRelations)
        {
            var dbFields = QPContext
                .EFContext
                .FieldSet
                .Where(x => x.ContentId == contentId)
                .ToList();

            return MapperFacade.FieldMapper.GetBizList(dbFields).Where(x => x.FieldPriority(withRelations) >= 0).ToList();
        }

        void IContentRepository.ChangeRelationIdToNewOne(int currentRelationFieldId, int newRelationFieldId)
        {
            using (new QPConnectionScope())
            {
                Common.ChangeRelationIdToNewOne(QPConnectionScope.Current.DbConnection, currentRelationFieldId, newRelationFieldId);
            }
        }

        public static List<QpPluginFieldValue> GetPluginValues(int contentId)
        {
            var actualValues = QPContext.EFContext.PluginFieldValueSet
                .Where(n => n.ContentId == contentId)
                .ToDictionary(k => (int)k.PluginFieldId, n => n.Value);

            var pluginDict = QpPluginRepository.GetQpFieldPluginDict();

            var pluginFieldValues = QpPluginRepository.GetPluginFields(QpPluginRelationType.Content)
                .Select(n => new QpPluginFieldValue()
                {
                    Field = n,
                    Plugin = pluginDict[n.Id],
                    Value = actualValues.TryGetValue(n.Id, out var result) ? result : String.Empty
                }).ToList();

            return pluginFieldValues;
        }

        private ContentToContentDAL GetDal(ContentLink bll)
        {
            return MapperFacade.ContentLinkMapper.GetDalObject(bll);
        }

        private ContentLink GetBll(ContentToContentDAL dal)
        {
            return MapperFacade.ContentLinkMapper.GetBizObject(dal);
        }

        ContentLink IContentRepository.SaveLink(ContentLink link)
        {
            EntityObject.VerifyIdentityInserting(EntityTypeCode.ContentLink, link.LinkId, link.ForceLinkId);
            if (link.ForceLinkId != 0)
            {
                link.LinkId = link.ForceLinkId;
            }
            using (var scope = new QPConnectionScope())
            {
                try
                {
                    if (QPContext.DatabaseType == DatabaseType.SqlServer)
                    {
                        DefaultRepository.TurnIdentityInsertOn(EntityTypeCode.ContentLink);
                        FieldRepository.ChangeInsertContentLinkTriggerState(scope.DbConnection, false);
                    }

                    var resultDal = DefaultRepository.SimpleSave(GetDal(link));
                    var result = GetBll(resultDal);
                    if (QPContext.DatabaseType == DatabaseType.Postgres)
                    {
                        Common.CreateLinkTables(scope.DbConnection, resultDal);
                    }
                    Common.CreateLinkView(scope.DbConnection, resultDal);

                    result.WasNew = true;
                    return result;

                }
                finally
                {
                    if (QPContext.DatabaseType == DatabaseType.SqlServer)
                    {
                        DefaultRepository.TurnIdentityInsertOff(EntityTypeCode.ContentLink);
                        FieldRepository.ChangeInsertContentLinkTriggerState(scope.DbConnection, true);
                    }
                }
            }

        }

        ContentLink IContentRepository.UpdateLink(ContentLink link)
        {
            var result = GetBll(DefaultRepository.SimpleUpdate(GetDal(link)));
            result.WasNew = false;
            return result;
        }

        internal static Content GetById(int id)
        {
            return GetByIdFromCache(id) ?? MapperFacade.ContentMapper.GetBizObject(QPContext.EFContext.ContentSet.Include("LastModifiedByUser").SingleOrDefault(n => n.Id == id));
        }

        internal static ListResult<ContentListItem> GetList(ContentListFilter filter, ListCommand cmd, int[] selectedContentIDs = null, int workflowId = 0)
        {
            using (var scope = new QPConnectionScope())
            {
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
                    LanguageId = QPContext.CurrentLanguageId,
                    CustomFilter = MapperFacade.CustomFilterMapper.GetDalList(filter?.CustomFilter?.ToList()).ToArray()
                };

                var rows = Common.GetContentsPage(scope.DbConnection, options, out var totalRecords);
                return new ListResult<ContentListItem> { Data = MapperFacade.ContentListItemRowMapper.GetBizList(rows.ToList()), TotalRecords = totalRecords };
            }
        }

        [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
        internal static IEnumerable<Content> GetList(IEnumerable<int> ids, bool loadSite = false, bool loadFields = false)
        {
            if (ids != null && ids.Any())
            {
                var context = QPContext.EFContext.ContentSet.AsQueryable();
                if (loadSite)
                {
                    context = context.Include("Site");
                }

                if (loadFields)
                {
                    context = context.Include("Fields");
                }

                var decIds = Converter.ToDecimalCollection(ids).Distinct().ToArray();
                return MapperFacade.ContentMapper.GetBizList(context.Where(c => decIds.Contains(c.Id)).ToList());
            }

            return Enumerable.Empty<Content>();
        }

        internal static IEnumerable<Content> GetAll() => MapperFacade.ContentMapper.GetBizList(QPContext.EFContext.ContentSet.ToList());

        internal static IEnumerable<Content> GetListBySiteId(int siteId)
        {
            return MapperFacade.ContentMapper.GetBizList(QPContext.EFContext.ContentSet.Include("WorkflowBinding").Where(c => c.SiteId == siteId).ToList());
        }

        internal static IEnumerable<ListItem> GetSimpleList(int currentSiteId, int id)
        {
            return GetListBySiteId(currentSiteId).Select(c => new ListItem(c.Id.ToString(), c.Name));
        }

        internal static IEnumerable<ListItem> GetSimpleList(int currentSiteId, IEnumerable<int> ids)
        {
            return GetList(ids, true).Select(c => new ListItem(c.Id.ToString(), c.SiteId == currentSiteId ? c.Name : $"{c.Site.Name}.{c.Name}"));
        }

        internal static int[] GetChangeDisabledIDs()
        {
            return Converter.ToInt32Collection(QPContext.EFContext.ContentSet.Where(c => c.DisableChangingActions).Select(c => c.Id)).ToArray();
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

        internal static Content GetByIdWithFields(int id)
        {
            return MapperFacade.ContentMapper.GetBizObject(QPContext.EFContext.ContentSet.Include("LastModifiedByUser").Include("Fields").SingleOrDefault(n => n.Id == id));
        }

        internal static int GetSiteId(int id)
        {
            return (int)QPContext.EFContext.ContentSet.Where(n => n.Id == (decimal)id).Select(n => n.SiteId).Single();
        }

        private static void ChangeCreateFieldsTriggerState(DbConnection cnn, bool enable)
        {
            Common.ChangeTriggerState(cnn, "ti_create_fields", enable);
        }

        private static void ChangeInsertAccessContentTriggerState(DbConnection cnn, bool enable)
        {
            Common.ChangeTriggerState(cnn, "ti_access_content", enable);
        }

        private static void ChangeInsertModificationTriggerState(DbConnection cnn, bool enable)
        {
            Common.ChangeTriggerState(cnn, "ti_insert_modify_row", enable);
        }

        private static void ChangeDropTableTriggerState(DbConnection cnn, bool enable)
        {
            Common.ChangeTriggerState(cnn, "td_drop_table", enable);
        }

        private static void ChangeCleanEmptyGropusTriggerState(DbConnection cnn, bool enable)
        {
            Common.ChangeTriggerState(cnn, "tiud_remove_empty_content_groups", enable);
        }

        internal static Content Save(Content content, bool createDefaultField)
        {
            using (var scope = new QPConnectionScope())
            {
                try
                {
                    if (QPContext.DatabaseType == DatabaseType.SqlServer)
                    {
                        ChangeCreateFieldsTriggerState(scope.DbConnection, false);
                        ChangeInsertAccessContentTriggerState(scope.DbConnection, false);
                        ChangeInsertModificationTriggerState(scope.DbConnection, false);
                        ChangeCleanEmptyGropusTriggerState(scope.DbConnection, false);
                        DefaultRepository.TurnIdentityInsertOn(EntityTypeCode.Content, content);
                    }

                    var binding = content.WorkflowBinding;
                    var fieldValues = content.QpPluginFieldValues;
                    var newContent = DefaultRepository.Save<Content, ContentDAL>(content);

                    if (QPContext.DatabaseType == DatabaseType.SqlServer)
                    {
                        DefaultRepository.TurnIdentityInsertOff(EntityTypeCode.Content);
                    }
                    Common.CreateContentTables(scope.DbConnection, newContent.Id, newContent.UseNativeEfTypes);
                    Common.CreateContentModification(scope.DbConnection, newContent.Id);
                    CommonSecurity.CreateContentAccess(scope.DbConnection, newContent.Id);

                    if (createDefaultField)
                    {
                        CreateDefaultField(newContent, content.ForceFieldIds); // implicitly create views
                    }

                    binding.SetContent(newContent);
                    WorkflowRepository.UpdateContentWorkflowBind(binding);
                    UpdatePluginValues(fieldValues, newContent.Id);
                    return GetById(newContent.Id);
                }
                finally
                {
                    if (QPContext.DatabaseType == DatabaseType.SqlServer)
                    {
                        DefaultRepository.TurnIdentityInsertOff(EntityTypeCode.Content);
                        ChangeCreateFieldsTriggerState(scope.DbConnection, true);
                        ChangeInsertAccessContentTriggerState(scope.DbConnection, true);
                        ChangeInsertModificationTriggerState(scope.DbConnection, true);
                        ChangeCleanEmptyGropusTriggerState(scope.DbConnection, true);
                    }
                }
            }
        }

        private static void CreateDefaultField(Content newContent, IReadOnlyList<int> forceFieldIds)
        {
            var field = Field.Create(newContent, new FieldRepository(), new ContentRepository());
            field.ExactType = FieldExactTypes.String;
            field.Name = "Title";
            field.Description = "Default field";
            field.Indexed = true;
            field.ViewInList = true;
            field.StringSize = 255;
            field.Order = 1;
            if (forceFieldIds != null && forceFieldIds.Count > 0)
            {
                field.ForceId = forceFieldIds[0];
            }

            field.PersistWithVirtualRebuild(true);
        }

        private static void DeleteEmptyContentGroups()
        {
            var entities = QPContext.EFContext;
            var emptyGroups = entities.ContentGroupSet.Where(
                n => !n.Contents.Any() && n.Name != ContentGroup.DefaultName
             ).ToArray();

            foreach (var emptyGroup in emptyGroups)
            {
                entities.Entry(emptyGroup).State = EntityState.Deleted;
            }

            entities.SaveChanges();
        }

        internal static Content Update(Content content)
        {
            using (var scope = new QPConnectionScope())
            {
                try
                {
                    if (QPContext.DatabaseType == DatabaseType.SqlServer)
                    {
                        ChangeCleanEmptyGropusTriggerState(scope.DbConnection, false);
                    }

                    var oldContent = GetById(content.Id);
                    var binding = content.WorkflowBinding;
                    var fieldValues = content.QpPluginFieldValues;
                    var newContent = DefaultRepository.Update<Content, ContentDAL>(content);
                    if (oldContent.UseNativeEfTypes != newContent.UseNativeEfTypes)
                    {
                        Common.ChangeNativeEfTypes(scope.DbConnection, content.Id, newContent.UseNativeEfTypes);
                    }
                    var isAggregated = IsAnyAggregatedFields(content.Id);
                    if (!isAggregated)
                    {
                        binding.SetContent(newContent);
                        WorkflowRepository.UpdateContentWorkflowBind(binding);

                        // Обновить свойства у агрегированных контентов
                        foreach (var aggregated in content.AggregatedContents)
                        {
                            var localAggregated = aggregated;
                            if (localAggregated.AutoArchive != content.AutoArchive)
                            {
                                localAggregated.AutoArchive = content.AutoArchive;
                                localAggregated = DefaultRepository.Update<Content, ContentDAL>(localAggregated);
                            }

                            binding.SetContent(localAggregated);
                            WorkflowRepository.UpdateContentWorkflowBind(binding);
                        }
                    }

                    UpdatePluginValues(fieldValues, newContent.Id);

                    DeleteEmptyContentGroups();

                    return GetById(newContent.Id);

                }
                finally
                {
                    if (QPContext.DatabaseType == DatabaseType.SqlServer)
                    {
                        ChangeCleanEmptyGropusTriggerState(scope.DbConnection, true);
                    }
                }
            }
        }

        internal static void Delete(int id)
        {
            using (var scope = new QPConnectionScope())
            {
                try
                {
                    if (QPContext.DatabaseType == DatabaseType.SqlServer)
                    {
                        ChangeDropTableTriggerState(scope.DbConnection, false);
                        ChangeCleanEmptyGropusTriggerState(scope.DbConnection, false);
                        FieldRepository.ChangeDeleteContentLinkTriggerState(scope.DbConnection, false);
                        FieldRepository.ChangeCleanEmptyLinksTriggerState(scope.DbConnection, false);
                        FieldRepository.ChangeRemoveFieldTriggerState(scope.DbConnection, false);
                        FieldRepository.ChangeReorderFieldsTriggerState(scope.DbConnection, false);
                    }

                    FieldRepository.DropLinkWithCheck(id);

                    DefaultRepository.Delete<ContentDAL>(id);

                    Common.DropContentViews(scope.DbConnection, id);
                    Common.DropContentTables(scope.DbConnection, id);

                    DeleteEmptyContentGroups();
                }
                finally
                {
                    if (QPContext.DatabaseType == DatabaseType.SqlServer)
                    {
                        ChangeDropTableTriggerState(scope.DbConnection, true);
                        ChangeCleanEmptyGropusTriggerState(scope.DbConnection, true);
                        FieldRepository.ChangeDeleteContentLinkTriggerState(scope.DbConnection, true);
                        FieldRepository.ChangeCleanEmptyLinksTriggerState(scope.DbConnection, true);
                        FieldRepository.ChangeRemoveFieldTriggerState(scope.DbConnection, true);
                        FieldRepository.ChangeReorderFieldsTriggerState(scope.DbConnection, true);
                    }
                }
            }
        }

        internal static Content Copy(Content content, int? forceId, int[] forceFieldIds, int[] forceLinkIds, bool forHierarchy)
        {
            var oldId = content.Id;
            content.LoadWorkflowBinding();
            content.Id = 0;
            content.XamlValidation = null;

            if (forceId.HasValue)
            {
                content.ForceId = forceId.Value;
            }

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

        public static IEnumerable<ContentGroup> GetSiteContentGroups(int siteId)
        {
            var defaultGroupId = GetDefaultGroupId(siteId);
            var result = MapperFacade.ContentGroupMapper.GetBizList(QPContext.EFContext.ContentGroupSet.Where(g => g.SiteId == siteId).ToList());

            foreach (var resultItem in result)
            {
                if (resultItem.Id == defaultGroupId)
                {
                    resultItem.Name = Translator.Translate(resultItem.Name);
                }
            }

            return result.OrderBy(g => g.Name).ToArray();
        }

        internal static ContentGroup GetGroupById(int id) => MapperFacade.ContentGroupMapper.GetBizObject(DefaultRepository.GetById<ContentGroupDAL>(id));

        internal static ContentGroup GetContentGroup(int contentId)
        {
            var content = QPContext.EFContext.ContentSet.Include("Group").SingleOrDefault(n => n.Id == contentId);
            return content != null ? MapperFacade.ContentGroupMapper.GetBizObject(content.Group) : null;
        }

        internal static ContentGroup SaveGroup(ContentGroup group)
        {
            var dal = MapperFacade.ContentGroupMapper.GetDalObject(group);
            DefaultRepository.TurnIdentityInsertOn(EntityTypeCode.ContentGroup, group);
            if (group.ForceId != 0)
            {
                dal.Id = group.ForceId;
            }

            var newDal = DefaultRepository.SimpleSave(dal);
            DefaultRepository.TurnIdentityInsertOff(EntityTypeCode.ContentGroup);
            return MapperFacade.ContentGroupMapper.GetBizObject(newDal);
        }

        internal static ContentGroup UpdateGroup(ContentGroup group)
        {
            var dal = MapperFacade.ContentGroupMapper.GetDalObject(group);
            var newDal = DefaultRepository.SimpleUpdate(dal);
            return MapperFacade.ContentGroupMapper.GetBizObject(newDal);
        }

        internal static List<ContentLink> GetContentLinks(int contentId)
        {
            return MapperFacade.ContentLinkMapper.GetBizList(QPContext.EFContext.ContentToContentSet.Where(n => n.LContentId == contentId || n.RContentId == contentId).OrderBy(n => n.LinkId).ToList());
        }

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
            return QPContext.EFContext.ContentSet.Any(n => n.Name == content.Name && n.Id != content.Id && n.SiteId == content.SiteId);
        }

        private static bool ContentNetNameExists(Content content)
        {
            return QPContext.EFContext.ContentSet.Where(n => n.NetName == content.NetName && n.Id != content.Id).Any(GetScopeExpression(content));
        }

        private static bool LinkNetNameExists(Content content)
        {
            return QPContext.EFContext.ContentToContentSet.Include("Content").Where(n => n.NetLinkName == content.NetName).Any(GetLinkScopeExpression(content));
        }

        internal static bool NetNameExists(Content content) => ContentNetNameExists(content) || LinkNetNameExists(content);

        internal static bool NetPluralNameExists(Content content) => ContentNetPluralNameExists(content) || LinkNetPluralNameExists(content);

        private static bool ContentNetPluralNameExists(Content content)
        {
            return QPContext.EFContext.ContentSet.Where(n => n.NetPluralName == content.NetPluralName && n.Id != content.Id).Any(GetScopeExpression(content));
        }

        private static bool LinkNetPluralNameExists(Content content)
        {
            return QPContext.EFContext.ContentToContentSet.Include("Content").Where(n => n.NetPluralLinkName == content.NetPluralName).Any(GetLinkScopeExpression(content));
        }

        internal static bool ContextClassNameExists(Content content) => content.AdditionalContextClassName == content.Site.FullyQualifiedContextClassName;

        internal static bool GroupNameExists(ContentGroup group)
        {
            return QPContext.EFContext.ContentGroupSet.Any(n => n.Name == group.Name && n.Id != group.Id && n.SiteId == group.Id);
        }

        private static Expression<Func<ContentDAL, bool>> GetScopeExpression(Content content)
        {
            return n => n.SiteId == content.SiteId;
        }

        private static Expression<Func<ContentToContentDAL, bool>> GetLinkScopeExpression(Content content)
        {
            return n => n.Content.SiteId == content.SiteId;
        }

        internal static Field GetTitleField(int contentId) => FieldRepository.GetByName(contentId, GetTitleName(contentId));

        internal static string GetTitleName(int contentId)
        {
            var displayFields = GetDisplayFields(contentId, true);
            var fieldName = displayFields
                .OrderByDescending(x => x.ViewInList)
                .ThenByDescending(x => x.FieldPriority(true))
                .ThenBy(x => x.Order)
                .FirstOrDefault();

            return fieldName?.Name ?? ContentDataColumnName.ContentItemId;
        }

        internal static IEnumerable<Field> GetDisplayFields(int contentId, Field field = null)
        {
            // ReSharper disable once PossibleInvalidOperationException
            var excludeId = field != null && field.ExactType == FieldExactTypes.M2ORelation ? field.BackRelationId.Value : 0;
            var fields = field == null || field.ListFieldTitleCount <= 1 && field.ExactType == FieldExactTypes.O2MRelation || field.ListFieldTitleCount <= 0
                ? null
                : ((IContentRepository)new ContentRepository()).GetDisplayFieldIds(contentId, field.IncludeRelationsInTitle, excludeId)
                .Take(field.ListFieldTitleCount)
                .Select(FieldRepository.GetById);

            var displayField = field?.Relation != null && field.ExactType == FieldExactTypes.O2MRelation ? field.Relation : GetTitleField(contentId);
            return fields ?? (displayField != null ? new[] { displayField } : new Field[]{});
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

        internal static IEnumerable<ListItem> GetAcceptableContentForRelation(int contentId)
        {
            var content = QPContext.EFContext.ContentSet.SingleOrDefault(n => n.Id == contentId);
            if (content == null)
            {
                return Enumerable.Empty<ListItem>();
            }

            return QPContext.EFContext.ContentSet
                .Where(c => c.VirtualType != VirtualType.Join && c.SiteId == content.SiteId)
                .Select(c => new { c.Id, Text = c.Name })
                .ToArray()
                .OrderBy(c => c.Text, StringComparer.InvariantCultureIgnoreCase)
                .Select(c => new ListItem { Value = c.Id.ToString(CultureInfo.InvariantCulture), Text = c.Text })
                .ToArray();
        }

        internal static void CopyAccess(int oldId, int newId)
        {
            using (new QPConnectionScope())
            {
                Common.CopyContentAccess(QPConnectionScope.Current.DbConnection, oldId, newId, QPContext.CurrentUserId);
            }
        }

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

        public static IEnumerable<Content> GetRelatedO2MContents(Content content)
        {
            if (!content.IsNew)
            {
                var context = QPContext.EFContext;
                var contentDal = (from f1 in context.FieldSet
                    join f2 in context.FieldSet on f1.Id equals f2.RelationId
                    where f1.ContentId == content.Id && f2.ContentId != content.Id
                    select f2.Content).ToList();

                return MapperFacade.ContentMapper.GetBizList(contentDal)
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

        internal static IEnumerable<string> GetSharedUnionBaseContentNames(int siteId)
        {
            using (new QPConnectionScope())
            {
                var rows = Common.GetSharedUnionBaseContentInfo(siteId, QPConnectionScope.Current.DbConnection);
                return rows.Select(r => r.Field<string>("BASE_CONTENT_NAME")).ToArray();
            }
        }

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
                return MapperFacade.ContentMapper.GetBizList(QPContext.EFContext.FieldSet
                    .Where(f => f.Classifier.ContentId == contentId)
                    .Select(f => f.Content)
                    .ToList());
            }

            return Enumerable.Empty<Content>();
        }

        public static Content GetBaseAggregationContent(int contentId)
        {
            if (contentId > 0)
            {
                return MapperFacade.ContentMapper.GetBizObject(
                    QPContext.EFContext.FieldSet
                        .Where(f => f.ContentId == contentId && f.Aggregated)
                        .Select(f => f.Classifier.Content)
                        .FirstOrDefault()
                );
            }

            return null;
        }

        internal static IEnumerable<ListItem> GetGroupSimpleList(int siteId, int[] selectedIds = null)
        {
            var groups = GetSiteContentGroups(siteId).Select(g => new ListItem(g.Id.ToString(), g.Name)).ToList();
            if (selectedIds != null)
            {
                foreach (var gr in groups.Where(gr => selectedIds.Contains(int.Parse(gr.Value))))
                {
                    gr.Selected = true;
                }
            }

            return groups;
        }

        internal static bool IsAnyAggregatedFields(int contentId)
        {
            return QPContext.EFContext.FieldSet.Any(f => f.ContentId == contentId && f.Aggregated);
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

        internal static string GetRelationsBetweenContentsXml(int sourceSiteId, int destinationSiteId, string newContentIds)
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

        internal static void CopyContentConstraints(string relationsBetweenContentsXml, out string result)
        {
            using (new QPConnectionScope())
            {
                var rows = Common.CopyContentConstraints(QPConnectionScope.Current.DbConnection, relationsBetweenContentsXml);
                result = MultistepActionHelper.GetXmlFromDataRows(rows, "constraint");
            }
        }

        internal static int CopyContents(int oldSiteId, int newSiteId, int startFrom, int endOn, out string result)
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
            return MapperFacade.ContentMapper.GetBizList(
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

        internal static void CopyContentLinks(int sourceSiteId, int destinationSiteId, out string result)
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

        internal static int[] GetContentIds(int siteId)
        {
            using (new QPConnectionScope())
            {
                return QPContext.EFContext.ContentSet.Where(n => n.SiteId == siteId).Select(n => (int)n.Id).ToArray();
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

        internal static void FillLinksTables(string relationsBetweenLinks)
        {
            using (new QPConnectionScope())
            {
                Common.FillLinksTables(QPConnectionScope.Current.DbConnection, relationsBetweenLinks);
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

        internal static int[] GetReferencedAggregatedContentIds(int contentId, int[] articleIds, bool isArchive = false)
        {
            using (var scope = new QPConnectionScope())
            {
                return Common.GetReferencedAggregatedContentIds(QPContext.EFContext, scope.DbConnection, contentId, articleIds, isArchive);
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
                return Common.GetAggregatedArticleIdsMap(QPContext.EFContext, scope.DbConnection, contentId, articleIds);
            }
        }

        private static void UpdatePluginValues(IEnumerable<QpPluginFieldValue> fieldValues, int contentId)
        {

            var actualFieldIds =
                QPContext.EFContext.PluginFieldValueSet.Where(n => n.ContentId == contentId)
                    .ToDictionary(n => (int)n.PluginFieldId, m => (int)m.Id);

            var entities = QPContext.EFContext;
            foreach (var fieldValue in fieldValues)
            {
                var dalValue = new PluginFieldValueDAL()
                {
                    ContentId = contentId,
                    Value = string.IsNullOrEmpty(fieldValue.Value) ? null : fieldValue.Value,
                    PluginFieldId = fieldValue.Field.Id,
                    Id = actualFieldIds.TryGetValue(fieldValue.Field.Id, out var result) ? result : 0
                };
                entities.Entry(dalValue).State = dalValue.Id == 0 ? EntityState.Added : EntityState.Modified;
            }

            entities.SaveChanges();
        }

    }
}
