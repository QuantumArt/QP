using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using AutoMapper;
using Quantumart.QP8.BLL.Factories;
using Quantumart.QP8.BLL.Factories.FolderFactory;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Repository.ArticleRepositories;
using Quantumart.QP8.BLL.Repository.ArticleRepositories.SearchParsers;
using Quantumart.QP8.BLL.Repository.ContentRepositories;
using Quantumart.QP8.BLL.Repository.FieldRepositories;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Constants;

// ReSharper disable PossibleInvalidOperationException
// ReSharper disable PossibleMultipleEnumeration

namespace Quantumart.QP8.BLL.Services
{
    public class EntityObjectService
    {
        /// <summary>
        /// Проверяет уникальность имен
        /// </summary>
        /// <param name="item">сущность</param>
        /// <returns>true или false</returns>
        public static bool CheckNameUniqueness(EntityObject item) => EntityObjectRepository.CheckNameUniqueness(item);

        /// <summary>
        /// Возвращает сущность
        /// </summary>
        /// <param name="entityTypeCode">код типа сущности</param>
        /// <param name="entityId">идентификатор сущности</param>
        /// <param name="loadChilds">признак, разрешающий загрузку дочерних сущностей</param>
        /// <param name="filter">фильтр</param>
        /// <returns>сущность</returns>
        public static EntityTreeItem GetByTypeAndIdForTree(string entityTypeCode, int entityId, bool loadChilds, string filter)
        {
            if (entityTypeCode == EntityTypeCode.Article)
            {
                return ArticleRepository.GetByIdForTree(entityId, loadChilds, filter);
            }

            if (entityTypeCode == EntityTypeCode.SiteFolder || entityTypeCode == EntityTypeCode.ContentFolder)
            {
                var folderRepository = FolderFactory.Create(entityTypeCode).CreateRepository();
                var folder = folderRepository.GetById(entityId);
                if (!loadChilds)
                {
                    return Mapper.Map<Folder, EntityTreeItem>(folder);
                }

                return Mapper.Map<Folder, EntityTreeItem>(folderRepository.GetSelfAndChildrenWithSync(folder.ParentEntityId, folder.Id));
            }

            return null;
        }

        public static IList<EntityTreeItem> GetEntityTreeItems(ChildListQuery query) => ArticleTreeFactory.Create(query).Process();

        /// <summary>
        /// Возвращает упрощенный список сущностей
        /// </summary>
        public static List<ListItem> SimpleList(SimpleListQuery query)
        {
            var itemList = new List<ListItem>();
            if (query.EntityTypeCode == EntityTypeCode.ContentGroup)
            {
                itemList = ContentRepository.GetGroupSimpleList(query.ParentEntityId, query.SelectedEntitiesIds).ToList();
            }
            else if (query.EntityTypeCode == EntityTypeCode.Article || query.EntityTypeCode == EntityTypeCode.ArchiveArticle)
            {
                itemList = ArticleRepository.GetSimpleList(query);
            }
            else if (query.EntityTypeCode == EntityTypeCode.Content)
            {
                itemList.AddRange(ContentRepository.GetSimpleList(query.ParentEntityId, query.SelectedEntitiesIds));
            }
            else if (query.EntityTypeCode == EntityTypeCode.Site)
            {
                itemList.AddRange(SiteRepository.GetSimpleList(query.SelectedEntitiesIds));
            }
            else if (query.EntityTypeCode == EntityTypeCode.User)
            {
                itemList.AddRange(UserRepository.GetSimpleList(query.SelectedEntitiesIds));
            }
            else if (query.EntityTypeCode == EntityTypeCode.UserGroup)
            {
                itemList.AddRange(UserGroupRepository.GetSimpleList(query.SelectedEntitiesIds));
            }
            else if (query.EntityTypeCode == EntityTypeCode.TemplateObjectFormat)
            {
                itemList.AddRange(ObjectFormatRepository.GetObjectFormats(query.ParentEntityId, query.ActualListId, query.SelectedEntitiesIds));
            }
            else if (query.EntityTypeCode == EntityTypeCode.Page)
            {
                itemList.AddRange(PageTemplateRepository.GetPageSimpleList(query.SelectedEntitiesIds));
            }
            else if (query.EntityTypeCode == EntityTypeCode.StatusType)
            {
                itemList.AddRange(StatusTypeRepository.GetStatusSimpleList(query.SelectedEntitiesIds));
            }
            else if (query.EntityTypeCode == EntityTypeCode.Field)
            {
                itemList.AddRange(FieldRepository.GetList(query.SelectedEntitiesIds).Select(c => new ListItem(c.Id.ToString(), c.Name)));
            }

            return itemList;
        }

