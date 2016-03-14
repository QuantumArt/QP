using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Repository.Articles;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.WebMvc.Extensions.ActionFilters;
using Quantumart.QP8.WebMvc.Extensions.ActionResults;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.Extensions.ModelBinders;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Quantumart.QP8.BLL.Services.DTO;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class EntityObjectController : QPController
    {
        [HttpGet]
        public JsonNetResult<bool> CheckExistence(string entityTypeCode, int entityId)
        {
            return EntityObjectService.CheckExistence(entityTypeCode, entityId);
        }

        /// <summary>
        /// Проверяет сущность на наличие рекурсивных связей
        /// </summary>
        /// <param name="entityTypeCode">код типа сущности</param>
        /// <param name="entityId">идентификатор сущности</param>
        /// <returns>результат проверки (true - есть рекурсивные связи; false - нет)</returns>
        [HttpGet]
        public JsonNetResult<bool> CheckPresenceSelfRelations(string entityTypeCode, int entityId)
        {
            return EntityObjectService.CheckPresenceSelfRelations(entityTypeCode, entityId);
        }

        /// <summary>
        /// Проверяет сущность на наличие вариаций
        /// </summary>
        /// <param name="entityTypeCode">код типа сущности</param>
        /// <param name="entityId">идентификатор сущности</param>
        /// <returns>результат проверки (true - есть вариации; false - нет)</returns>
        [HttpGet]
        public JsonNetResult<bool> CheckForVariations(string entityTypeCode, int entityId)
        {
            return EntityObjectService.CheckForVariations(entityTypeCode, entityId);
        }

        [HttpGet]
        public JsonNetResult<EntityTreeItem> GetByTypeAndIdForTree(string entityTypeCode, int entityId, bool loadChilds, string filter)
        {
            return EntityObjectService.GetByTypeAndIdForTree(entityTypeCode, entityId, loadChilds, filter);
        }

        /// <summary>
        /// Возвращает список дочерних сущностей
        /// </summary>
        /// <param name="entityTypeCode">код типа сущности</param>
        /// <param name="parentEntityId">идентификатор родительской сущности</param>
        /// <param name="entityId">идентификатор сущности</param>
        /// <param name="returnSelf">возвращает саму сущность по entityId (необходимо в случае генерации корня поддерева)</param>
        /// <param name="filter"></param>
        /// <param name="selectItemIDs"></param>
        /// <param name="searchQuery"></param>
        /// <param name="contextQuery"></param>
        /// <returns>список дочерних сущностей</returns>
        [HttpPost]
        public JsonNetResult<IList<EntityTreeItem>> GetChildList(string entityTypeCode,
            int? parentEntityId,
            int? entityId,
            bool returnSelf,
            string filter,
            string selectItemIDs,
            [ModelBinder(typeof(JsonStringModelBinder<ArticleSearchQueryParam[]>))] ArticleSearchQueryParam[] searchQuery,
            [ModelBinder(typeof(JsonStringModelBinder<ArticleContextQueryParam[]>))] ArticleContextQueryParam[] contextQuery)
        {
            var ftsParser = DependencyResolver.Current.GetService<ArticleFullTextSearchQueryParser>();
            var data = EntityObjectService.GetEntityTreeItems(entityTypeCode, parentEntityId, entityId, returnSelf, filter, selectItemIDs, searchQuery, contextQuery, ftsParser);
            return new JsonNetResult<IList<EntityTreeItem>>(data);
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
        [HttpPost]
        public JsonNetResult<IList<ListItem>> GetSimpleList(string entityTypeCode, int parentEntityId, int? entityId, int? listId, int selectionMode, int[] selectedEntitiesIDs, string filter, int testEntityId = 0)
        {
            var listSelectionMode = (Constants.ListSelectionMode)Enum.Parse(typeof(Constants.ListSelectionMode), selectionMode.ToString());
            return EntityObjectService.SimpleList(entityTypeCode, parentEntityId,
                entityId > 0 ? entityId : null,
                listId > 0 ? listId : null,
                listSelectionMode, selectedEntitiesIDs, filter, testEntityId);
        }

        /// <summary>
        /// Возвращает название сущности
        /// </summary>
        /// <param name="entityTypeCode">код типа сущности</param>
        /// <param name="entityId">идентификатор сущности</param>
        /// <param name="parentEntityId"></param>
        /// <returns>название сущности</returns>
        [HttpGet]
        public JsonNetResult<string> GetName(string entityTypeCode, int entityId, int? parentEntityId)
        {
            return EntityObjectService.GetName(entityTypeCode, entityId, parentEntityId ?? 0);
        }

        [HttpGet]
        public JsonNetResult<int?> GetParentId(string entityTypeCode, int entityId)
        {
            return EntityObjectService.GetParentId(entityTypeCode, entityId);
        }

        [HttpPost]
        public JsonNetResult<int[]> GetParentIdsForTree(string entityTypeCode, int[] ids)
        {
            return EntityObjectService.GetParentIdsForTree(entityTypeCode, ids);
        }

        [HttpGet]
        public JsonNetResult<IEnumerable<EntityInfo>> GetBreadCrumbsList(string entityTypeCode, long entityId, long? parentEntityId, string actionCode)
        {
            var data = EntityObjectService.GetBreadCrumbsList(entityTypeCode, entityId, parentEntityId, actionCode);
            return new JsonNetResult<IEnumerable<EntityInfo>>(data);
        }

        [HttpGet]
        public JsonNetResult<IEnumerable<EntityInfo>> GetParentInfo(string entityTypeCode, long entityId, long? parentEntityId)
        {
            var data = EntityObjectService.GetParentInfo(entityTypeCode, entityId, parentEntityId);
            return new JsonNetResult<IEnumerable<EntityInfo>>(data);
        }

        [HttpGet]
        public JsonNetResult<string> GetArticleFieldValue(int contentId, string fieldName, int articleId)
        {
            return EntityObjectService.GetArticleFieldValue(contentId, fieldName, articleId);
        }

        [HttpGet]
        public JsonNetResult<string> GetArticleLinkedItems(int linkId, int articleId)
        {
            return EntityObjectService.GetArticleLinkedItems(linkId, articleId);
        }

        [HttpGet]
        public JsonNetResult<int> GetArticleIdByFieldValue(int contentId, string fieldName, string fieldValue)
        {
            return EntityObjectService.GetArticleIdByFieldValue(contentId, fieldName, fieldValue);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope(ConnectionScopeMode.TransactionOn)]
        public JsonNetResult<object> UnlockAllEntities()
        {
            EntityObjectService.UnlockAllEntitiesLockedByCurrentUser();
            return null;
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        public JsonNetResult<object> AutosaveRestoringCheck([ModelBinder(typeof(JsonStringModelBinder<AutosavedEntityRecordHeader[]>))] AutosavedEntityRecordHeader[] recordHeaders)
        {
            var result = EntityObjectService.AutosaveRestoringCheck(recordHeaders);
            return new JsonResult
            {
                Data = new
                {
                    success = true,
                    approvedRecordIDs = result
                }
            };
        }
    }
}
