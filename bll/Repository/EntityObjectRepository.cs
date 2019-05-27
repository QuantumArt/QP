using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Quantumart.QP8.BLL.Repository.ArticleRepositories;
using Quantumart.QP8.BLL.Repository.ContentRepositories;
using Quantumart.QP8.BLL.Repository.FieldRepositories;
using Quantumart.QP8.BLL.Repository.Helpers;
using Quantumart.QP8.Constants;
using Quantumart.QP8.DAL;

namespace Quantumart.QP8.BLL.Repository
{
    internal static class EntityObjectRepository
    {
        internal static bool CheckNameUniqueness(EntityObject entity)
        {
            using (new QPConnectionScope())
            {
                return Common.CheckUnique(QPConnectionScope.Current.DbConnection, entity.EntityTypeCode, entity.Name, entity.Id, entity.ParentEntityId, entity.RecurringId);
            }
        }

        private static DateTime? Lock(EntityObject item, int? userId)
        {
            using (new QPConnectionScope())
            {
                var entityType = EntityTypeRepository.GetByCode(item.EntityTypeCode);
                var source = entityType?.Source;
                var idField = entityType?.IdField;
                return Common.Lock(QPConnectionScope.Current.DbConnection, source, idField, item.Id, userId);
                // return Common.Lock(QPConnectionScope.Current.DbConnection, item.EntityTypeCode, item.Id, userId);
            }
        }

        internal static DateTime? Lock(EntityObject item) => Lock(item, QPContext.CurrentUserId);

        internal static void UnLock(EntityObject item)
        {
            Lock(item, null);
        }

        internal static DateTime? CaptureLock(EntityObject item)
        {
            UnLock(item);
            return Lock(item);
        }

        internal static EntityObject GetById<T>(int id)
            where T : EntityObject
        {
            if (typeof(T) == typeof(Article))
            {
                return ArticleRepository.GetById(id);
            }

            if (typeof(T) == typeof(Site))
            {
                return SiteRepository.GetById(id);
            }

            if (typeof(T) == typeof(Content))
            {
                return ContentRepository.GetById(id);
            }

            if (typeof(T) == typeof(Field))
            {
                return FieldRepository.GetById(id);
            }

            throw new Exception("Unsupported entity object type");
        }

        private class CustomerObject : EntityObject
        {
            public override string EntityTypeCode => Constants.EntityTypeCode.CustomerCode;
        }

        internal static IEnumerable<EntityObject> GetList(string entityTypeCode, IList<int> ids)
        {
            if (entityTypeCode.Equals(EntityTypeCode.CustomerCode, StringComparison.InvariantCultureIgnoreCase) && ids.Any())
            {
                return new EntityObject[] { new CustomerObject { Id = ids.First(), Modified = DateTime.MinValue, IsReadOnly = true } };
            }

            if (entityTypeCode.Equals(EntityTypeCode.Site, StringComparison.InvariantCultureIgnoreCase))
            {
                return SiteRepository.GetList(ids);
            }

            if (entityTypeCode.Equals(EntityTypeCode.Content, StringComparison.InvariantCultureIgnoreCase))
            {
                return ContentRepository.GetList(ids);
            }

            if (entityTypeCode.Equals(EntityTypeCode.Field, StringComparison.InvariantCultureIgnoreCase))
            {
                return FieldRepository.GetList(ids);
            }

            if (entityTypeCode.Equals(EntityTypeCode.Article, StringComparison.InvariantCultureIgnoreCase))
            {
                return ArticleRepository.GetList(ids);
            }

            if (entityTypeCode.Equals(EntityTypeCode.Notification, StringComparison.InvariantCultureIgnoreCase))
            {
                return NotificationRepository.GetList(ids);
            }

            if (entityTypeCode.Equals(EntityTypeCode.VisualEditorPlugin, StringComparison.InvariantCultureIgnoreCase))
            {
                return VisualEditorRepository.GetPluginList(ids);
            }

            if (entityTypeCode.Equals(EntityTypeCode.VisualEditorCommand, StringComparison.InvariantCultureIgnoreCase))
            {
                return VisualEditorRepository.GetCommandList(ids);
            }

            if (entityTypeCode.Equals(EntityTypeCode.VisualEditorStyle, StringComparison.InvariantCultureIgnoreCase))
            {
                return VisualEditorRepository.GetStyleList(ids);
            }

            if (entityTypeCode.Equals(EntityTypeCode.StatusType, StringComparison.InvariantCultureIgnoreCase))
            {
                return StatusTypeRepository.GetList(ids);
            }

            if (entityTypeCode.Equals(EntityTypeCode.Workflow, StringComparison.InvariantCultureIgnoreCase))
            {
                return WorkflowRepository.GetList(ids);
            }

            if (entityTypeCode.Equals(EntityTypeCode.PageTemplate, StringComparison.InvariantCultureIgnoreCase))
            {
                return PageTemplateRepository.GetPageTemplateList(ids);
            }

            if (entityTypeCode.Equals(EntityTypeCode.User, StringComparison.InvariantCultureIgnoreCase))
            {
                return UserRepository.GetList(ids);
            }

            if (entityTypeCode.Equals(EntityTypeCode.UserGroup, StringComparison.InvariantCultureIgnoreCase))
            {
                return UserGroupRepository.GetList(ids);
            }

            if (entityTypeCode.Equals(EntityTypeCode.TemplateObjectFormat, StringComparison.InvariantCultureIgnoreCase))
            {
                return FormatRepository.GetList(ids, false);
            }

            if (entityTypeCode.Equals(EntityTypeCode.PageObjectFormat, StringComparison.InvariantCultureIgnoreCase))
            {
                return FormatRepository.GetList(ids, true);
            }

            if (entityTypeCode.Equals(EntityTypeCode.PageObject, StringComparison.InvariantCultureIgnoreCase) || entityTypeCode.Equals(EntityTypeCode.TemplateObject, StringComparison.InvariantCultureIgnoreCase))
            {
                return ObjectRepository.GetList(ids);
            }

            if (entityTypeCode.Equals(EntityTypeCode.VirtualContent, StringComparison.InvariantCultureIgnoreCase))
            {
                return VirtualContentRepository.GetList(ids);
            }

            if (entityTypeCode.Equals(EntityTypeCode.Page, StringComparison.InvariantCultureIgnoreCase))
            {
                return PageRepository.GetList(ids);
            }

            return Enumerable.Empty<EntityObject>();
        }

