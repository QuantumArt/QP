using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Quantumart.QP8;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.WebMvc.Extensions.Controllers;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class TreeMenuController : QPController
    {
		[HttpGet]
		public JsonResult GetNode(string entityTypeCode, int entityId, int? parentEntityId, bool isFolder, bool isGroup, string groupItemCode, bool loadChildNodes)
        {
			TreeNode node = TreeMenuService.GetNode(Utils.Converter.ToNull(entityTypeCode), entityId, parentEntityId, isFolder, isGroup, Utils.Converter.ToNull(groupItemCode), loadChildNodes);
			return Json(node, JsonRequestBehavior.AllowGet);
        }

		[HttpGet]
		public JsonResult GetChildNodesList(string entityTypeCode, int? parentEntityId, bool isFolder, bool isGroup, string groupItemCode)
		{
			IEnumerable<TreeNode> nodeList = TreeMenuService.GetChildNodeList(Utils.Converter.ToNull(entityTypeCode), parentEntityId, isFolder, isGroup, Utils.Converter.ToNull(groupItemCode));
			return Json(nodeList, JsonRequestBehavior.AllowGet);
		}

        /// <summary>
        /// Возвращает поддерево меню от корня до ближайшего существующего нода для параметров
        /// </summary>
        /// <param name="entityTypeCode"></param>
        /// <param name="entityId"></param>
        /// <param name="parentEntityId"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetSubTreeToEntity(string entityTypeCode, long parentEntityId, long entityId = 0)
        {
            return Json(
                TreeMenuService.GetSubTreeToEntity(entityTypeCode, entityId, parentEntityId), 
                JsonRequestBehavior.AllowGet
            );
        }
    }
}