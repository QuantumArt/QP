using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
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
        private readonly IServiceProvider _serviceProvider;

        public EntityObjectController(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public JsonResult CheckExistence(string entityTypeCode, int entityId)
        {
            return Json(EntityObjectService.CheckExistence(entityTypeCode, entityId));
        }

        public JsonResult CheckPresenceSelfRelations(string entityTypeCode, int entityId)
        {
            return Json(EntityObjectService.CheckPresenceSelfRelations(entityTypeCode, entityId));
        }

        public JsonResult CheckForVariations(string entityTypeCode, int entityId)
        {
            return Json(EntityObjectService.CheckForVariations(entityTypeCode, entityId));
        }

        public JsonResult GetByTypeAndIdForTree(string entityTypeCode, int entityId, bool loadChilds, string filter)
        {
            return Json(EntityObjectService.GetByTypeAndIdForTree(entityTypeCode, entityId, loadChilds, filter));
        }

        [HttpPost]
        public JsonResult GetChildList(
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
            var ftsParser = _serviceProvider.GetRequiredService<ArticleFullTextSearchQueryParser>();
            var data = EntityObjectService.GetEntityTreeItems(entityTypeCode, parentEntityId, entityId, returnSelf, filter, hostFilter, selectItemIDs, searchQuery, contextQuery, ftsParser);
            return Json(data);
        }

        [HttpPost]
        public JsonResult GetSimpleList(string entityTypeCode, int parentEntityId, int? entityId, int? listId, int selectionMode, int[] selectedEntitiesIDs, string filter, int testEntityId = 0)
        {
            return Json(EntityObjectService.SimpleList(
                entityTypeCode,
                parentEntityId,
                entityId > 0 ? entityId : null,
                listId > 0 ? listId : null,
                (ListSelectionMode)Enum.Parse(typeof(ListSelectionMode), selectionMode.ToString()),
                selectedEntitiesIDs,
                filter,
                testEntityId
            ));
        }

        public JsonResult GetName(string entityTypeCode, int entityId, int? parentEntityId)
        {
            return Json(EntityObjectService.GetName(entityTypeCode, entityId, parentEntityId ?? 0));
        }

        public JsonResult GetParentId(string entityTypeCode, int entityId)
        {
            return Json(EntityObjectService.GetParentId(entityTypeCode, entityId));
        }

        [HttpPost]
        public JsonResult GetParentIdsForTree(string entityTypeCode, int[] ids)
        {
            return Json(EntityObjectService.GetParentIdsForTree(entityTypeCode, ids));
        }

        public JsonResult GetBreadCrumbsList(string entityTypeCode, long entityId, long? parentEntityId, string actionCode)
        {
            return Json(EntityObjectService.GetBreadCrumbsList(entityTypeCode, entityId, parentEntityId, actionCode)?.ToList());
        }

        public JsonResult GetParentInfo(string entityTypeCode, long entityId, long? parentEntityId)
        {
            return Json(EntityObjectService.GetParentInfo(entityTypeCode, entityId, parentEntityId)?.ToList());
        }

        public JsonResult GetArticleFieldValue(int contentId, string fieldName, int articleId)
        {
            return Json(EntityObjectService.GetArticleFieldValue(contentId, fieldName, articleId));
        }

        public JsonResult GetContentFieldValues(int contentId, string fieldName)
        {
            return Json(EntityObjectService.GetContentFieldValues(contentId, fieldName));
        }

        public JsonResult GetArticleLinkedItems(int linkId, int articleId)
        {
            return Json(EntityObjectService.GetArticleLinkedItems(linkId, articleId));
        }

        public JsonResult GetArticleIdByFieldValue(int contentId, string fieldName, string fieldValue)
        {
            return Json(EntityObjectService.GetArticleIdByFieldValue(contentId, fieldName, fieldValue));
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope]
        public JsonResult UnlockAllEntities()
        {
            EntityObjectService.UnlockAllEntitiesLockedByCurrentUser();
            return Json(null);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        public JsonResult AutosaveRestoringCheck([ModelBinder(typeof(JsonStringModelBinder<AutosavedEntityRecordHeader[]>))] AutosavedEntityRecordHeader[] recordHeaders)
        {
            return Json(new
            {
                success = true,
                approvedRecordIDs = EntityObjectService.AutosaveRestoringCheck(recordHeaders)
            });
        }
    }
}