        /// <summary>
        /// Проверяет существование сущности
        /// </summary>
        /// <param name="entityTypeCode">код типа сущности</param>
        /// <param name="entityId">идентификатор сущности</param>
        /// <returns>результат проверки (true - существует; false - не существует)</returns>
        public static bool CheckExistence(string entityTypeCode, int entityId) => EntityObjectRepository.CheckExistence(entityTypeCode, entityId);

        /// <summary>
        /// Проверяет сущность на наличие рекурсивных связей
        /// </summary>
        /// <param name="entityTypeCode">код типа сущности</param>
        /// <param name="entityId">идентификатор сущности</param>
        /// <returns>результат проверки (true - есть рекурсивные связи; false - нет)</returns>
        public static bool CheckPresenceSelfRelations(string entityTypeCode, int entityId) => EntityObjectRepository.CheckPresenceSelfRelations(entityTypeCode, entityId);

        /// <summary>
        /// Проверяет сущность на наличие вариаций
        /// </summary>
        /// <param name="entityTypeCode">код типа сущности</param>
        /// <param name="entityId">идентификатор сущности</param>
        /// <returns>результат проверки (true - есть вариации; false - нет)</returns>
        public static bool CheckForVariations(string entityTypeCode, int entityId) => EntityObjectRepository.CheckForVariations(entityTypeCode, entityId);

        /// <summary>
        /// Возвращает название сущности
        /// </summary>
        /// <param name="entityTypeCode">код типа сущности</param>
        /// <param name="entityId">идентификатор сущности</param>
        /// <param name="parentEntityId"></param>
        /// <returns>название сущности</returns>
        public static string GetName(string entityTypeCode, int entityId, int parentEntityId) => EntityObjectRepository.GetName(entityTypeCode, entityId, parentEntityId);

        /// <summary>
        /// Возвращает идентификатор родительской сущности
        /// </summary>
        /// <param name="entityTypeCode">код типа сущности</param>
        /// <param name="entityId">идентификатор сущности</param>
        /// <returns>идентификатор родительской сущности</returns>
        public static int? GetParentId(string entityTypeCode, int entityId) => EntityObjectRepository.GetParentId(entityTypeCode, entityId);

        public static int[] GetParentIdsForTree(ParentIdsForTreeQuery query) => EntityObjectRepository.GetParentIdsForTree(query);

        /// <summary>
        /// Возвращает цепочку сущностей для хлебных крошек
        /// </summary>
        /// <param name="entityTypeCode">код типа сущности</param>
        /// <param name="entityId">идентификатор сущности</param>
        /// <param name="parentEntityId">идентификатор родительской сущности</param>
        /// <param name="actionCode"></param>
        /// <returns>цепочка сущностей</returns>
        public static IEnumerable<EntityInfo> GetBreadCrumbsList(string entityTypeCode, long entityId, long? parentEntityId, string actionCode)
        {
            if (!string.IsNullOrWhiteSpace(actionCode))
            {
                if (actionCode.Equals(ActionCode.ChildContentPermissions, StringComparison.InvariantCultureIgnoreCase))
                {
                    entityTypeCode = EntityTypeCode.Content;
                }
                else if (actionCode.Equals(ActionCode.ChildArticlePermissions, StringComparison.InvariantCultureIgnoreCase))
                {
                    entityTypeCode = EntityTypeCode.Article;
                }
            }

            return EntityObjectRepository.GetParentsChain(entityTypeCode, entityId, parentEntityId);
        }

        /// <summary>
        /// Возвращает информацию о текущей и родительской сущности
        /// </summary>
        /// <param name="entityTypeCode">код типа сущности</param>
        /// <param name="entityId">идентификатор сущности</param>
        /// <param name="parentEntityId">идентификатор родительской сущности</param>
        /// <returns>информация о текущей и родительской сущности</returns>
        public static IEnumerable<EntityInfo> GetParentInfo(string entityTypeCode, long entityId, long? parentEntityId) => EntityObjectRepository.GetParentsChain(entityTypeCode, entityId, parentEntityId, true);

