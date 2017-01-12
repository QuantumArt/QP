using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Quantumart.QP8.BLL.Factories;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Repository.Articles;
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
        public static bool CheckNameUniqueness(EntityObject item)
        {
            return EntityObjectRepository.CheckNameUniqueness(item);
        }

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

        public static IList<EntityTreeItem> GetEntityTreeItems(string entityTypeCode, int? parentEntityId, int? entityId, bool returnSelf, string filter, string hostFilter, string selectItemIDs, IList<ArticleSearchQueryParam> searchQuery, IList<ArticleContextQueryParam> contextQuery, ArticleFullTextSearchQueryParser ftsParser)
        {
            return ArticleTreeFactory.Create(entityTypeCode, parentEntityId, entityId, returnSelf, filter, hostFilter, selectItemIDs, searchQuery, contextQuery, ftsParser).Process();
        }

        /// <summary>
        /// Возвращает упрощенный список сущностей
        /// </summary>
        /// <param name="entityTypeCode">код типа сущности</param>
        /// <param name="parentEntityId">идентификатор родительской сущности</param>
        /// <param name="entityId">идентификатор сущности</param>
        /// <param name="listId">дополнительный параметр для идентификации списка</param>
        /// <param name="selectionMode">режим выделения списка</param>
        /// <param name="selectedEntitiesIDs">идентификаторы выбранных сущностей</param>
        /// <param name="filter"></param>
        /// <param name="testEntityId"></param>
        /// <returns>упрощенный список сущностей</returns>
        public static List<ListItem> SimpleList(string entityTypeCode, int parentEntityId, int? entityId, int? listId, ListSelectionMode selectionMode, int[] selectedEntitiesIDs, string filter, int testEntityId)
        {
            var itemList = new List<ListItem>();
            if (entityTypeCode == EntityTypeCode.ContentGroup)
            {
                itemList = ContentRepository.GetGroupSimpleList(parentEntityId, selectedEntitiesIDs).ToList();
            }
            else if (entityTypeCode == EntityTypeCode.Article || entityTypeCode == EntityTypeCode.ArchiveArticle)
            {
                itemList = ArticleRepository.GetSimpleList(parentEntityId, entityId, listId, selectionMode, selectedEntitiesIDs, filter, testEntityId);
            }
            else if (entityTypeCode == EntityTypeCode.Content)
            {
                itemList.AddRange(ContentRepository.GetSimpleList(parentEntityId, selectedEntitiesIDs));
            }
            else if (entityTypeCode == EntityTypeCode.Site)
            {
                itemList.AddRange(SiteRepository.GetSimpleList(selectedEntitiesIDs));
            }
            else if (entityTypeCode == EntityTypeCode.User)
            {
                itemList.AddRange(UserRepository.GetSimpleList(selectedEntitiesIDs));
            }
            else if (entityTypeCode == EntityTypeCode.UserGroup)
            {
                itemList.AddRange(UserGroupRepository.GetSimpleList(selectedEntitiesIDs));
            }
            else if (entityTypeCode == EntityTypeCode.TemplateObjectFormat)
            {
                itemList.AddRange(ObjectFormatRepository.GetObjectFormats(parentEntityId, listId, selectedEntitiesIDs));
            }
            else if (entityTypeCode == EntityTypeCode.Page)
            {
                itemList.AddRange(PageTemplateRepository.GetPageSimpleList(selectedEntitiesIDs));
            }
            else if (entityTypeCode == EntityTypeCode.StatusType)
            {
                itemList.AddRange(StatusTypeRepository.GetStatusSimpleList(selectedEntitiesIDs));
            }
            else if (entityTypeCode == EntityTypeCode.Field)
            {
                itemList.AddRange(FieldRepository.GetList(selectedEntitiesIDs).Select(c => new ListItem(c.Id.ToString(), c.Name)));
            }

            return itemList;
        }

        /// <summary>
        /// Проверяет существование сущности
        /// </summary>
        /// <param name="entityTypeCode">код типа сущности</param>
        /// <param name="entityId">идентификатор сущности</param>
        /// <returns>результат проверки (true - существует; false - не существует)</returns>
        public static bool CheckExistence(string entityTypeCode, int entityId)
        {
            return EntityObjectRepository.CheckExistence(entityTypeCode, entityId);
        }

        /// <summary>
        /// Проверяет сущность на наличие рекурсивных связей
        /// </summary>
        /// <param name="entityTypeCode">код типа сущности</param>
        /// <param name="entityId">идентификатор сущности</param>
        /// <returns>результат проверки (true - есть рекурсивные связи; false - нет)</returns>
        public static bool CheckPresenceSelfRelations(string entityTypeCode, int entityId)
        {
            return EntityObjectRepository.CheckPresenceSelfRelations(entityTypeCode, entityId);
        }

        /// <summary>
        /// Проверяет сущность на наличие вариаций
        /// </summary>
        /// <param name="entityTypeCode">код типа сущности</param>
        /// <param name="entityId">идентификатор сущности</param>
        /// <returns>результат проверки (true - есть вариации; false - нет)</returns>
        public static bool CheckForVariations(string entityTypeCode, int entityId)
        {
            return EntityObjectRepository.CheckForVariations(entityTypeCode, entityId);
        }

        /// <summary>
        /// Возвращает название сущности
        /// </summary>
        /// <param name="entityTypeCode">код типа сущности</param>
        /// <param name="entityId">идентификатор сущности</param>
        /// <param name="parentEntityId"></param>
        /// <returns>название сущности</returns>
        public static string GetName(string entityTypeCode, int entityId, int parentEntityId)
        {
            return EntityObjectRepository.GetName(entityTypeCode, entityId, parentEntityId);
        }

        /// <summary>
        /// Возвращает идентификатор родительской сущности
        /// </summary>
        /// <param name="entityTypeCode">код типа сущности</param>
        /// <param name="entityId">идентификатор сущности</param>
        /// <returns>идентификатор родительской сущности</returns>
        public static int? GetParentId(string entityTypeCode, int entityId)
        {
            return EntityObjectRepository.GetParentId(entityTypeCode, entityId);
        }

        public static int[] GetParentIdsForTree(string entityTypeCode, int[] ids)
        {
            return EntityObjectRepository.GetParentIdsForTree(entityTypeCode, ids);
        }

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
        public static IEnumerable<EntityInfo> GetParentInfo(string entityTypeCode, long entityId, long? parentEntityId)
        {
            return EntityObjectRepository.GetParentsChain(entityTypeCode, entityId, parentEntityId, true);
        }

        /// <summary>
        /// Возвращает информацию о текущей сущности и всех предках
        /// </summary>
        /// <param name="entityTypeCode">код типа сущности</param>
        /// <param name="entityId">идентификатор сущности</param>
        /// <param name="parentEntityId">идентификатор родительской сущности</param>
        /// <returns>информация о текущей и родительской сущности</returns>
        public static IEnumerable<EntityInfo> GetParentsChain(string entityTypeCode, long entityId, long? parentEntityId)
        {
            return EntityObjectRepository.GetParentsChain(entityTypeCode, entityId, parentEntityId);
        }

        public static void UnlockAllEntitiesLockedByCurrentUser()
        {
            EntityObjectRepository.UnlockAllEntitiesLockedByUser(QPContext.CurrentUserId);
        }

        /// <summary>
        /// Проверить возможность восстановления а втосохраненных сущностей
        /// </summary>
        /// <param name="recordHeaders"></param>
        /// <returns></returns>
        public static IEnumerable<long> AutosaveRestoringCheck(IEnumerable<AutosavedEntityRecordHeader> recordHeaders)
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
                                (e is LockableEntityObject
                                && !((LockableEntityObject)e).LockedByAnyoneElse)
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

        public static string GetArticleFieldValue(int contentId, string fieldName, int articleId)
        {
            return ArticleRepository.GetFieldValue(articleId, contentId, fieldName);
        }

        public static string GetArticleLinkedItems(int linkId, int articleId)
        {
            return ArticleRepository.GetLinkedItems(linkId, articleId);
        }

        public static int GetArticleIdByFieldValue(int contentId, string fieldName, string fieldValue)
        {
            return ArticleRepository.GetArticleIdByFieldValue(contentId, fieldName, fieldValue);
        }
    }
}