        internal sealed class VariousTypeEntityQueryParam
        {
            public string EntityTypeCode { get; set; }

            public IEnumerable<int> EntityIDs { get; set; }
        }

        internal static IEnumerable<EntityObject> GetVariousTypeList(this IEnumerable<VariousTypeEntityQueryParam> param)
        {
            var result = new List<EntityObject>();
            foreach (var p in param)
            {
                result.AddRange(GetList(p.EntityTypeCode, p.EntityIDs.ToList()));
            }

            return result;
        }

        /// <summary>
        /// Проверяет существование сущности
        /// </summary>
        /// <param name="entityTypeCode">код типа сущности</param>
        /// <param name="entityId">идентификатор сущности</param>
        /// <returns>результат проверки (true - существует; false - не существует)</returns>
        internal static bool CheckExistence(string entityTypeCode, int entityId)
        {
            using (var scope = new QPConnectionScope())
            {
                return Common.CheckEntityExistence(scope.DbConnection, entityTypeCode, entityId);
            }
        }

        /// <summary>
        /// Проверяет сущность на наличие рекурсивных связей
        /// </summary>
        /// <param name="entityTypeCode">код типа сущности</param>
        /// <param name="entityId">идентификатор сущности</param>
        /// <returns>результат проверки (true - есть рекурсивные связи; false - нет)</returns>
        internal static bool CheckPresenceSelfRelations(string entityTypeCode, int entityId)
        {
            var result = false;

            if (entityTypeCode == EntityTypeCode.Content || entityTypeCode == EntityTypeCode.VirtualContent)
            {
                result = ContentRepository.HasContentTreeField(entityId);
            }

            return result;
        }

        internal static bool CheckForVariations(string entityTypeCode, int parentEntityId)
        {
            var result = false;

            if (entityTypeCode == EntityTypeCode.Article || entityTypeCode == EntityTypeCode.VirtualArticle)
            {
                result = ContentRepository.HasVariationField(parentEntityId);
            }

            return result;
        }

        /// <summary>
        /// Возвращает название сущности
        /// </summary>
        /// <param name="entityTypeCode">код типа сущности</param>
        /// <param name="entityId">идентификатор сущности</param>
        /// <param name="parentEntityId"></param>
        /// <returns>название сущности</returns>
        internal static string GetName(string entityTypeCode, int entityId, int parentEntityId = 0)
        {

            return GetEntityTitle(entityId, entityTypeCode, parentEntityId);
            // using (var scope = new QPConnectionScope())
            // {
            //     return Common.GetEntityName(scope.DbConnection, entityTypeCode, entityId, parentEntityId);
            // }
        }

