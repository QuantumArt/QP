using System.Web.Mvc;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.Utils;
using Quantumart.QP8.WebMvc.Extensions.Controllers;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class TreeMenuController : QPController
    {
        public JsonResult GetNode(string entityTypeCode, int entityId, int? parentEntityId, bool isFolder, bool isGroup, string groupItemCode, bool loadChildNodes)
        {
            var node = TreeMenuService.GetNode(Converter.ToNull(entityTypeCode), entityId, parentEntityId, isFolder, isGroup, Converter.ToNull(groupItemCode), loadChildNodes);
            return Json(node, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetChildNodesList(string entityTypeCode, int? parentEntityId, bool isFolder, bool isGroup, string groupItemCode)
        {
            var nodeList = TreeMenuService.GetChildNodeList(Converter.ToNull(entityTypeCode), parentEntityId, isFolder, isGroup, Converter.ToNull(groupItemCode));
            return Json(nodeList, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetSubTreeToEntity(string entityTypeCode, long parentEntityId, long entityId = 0) => Json(TreeMenuService.GetSubTreeToEntity(entityTypeCode, entityId, parentEntityId), JsonRequestBehavior.AllowGet);
    }
}
