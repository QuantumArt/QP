using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.Infrastructure.ActionFilters;
using Quantumart.QP8.WebMvc.Infrastructure.ActionResults;
using Quantumart.QP8.WebMvc.Infrastructure.Enums;
using Quantumart.QP8.WebMvc.ViewModels;
using Quantumart.QP8.WebMvc.ViewModels.QpPlugin;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class QpPluginVersionController : AuthQpController
    {
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.QpPluginVersions)]
        [EntityAuthorize(ActionTypeCode.Read, EntityTypeCode.QpPluginVersion, "parentId")]
        [BackendActionContext(ActionCode.QpPluginVersions)]
        public async Task<ActionResult> Index(string tabId, int parentId)
        {
            var model = QpPluginVersionListViewModel.Create(tabId, parentId);
            return await JsonHtml("Index", model);
        }

        [HttpPost]
        [ActionAuthorize(ActionCode.QpPluginVersions)]
        [BackendActionContext(ActionCode.QpPluginVersions)]
        public ActionResult _Index(
            string tabId,
            int parentId,
            int page,
            int pageSize,
            string orderBy)
        {
            var listCommand = GetListCommand(page, pageSize, orderBy);
            var serviceResult = QpPluginVersionService.List(parentId, listCommand);
            var result = Mapper.Map<List<QpPluginVersion>, List<QpPluginVersionListItem>>(serviceResult);
            return new TelerikResult(result, result.Count);
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.CompareQpPluginVersionWithCurrent)]
        [EntityAuthorize(ActionTypeCode.Read, EntityTypeCode.QpPluginVersion, "parentId")]
        [BackendActionContext(ActionCode.CompareQpPluginVersionWithCurrent)]
        public async Task<ActionResult> CompareWithCurrent(string tabId, int parentId, int id, bool? boundToExternal)
        {
            var version = QpPluginVersionService.GetMergedVersion(new[] { id, ArticleVersion.CurrentVersionId }, parentId);
            var model = QpPluginVersionViewModel.Create(version, tabId, parentId, boundToExternal);
            model.ViewType = QpPluginVersionViewType.CompareWithCurrent;
            return await JsonHtml("Compare", model);
        }


        [HttpPost]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.CompareQpPluginVersions)]
        [EntityAuthorize(ActionTypeCode.Read, EntityTypeCode.QpPluginVersion, "parentId")]
        [BackendActionContext(ActionCode.CompareQpPluginVersions)]
        public async Task<ActionResult> Compare(string tabId, int parentId, [FromBody] SelectedItemsViewModel selModel, bool? boundToExternal)
        {
            var version = QpPluginVersionService.GetMergedVersion(selModel.Ids, parentId);
            var model = QpPluginVersionViewModel.Create(version, tabId, parentId, boundToExternal);
            model.ViewType = QpPluginVersionViewType.CompareVersions;
            return await JsonHtml("Compare", model);
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.PreviewQpPluginVersion)]
        [EntityAuthorize(ActionTypeCode.Read, EntityTypeCode.Article, "parentId")]
        [BackendActionContext(ActionCode.PreviewQpPluginVersion)]
        public async Task<ActionResult> Properties(string tabId, int parentId, int id, string successfulActionCode, bool? boundToExternal)
        {
            var version = QpPluginVersionService.Read(id, parentId);
            var model = QpPluginVersionViewModel.Create(version, tabId, parentId, successfulActionCode, boundToExternal);
            model.ViewType = QpPluginVersionViewType.Preview;
            return await JsonHtml("Properties", model);
        }

    }
}