        /// <summary>
        /// Возвращает идентификатор родительской сущности
        /// </summary>
        /// <param name="entityTypeCode">код типа сущности</param>
        /// <param name="entityId">идентификатор сущности</param>
        /// <returns>идентификатор родительской сущности</returns>
        internal static int? GetParentId(string entityTypeCode, int entityId)
        {
            using (var scope = new QPConnectionScope())
            {
                return Common.GetParentEntityId(scope.DbConnection, entityTypeCode, entityId);
            }
        }

        internal static int[] GetParentIdsForTree(string entityTypeCode, int[] ids)
        {
            using (var scope = new QPConnectionScope())
            {
                return Common.GetParentEntityIdsForTree(scope.DbConnection, entityTypeCode, ids);
            }
        }

        /// <summary>
        /// Получить словари
        /// </summary>
        internal static IEnumerable<DataRow> GetTranslations()
        {
            using (var scope = new QPConnectionScope())
            {
                return Common.GetTranslations(scope.DbConnection);
            }
        }

        /// <summary>
        /// Возвращает цепочку родительских сущностей
        /// </summary>
        /// <param name="entityTypeCode">код типа сущности</param>
        /// <param name="entityId">идентификатор сущности</param>
        /// <param name="parentEntityId">идентификатор родительской сущности</param>
        /// <param name="oneLevel"></param>
        /// <returns>цепочка сущностей</returns>
        internal static IEnumerable<EntityInfo> GetParentsChain(string entityTypeCode, long entityId, long? parentEntityId = null, bool oneLevel = false)
        {
            var result = GetParentsChainInternal(entityTypeCode, entityId, parentEntityId, oneLevel);
            var customerCodeInfo = result?.SingleOrDefault(n => n.Code == EntityTypeCode.CustomerCode);
            if (customerCodeInfo != null)
            {
                customerCodeInfo.Title = QPContext.CurrentCustomerCode;
            }

            return result;
        }

        internal static IList<EntityInfo> GetParentsChainInternal(string entityTypeCode, long entityId, long? parentEntityId = null, bool oneLevel = false)
        {
            string code;

            var defaultActionCode = "";
            string folderDefaultActionCode;



            var result = new List<Tuple<EntityInfo, string>>();

            using (new QPConnectionScope())
            {
                var entityTypes = EntityTypeRepository.GetList();
                var backendActions = BackendActionCache.Actions;

                int level;
                long? id;
                if (entityId != 0)
                {
                    level = 0;
                    id = entityId;
                    code = entityTypeCode;
                }
                else
                {
                    level = 1;
                    id = parentEntityId;

                    code = entityTypes
                        .FirstOrDefault(x => x.Code.Equals(entityTypeCode, StringComparison.InvariantCultureIgnoreCase))
                        ?.ParentCode;

                }

                while (!string.IsNullOrWhiteSpace(code))
                {
                    var entity = entityTypes
                        .FirstOrDefault(x => x.Code.Equals(code, StringComparison.InvariantCultureIgnoreCase));

                    var title = GetEntityTitle(id, code, parentEntityId);

                    var recurringId = !string.IsNullOrWhiteSpace(entity?.RecurringIdField) && !string.IsNullOrWhiteSpace(entity?.Source) && !string.IsNullOrWhiteSpace(entity?.IdField) && !string.IsNullOrWhiteSpace(entity?.RecurringIdField)
                        ? (long?)Common.GetNumericFieldValue(QPConnectionScope.Current.DbConnection, entity.RecurringIdField, entity.Source, entity.IdField, (decimal)id)
                        : null;

                    long? parentId;
                    if (recurringId.HasValue)
                    {
                        parentId = recurringId;
                    }
                    else
                    {
                        if (code == EntityTypeCode.VirtualArticle)
                        {
                            parentId = parentEntityId;
                        }
                        else
                        {
                            parentId = !string.IsNullOrWhiteSpace(entity?.ParentIdField) && !string.IsNullOrWhiteSpace(entity?.Source) && !string.IsNullOrWhiteSpace(entity?.IdField)
                                ? (long?)Common.GetNumericFieldValue(QPConnectionScope.Current.DbConnection, entity.ParentIdField, entity.Source, entity.IdField, (decimal)id)
                                : null;
                        }
                    }

                    if (!parentId.HasValue)
                    {
                        parentId =
                            code == EntityTypeCode.CustomerCode
                                ? 0
                                : entityTypes.FirstOrDefault(x => x.Code == EntityTypeCode.CustomerCode)?.Id;
                    }

                    result.Add(new Tuple<EntityInfo, string>(new EntityInfo
                        {
                            Id = id.Value,
                            ParentId = parentId,
                            Code = code,
                            EntityTypeName = entity?.Name,
                            Title = title,
                            IsFolder = false,
                            ActionCode = backendActions.FirstOrDefault(x => x.Id == entity.DefaultActionId)?.Code,
                        },
                        backendActions.FirstOrDefault(x => x.Id == entity.FolderDefaultActionId)?.Code
                    ));

                    id = parentId;

                    if (!recurringId.HasValue)
                    {
                        code = entity.ParentCode;
                    }

                    if (level == 1 && oneLevel) break;

                    level++;
                }

                result.ForEach(x =>
                {
                    var parentFolder = result.FirstOrDefault(y => y.Item1.Id == x.Item1.ParentId && !string.IsNullOrWhiteSpace(y.Item2));
                    if (parentFolder != null)
                    {
                        x.Item1.ActionCode = parentFolder.Item2;
                    }
                });

                return result.Select(x => x.Item1).ToList();
            }
        }

