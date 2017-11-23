using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using QP8.Infrastructure.Web.AspNet.ActionResults;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Repository.ArticleRepositories.SearchParsers;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.Extensions.ModelBinders;
using Quantumart.QP8.WebMvc.Infrastructure.ActionFilters;
using Quantumart.QP8.WebMvc.Infrastructure.Enums;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class EntityObjectController : QPController
    {
        [HttpGet]
        public JsonNetResult<bool> CheckExistence(string entityTypeCode, int entityId) => EntityObjectService.CheckExistence(entityTypeCode, entityId);

        /// <summary>
        /// Проверяет сущность на наличие рекурсивных связей.
        /// </summary>
        /// <param name="entityTypeCode">Код типа сущности.</param>
        /// <param name="entityId">Идентификатор сущности.</param>
        /// <returns>Результат проверки (true - есть рекурсивные связи; false - нет).</returns>
        [HttpGet]
        public JsonNetResult<bool> CheckPresenceSelfRelations(string entityTypeCode, int entityId) => EntityObjectService.CheckPresenceSelfRelations(entityTypeCode, entityId);

        /// <summary>
        /// Проверяет сущность на наличие вариаций.
        /// </summary>
        /// <param name="entityTypeCode">Код типа сущности.</param>
        /// <param name="entityId">Идентификатор сущности.</param>
        /// <returns>Результат проверки (true - есть вариации; false - нет).</returns>
        [HttpGet]
        public JsonNetResult<bool> CheckForVariations(string entityTypeCode, int entityId) => EntityObjectService.CheckForVariations(entityTypeCode, entityId);

        [HttpGet]
        public JsonNetResult<EntityTreeItem> GetByTypeAndIdForTree(string entityTypeCode, int entityId, bool loadChilds, string filter) => EntityObjectService.GetByTypeAndIdForTree(entityTypeCode, entityId, loadChilds, filter);

        /// <summary>
        /// Возвращает список дочерних сущностей.
        /// </summary>
        [HttpPost]
        public JsonNetResult<IList<EntityTreeItem>> GetChildList(
            string entityTypeCode,
            int? parentEntityId,
            int? entityId,
            bool returnSelf,
            string filter,
            string hostFilter,
            string selectItemIDs,
            [ModelBinder(typeof(JsonStringModelBinder<ArticleSearchQueryParam[]>))] ArticleSearchQueryParam[] searchQuery,
            [ModelBinder(typeof(JsonStringModelBinder<ArticleContextQueryParam[]>))] ArticleContextQueryParam[] contextQuery)
        {
            var ftsParser = DependencyResolver.Current.GetService<ArticleFullTextSearchQueryParser>();
            var data = EntityObjectService.GetEntityTreeItems(entityTypeCode, parentEntityId, entityId, returnSelf, filter, hostFilter, selectItemIDs, searchQuery, contextQuery, ftsParser);
            return new JsonNetResult<IList<EntityTreeItem>>(data);
        }

        /// <summary>
        /// Возвращает упрощенный список сущностей.
        /// </summary>
        [HttpPost]
        public JsonNetResult<IList<ListItem>> GetSimpleList(string entityTypeCode, int parentEntityId, int? entityId, int? listId, int selectionMode, int[] selectedEntitiesIDs, string filter, int testEntityId = 0)
        {
            var listSelectionMode = (ListSelectionMode)Enum.Parse(typeof(ListSelectionMode), selectionMode.ToString());
            return EntityObjectService.SimpleList(
                entityTypeCode,
                parentEntityId,
                entityId > 0 ? entityId : null,
                listId > 0 ? listId : null,
                listSelectionMode,
                selectedEntitiesIDs,
                filter,
                testEntityId
            );
        }

        /// <summary>
        /// Возвращает название сущности.
        /// </summary>
        [HttpGet]
        public JsonNetResult<string> GetName(string entityTypeCode, int entityId, int? parentEntityId) => EntityObjectService.GetName(entityTypeCode, entityId, parentEntityId ?? 0);

        [HttpGet]
        public JsonNetResult<int?> GetParentId(string entityTypeCode, int entityId) => EntityObjectService.GetParentId(entityTypeCode, entityId);

        [HttpPost]
        public JsonNetResult<int[]> GetParentIdsForTree(string entityTypeCode, int[] ids) => EntityObjectService.GetParentIdsForTree(entityTypeCode, ids);

        [HttpGet]
        public JsonNetResult<IEnumerable<EntityInfo>> GetBreadCrumbsList(string entityTypeCode, long entityId, long? parentEntityId, string actionCode) => EntityObjectService.GetBreadCrumbsList(entityTypeCode, entityId, parentEntityId, actionCode).ToList();

        [HttpGet]
        public JsonNetResult<IEnumerable<EntityInfo>> GetParentInfo(string entityTypeCode, long entityId, long? parentEntityId)
        {
            var data = EntityObjectService.GetParentInfo(entityTypeCode, entityId, parentEntityId);
            return new JsonNetResult<IEnumerable<EntityInfo>>(data);
        }

        [HttpGet]
        public JsonNetResult<string> GetArticleFieldValue(int contentId, string fieldName, int articleId) => EntityObjectService.GetArticleFieldValue(contentId, fieldName, articleId);

        [HttpGet]
        public JsonNetResult<Dictionary<int, string>> GetContentFieldValues(int contentId, string fieldName) => EntityObjectService.GetContentFieldValues(contentId, fieldName);
        

        [HttpGet]
        public JsonNetResult<string> GetArticleLinkedItems(int linkId, int articleId) => EntityObjectService.GetArticleLinkedItems(linkId, articleId);

        [HttpGet]
        public JsonNetResult<int> GetArticleIdByFieldValue(int contentId, string fieldName, string fieldValue) => EntityObjectService.GetArticleIdByFieldValue(contentId, fieldName, fieldValue);

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope]
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
            return new
            {
                success = true,
                approvedRecordIDs = result
            };
        }
    }
}