        /// <summary>
        /// Возвращает информацию о текущей сущности и всех предках
        /// </summary>
        /// <param name="entityTypeCode">код типа сущности</param>
        /// <param name="entityId">идентификатор сущности</param>
        /// <param name="parentEntityId">идентификатор родительской сущности</param>
        /// <returns>информация о текущей и родительской сущности</returns>
        public static IEnumerable<EntityInfo> GetParentsChain(string entityTypeCode, long entityId, long? parentEntityId) => EntityObjectRepository.GetParentsChain(entityTypeCode, entityId, parentEntityId);

        public static void UnlockAllEntitiesLockedByCurrentUser()
        {
            var id = QPContext.CurrentUserId;
            if (id != 0)
            {
                EntityObjectRepository.UnlockAllEntitiesLockedByUser(id);
            }
        }

        /// <summary>
        /// Проверить возможность восстановления а втосохраненных сущностей
        /// </summary>
        /// <param name="recordHeaders"></param>
        /// <returns></returns>
        public static IEnumerable<long> AutosaveRestoringCheck(IList<AutosavedEntityRecordHeader> recordHeaders)
        {
            if (recordHeaders != null && recordHeaders.Any())
            {
                // Проверка для существующих сущностей
                var approvedExisted = recordHeaders
                    .Where(h => !h.IsNew && h.Modified.HasValue)
                    .GroupBy(h => h.EntityTypeCode, StringComparer.InvariantCultureIgnoreCase)
                    .Select(g => new EntityObjectRepository.VariousTypeEntityQueryParam
                    {
                        EntityTypeCode = g.Key,
                        EntityIDs = g.Select(h2 => h2.EntityId).Distinct()
                    })
                    .GetVariousTypeList()
                    .Where(e =>
                    {
                        // проверка на возможность восстановить сущность
                        var rh = recordHeaders.First(h => h.EntityId == e.Id && h.EntityTypeCode.Equals(e.EntityTypeCode, StringComparison.InvariantCultureIgnoreCase));
                        return rh.Modified.Value <= e.Modified
                            && ( // проверка на lock
                                e is LockableEntityObject
                                && !((LockableEntityObject)e).LockedByAnyoneElse
                                || !(e is LockableEntityObject)
                            )
                            && !e.IsReadOnly;
                    }).Select(e => recordHeaders
                        .First(h => !h.IsNew && h.EntityId == e.Id && h.EntityTypeCode.Equals(e.EntityTypeCode, StringComparison.InvariantCultureIgnoreCase))
                        .RecordId
                    );

                // Проверка для новых сущностей (существует ли parent)
                var code2ParentCode = EntityTypeRepository.GetList().ToDictionary(t => t.Code, t => t.ParentCode, StringComparer.InvariantCultureIgnoreCase);
                foreach (var h in recordHeaders)
                {
                    h.ParentEntityTypeCode = code2ParentCode[h.EntityTypeCode];
                }

                var approvedNew = recordHeaders
                    .Where(h => h.IsNew)
                    .GroupBy(h => h.ParentEntityTypeCode, StringComparer.InvariantCultureIgnoreCase)
                    .Select(g => new EntityObjectRepository.VariousTypeEntityQueryParam
                    {
                        EntityTypeCode = g.Key,
                        EntityIDs = g.Select(h2 => h2.ParentEntityId).Distinct()
                    })
                    .GetVariousTypeList()
                    .SelectMany(e => recordHeaders
                        .Where(h =>
                            h.IsNew &&
                            h.ParentEntityId == e.Id &&
                            h.ParentEntityTypeCode.Equals(e.EntityTypeCode, StringComparison.InvariantCultureIgnoreCase)
                        )
                        .Select(h => h.RecordId)
                    );

                return approvedExisted.Concat(approvedNew).ToArray();
            }

            return Enumerable.Empty<long>();
        }

        public static string GetArticleFieldValue(int contentId, string fieldName, int articleId) => ArticleRepository.GetFieldValue(articleId, contentId, fieldName);

        public static Dictionary<int, string> GetContentFieldValues(int contentId, string fieldName) => ArticleRepository.GetContentFieldValues(contentId, fieldName);


        public static string GetArticleLinkedItems(int linkId, int articleId) => ArticleRepository.GetLinkedItems(new [] {linkId}, articleId)[linkId];

        public static int GetArticleIdByFieldValue(int contentId, string fieldName, string fieldValue) => ArticleRepository.GetArticleIdByFieldValue(contentId, fieldName, fieldValue);
    }
}