        private static string GetEntityTitle(long? entityId, string entityTypeCode, decimal? parentEntityId)
        {
            if (entityTypeCode == EntityTypeCode.VirtualArticle)
            {
                return GetArticleTitle(entityId.Value, parentEntityId.Value);
            }

            if (entityTypeCode == EntityTypeCode.Article || entityTypeCode == EntityTypeCode.ArchiveArticle)
            {
                var contentId = QPContext.EFContext.ArticleSet.FirstOrDefault(x => x.Id == entityId)?.ContentId;
                return GetArticleTitle(entityId.Value, contentId.Value);
            }

            var entity = EntityTypeRepository.GetList().FirstOrDefault(x => x.Code == entityTypeCode);

            if (string.IsNullOrWhiteSpace(entity?.Source) || string.IsNullOrWhiteSpace(entity?.IdField))
            {
                return null;
            }
            else
            {
                using (var scope = new QPConnectionScope())
                {
                    return Common.GetStringFieldValue(scope.DbConnection, entity.TitleField, entity.Source, entity.IdField, entityId.Value);
                }

            }
        }

        private static string GetArticleTitle(long contentItemId, decimal contentId)
        {
            var displayFields = ContentRepository.GetDisplayFields((int)contentId, true);

            var r = displayFields.Select(x =>
                {
                    var relatedField = x.RelationId != null ? FieldRepository.GetById(x.RelationId.Value) : null;
                    var relatedRelatedField = relatedField?.RelationId != null ? FieldRepository.GetById(relatedField.RelationId.Value) : null;
                    return new
                    {
                        Field = x,
                        RelatedField = relatedField,
                        RelatedRelatedField = relatedRelatedField
                    };
                })
                .OrderBy(x => x.Field.Order)
                .FirstOrDefault();

            using (var scope = new QPConnectionScope())
            {
                return Common.GetArticleTitle(scope.DbConnection,
                    contentItemId,
                    contentId,
                    r?.Field?.Name, r?.RelatedField?.Name, r?.RelatedField?.ContentId, r?.RelatedRelatedField?.Name, r?.RelatedRelatedField?.ContentId);
            }

        }

        internal static void UnlockAllEntitiesLockedByUser(int userId)
        {
            using (var scope = new QPConnectionScope())
            {
                Common.UnlockAllArticlesLockedByUser(scope.DbConnection, userId);
                Common.UnlockAllSitesLockedByUser(scope.DbConnection, userId);
                Common.UnlockAllTemplatesLockedByUser(scope.DbConnection, userId);
            }
        }

        internal static HashSet<int> GetTreeIdsToLoad(string entityTypeCode, int parentEntityId, string selectItemIds)
        {
            using (var scope = new QPConnectionScope())
            {
                if (entityTypeCode == EntityTypeCode.Article || entityTypeCode == EntityTypeCode.VirtualArticle)
                {
                    return Common.GetTreeIdsToLoad(scope.DbConnection, $"CONTENT_{parentEntityId}_UNITED", ((IContentRepository)new ContentRepository()).GetTreeFieldName(parentEntityId, 0), FieldName.ContentItemId, selectItemIds);
                }

                return Common.GetTreeIdsToLoad(scope.DbConnection, entityTypeCode, selectItemIds);
            }
        }
    }
}
