using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Repository.ArticleRepositories.SearchParsers;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.Infrastructure.ActionFilters;
using Quantumart.QP8.WebMvc.Infrastructure.Enums;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class EntityObjectController : AuthQpController
    {
        private readonly ArticleFullTextSearchQueryParser _parser;

        public EntityObjectController(ArticleFullTextSearchQueryParser parser)
        {
            _parser = parser;
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
        public JsonResult GetChildList([FromBody]ChildListQuery query)
        {
            query.Parser = _parser;
            var data = EntityObjectService.GetEntityTreeItems(query);
            return Json(data);
        }

        [HttpPost]
        public JsonResult GetSimpleList([FromBody]SimpleListQuery query)
        {
            return Json(EntityObjectService.SimpleList(query));
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
        public JsonResult GetParentIdsForTree([FromBody]ParentIdsForTreeQuery query)
        {
            return Json(EntityObjectService.GetParentIdsForTree(query));
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
        public JsonResult UnlockAllEntities()
        {
            EntityObjectService.UnlockAllEntitiesLockedByCurrentUser();
            return Json(null);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        public JsonResult AutosaveRestoringCheck([FromBody] AutoSaveData data)
        {
            return Json(new
            {
                success = true,
                approvedRecordIDs = EntityObjectService.AutosaveRestoringCheck(data.RecordHeaders)
            });
        }
    }
}
