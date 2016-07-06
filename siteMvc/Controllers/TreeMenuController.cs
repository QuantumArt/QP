using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using System.Web.Mvc;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class TreeMenuController : QPController
    {
        [HttpGet]
        public JsonResult GetNode(string entityTypeCode, int entityId, int? parentEntityId, bool isFolder, bool isGroup, string groupItemCode, bool loadChildNodes)
        {
            var node = TreeMenuService.GetNode(Utils.Converter.ToNull(entityTypeCode), entityId, parentEntityId, isFolder, isGroup, Utils.Converter.ToNull(groupItemCode), loadChildNodes);
            return Json(node, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetChildNodesList(string entityTypeCode, int? parentEntityId, bool isFolder, bool isGroup, string groupItemCode)
        {
            var nodeList = TreeMenuService.GetChildNodeList(Utils.Converter.ToNull(entityTypeCode), parentEntityId, isFolder, isGroup, Utils.Converter.ToNull(groupItemCode));
            return Json(nodeList, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetSubTreeToEntity(string entityTypeCode, long parentEntityId, long entityId = 0)
        {
            return Json(TreeMenuService.GetSubTreeToEntity(entityTypeCode, entityId, parentEntityId), JsonRequestBehavior.AllowGet);
        }
    }
}
