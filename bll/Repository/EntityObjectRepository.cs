using Quantumart.QP8.BLL.Repository.Articles;
using Quantumart.QP8.Constants;
using Quantumart.QP8.DAL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

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
                return Common.Lock(QPConnectionScope.Current.DbConnection, item.EntityTypeCode, item.Id, userId);
            }
        }

        internal static DateTime? Lock(EntityObject item)
        {
            return Lock(item, QPContext.CurrentUserId);
        }

        internal static void UnLock(EntityObject item)
        {
            Lock(item, null);
        }

        internal static DateTime? CaptureLock(EntityObject item)
        {
            UnLock(item);
            return Lock(item);
        }


        internal static EntityObject GetById<T>(int id) where T : EntityObject
        {
            if (typeof(T) == typeof(Article))
                return ArticleRepository.GetById(id);
            if (typeof(T) == typeof(Site))
                return SiteRepository.GetById(id);
            if (typeof(T) == typeof(Content))
                return ContentRepository.GetById(id);
            if (typeof(T) == typeof(Field))
                return FieldRepository.GetById(id);

            throw new Exception("Unsupported entity object type");
        }

        private class CustomerObject : EntityObject
        {
            public override string EntityTypeCode => Constants.EntityTypeCode.CustomerCode;
        }

        internal static IEnumerable<EntityObject> GetList(string entityTypeCode, IList<int> ids)
        {
            if (entityTypeCode.Equals(EntityTypeCode.CustomerCode, StringComparison.InvariantCultureIgnoreCase) && ids.Any())
                return new EntityObject[] { new CustomerObject { Id = ids.First(), Modified = DateTime.MinValue, IsReadOnly = true } };
            if (entityTypeCode.Equals(EntityTypeCode.Site, StringComparison.InvariantCultureIgnoreCase))
                return SiteRepository.GetList(ids);
            if (entityTypeCode.Equals(EntityTypeCode.Content, StringComparison.InvariantCultureIgnoreCase))
                return ContentRepository.GetList(ids);
            if (entityTypeCode.Equals(EntityTypeCode.Field, StringComparison.InvariantCultureIgnoreCase))
                return FieldRepository.GetList(ids);
            if (entityTypeCode.Equals(EntityTypeCode.Article, StringComparison.InvariantCultureIgnoreCase))
                return ArticleRepository.GetList(ids);
            if (entityTypeCode.Equals(EntityTypeCode.Notification, StringComparison.InvariantCultureIgnoreCase))
                return NotificationRepository.GetList(ids);
            if (entityTypeCode.Equals(EntityTypeCode.VisualEditorPlugin, StringComparison.InvariantCultureIgnoreCase))
                return VisualEditorRepository.GetPluginList(ids);
            if (entityTypeCode.Equals(EntityTypeCode.VisualEditorCommand, StringComparison.InvariantCultureIgnoreCase))
                return VisualEditorRepository.GetCommandList(ids);
            if (entityTypeCode.Equals(EntityTypeCode.VisualEditorStyle, StringComparison.InvariantCultureIgnoreCase))
                return VisualEditorRepository.GetStyleList(ids);
            if (entityTypeCode.Equals(EntityTypeCode.StatusType, StringComparison.InvariantCultureIgnoreCase))
                return StatusTypeRepository.GetList(ids);
            if (entityTypeCode.Equals(EntityTypeCode.Workflow, StringComparison.InvariantCultureIgnoreCase))
                return WorkflowRepository.GetList(ids);
            if (entityTypeCode.Equals(EntityTypeCode.PageTemplate, StringComparison.InvariantCultureIgnoreCase))
                return PageTemplateRepository.GetPageTemplateList(ids);
            if (entityTypeCode.Equals(EntityTypeCode.User, StringComparison.InvariantCultureIgnoreCase))
                return UserRepository.GetList(ids);
            if (entityTypeCode.Equals(EntityTypeCode.UserGroup, StringComparison.InvariantCultureIgnoreCase))
                return UserGroupRepository.GetList(ids);
            if (entityTypeCode.Equals(EntityTypeCode.TemplateObjectFormat, StringComparison.InvariantCultureIgnoreCase))
                return FormatRepository.GetList(ids, false);
            if (entityTypeCode.Equals(EntityTypeCode.PageObjectFormat, StringComparison.InvariantCultureIgnoreCase))
                return FormatRepository.GetList(ids, true);
            if (entityTypeCode.Equals(EntityTypeCode.PageObject, StringComparison.InvariantCultureIgnoreCase) || entityTypeCode.Equals(EntityTypeCode.TemplateObject, StringComparison.InvariantCultureIgnoreCase))
                return ObjectRepository.GetList(ids);
            if (entityTypeCode.Equals(EntityTypeCode.VirtualContent, StringComparison.InvariantCultureIgnoreCase))
                return VirtualContentRepository.GetList(ids);
            if (entityTypeCode.Equals(EntityTypeCode.Page, StringComparison.InvariantCultureIgnoreCase))
                return PageRepository.GetList(ids);
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
            return QPContext.EFContext.CheckEntityExistence(entityTypeCode, entityId);
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
            return QPContext.EFContext.GetEntityName(entityTypeCode, entityId, parentEntityId);
        }

        /// <summary>
        /// Возвращает идентификатор родительской сущности
        /// </summary>
        /// <param name="entityTypeCode">код типа сущности</param>
        /// <param name="entityId">идентификатор сущности</param>
        /// <returns>идентификатор родительской сущности</returns>
        internal static int? GetParentId(string entityTypeCode, int entityId)
        {
            using (new QPConnectionScope())
            {
                return Common.GetParentEntityId(QPConnectionScope.Current.DbConnection, entityTypeCode, entityId);
            }
        }

        internal static int[] GetParentIdsForTree(string entityTypeCode, int[] ids)
        {
            using (new QPConnectionScope())
            {
                return Common.GetParentEntityIdsForTree(QPConnectionScope.Current.DbConnection, entityTypeCode, ids);
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
            IList<EntityInfo> result = null;
            using (new QPConnectionScope())
            {
                var dt = Common.GetBreadCrumbsList(QPConnectionScope.Current.DbConnection, QPContext.CurrentUserId, entityTypeCode, entityId, parentEntityId, oneLevel);
                if (dt != null)
                {
                    result = dt.AsEnumerable().Select(EntityInfo.Create).ToList();
                }
            }

            var customerCodeInfo = result?.SingleOrDefault(n => n.Code == EntityTypeCode.CustomerCode);
            if (customerCodeInfo != null)
            {
                customerCodeInfo.Title = QPContext.CurrentCustomerCode;
            }

            return result;
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
                    return Common.GetTreeIdsToLoad(scope.DbConnection, $"CONTENT_{parentEntityId}_UNITED", ContentRepository.GetTreeFieldName(parentEntityId), "CONTENT_ITEM_ID", selectItemIds);
                }

                return Common.GetTreeIdsToLoad(scope.DbConnection, entityTypeCode, selectItemIds);
            }
        }
    }
}
