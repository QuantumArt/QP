using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Quantumart.QP8.BLL.Services.EntityPermissions;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Controllers.Base;
using Quantumart.QP8.WebMvc.Infrastructure.ActionFilters;
using Quantumart.QP8.WebMvc.Infrastructure.Enums;
using Quantumart.QP8.WebMvc.ViewModels;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class ArticlePermissionController : PermissionWithChildControllerBase
    {
        public ArticlePermissionController(ArticlePermissionService service, ChildArticlePermissionService childContentService)
            : base(service, childContentService)
        {
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.ArticlePermissions)]
        [BackendActionContext(ActionCode.ArticlePermissions)]
        public override async Task<ActionResult> Index(string tabId, int parentId)
        {
            return await base.Index(tabId, parentId);
        }

        [HttpPost]
        [ActionAuthorize(ActionCode.ArticlePermissions)]
        [BackendActionContext(ActionCode.ArticlePermissions)]
        public override ActionResult _Index(
            string tabId,
            int parentId,
            int page,
            int pageSize,
            string orderBy) => base._Index(
                tabId,
                parentId,
                page,
                pageSize,
                orderBy);

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.AddNewArticlePermission)]
        [BackendActionContext(ActionCode.AddNewArticlePermission)]
        public override async Task<ActionResult> New(string tabId, int parentId)
        {
            return await base.New(tabId, parentId);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.AddNewArticlePermission)]
        [BackendActionContext(ActionCode.AddNewArticlePermission)]
        [BackendActionLog]
        [Record]
        public override async Task<ActionResult> New(string tabId, int parentId, IFormCollection collection)
        {
            return await base.New(tabId, parentId, collection);
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.ArticlePermissionProperties)]
        [BackendActionContext(ActionCode.ArticlePermissionProperties)]
        public override async Task<ActionResult> Properties(string tabId, int parentId, int id, string successfulActionCode)
        {
            return await base.Properties(tabId, parentId, id, successfulActionCode);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.UpdateArticlePermission)]
        [BackendActionContext(ActionCode.UpdateArticlePermission)]
        [BackendActionLog]
        [Record(ActionCode.ArticlePermissionProperties)]
        public override async Task<ActionResult> Properties(string tabId, int parentId, int id, IFormCollection collection)
        {
            return await base.Properties(tabId, parentId, id, collection);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.MultipleRemoveArticlePermission)]
        [BackendActionContext(ActionCode.MultipleRemoveArticlePermission)]
        [BackendActionLog]
        [Record]
        public override ActionResult MultipleRemove(int parentId, [FromBody] SelectedItemsViewModel model)
        {
            return base.MultipleRemove(parentId, model);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.RemoveArticlePermission)]
        [BackendActionContext(ActionCode.RemoveArticlePermission)]
        [BackendActionLog]
        [Record]
        public override ActionResult Remove(int parentId, int id) => base.Remove(parentId, id);

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.ChildArticlePermissions)]
        [BackendActionContext(ActionCode.ChildArticlePermissions)]
        public override async Task<ActionResult> ChildIndex(string tabId, int parentId)
        {
            return await base.ChildIndex(tabId, parentId);
        }

        [HttpPost]
        [ActionAuthorize(ActionCode.ChildArticlePermissions)]
        [BackendActionContext(ActionCode.ChildArticlePermissions)]
        public override ActionResult _ChildIndex(
            string tabId,
            int parentId,
            int? userId,
            int? groupId,
            int page,
            int pageSize,
            string orderBy) => base._ChildIndex(tabId, parentId, userId, groupId, page, pageSize, orderBy);

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.MultipleChangeChildArticlePermissions)]
        [BackendActionContext(ActionCode.MultipleChangeChildArticlePermissions)]
        public override async Task<ActionResult> MultipleChangeAsChild(string tabId, int parentId, [FromBody] SelectedItemsViewModel model, int? userId, int? groupId)
        {
            return await base.MultipleChangeAsChild(tabId, parentId, model, userId, groupId);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.MultipleChangeChildArticlePermissions)]
        [BackendActionContext(ActionCode.MultipleChangeChildArticlePermissions)]
        [BackendActionLog]
        [Record]
        public override async Task<ActionResult> SaveMultipleChangeAsChild(string tabId, int parentId, IFormCollection collection)
        {
            return await base.SaveMultipleChangeAsChild(tabId, parentId, collection);
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.ChangeAllChildArticlePermissions)]
        [BackendActionContext(ActionCode.ChangeAllChildArticlePermissions)]
        public override async Task<ActionResult> AllChangeAsChild(string tabId, int parentId, int? userId, int? groupId)
        {
            return await base.AllChangeAsChild(tabId, parentId, userId, groupId);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.ChangeAllChildArticlePermissions)]
        [BackendActionContext(ActionCode.ChangeAllChildArticlePermissions)]
        [BackendActionLog]
        [Record(ActionCode.ChangeAllChildArticlePermissions)]
        public override async Task<ActionResult> AllChangeAsChild(string tabId, int parentId, IFormCollection collection)
        {
            return await base.AllChangeAsChild(tabId, parentId, collection);
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.ChangeChildArticlePermission)]
        [BackendActionContext(ActionCode.ChangeChildArticlePermission)]
        public override async Task<ActionResult> ChangeAsChild(string tabId, int parentId, int id, int? userId, int? groupId)
        {
            return await base.ChangeAsChild(tabId, parentId, id, userId, groupId);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.ChangeChildArticlePermission)]
        [BackendActionContext(ActionCode.ChangeChildArticlePermission)]
        [BackendActionLog]
        [Record(ActionCode.ChangeChildArticlePermission)]
        public override async Task<ActionResult> ChangeAsChild(string tabId, int parentId, IFormCollection collection)
        {
            return await base.ChangeAsChild(tabId, parentId, collection);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.MultipleRemoveChildArticlePermissions)]
        [BackendActionContext(ActionCode.MultipleRemoveChildArticlePermissions)]
        [BackendActionLog]
        [Record]
        public override ActionResult MultipleRemoveAsChild(int parentId, [FromBody] SelectedItemsViewModel model, int? userId, int? groupId)
        {
            var result = base.MultipleRemoveAsChild(parentId, model, userId, groupId);
            PersistUserAndGroupIds(userId, groupId);
            return result;
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.RemoveAllChildArticlePermissions)]
        [BackendActionContext(ActionCode.RemoveAllChildArticlePermissions)]
        [BackendActionLog]
        [Record]
        public override ActionResult AllRemoveAsChild(int parentId, int? userId, int? groupId)
        {
            var result = base.AllRemoveAsChild(parentId, userId, groupId);
            PersistUserAndGroupIds(userId, groupId);
            return result;
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.RemoveChildArticlePermission)]
        [BackendActionContext(ActionCode.RemoveChildArticlePermission)]
        [BackendActionLog]
        [Record]
        public override ActionResult RemoveAsChild(int parentId, int id, int? userId, int? groupId)
        {
            var result = base.RemoveAsChild(parentId, id, userId, groupId);
            PersistUserAndGroupIds(userId, groupId);
            return result;
        }

        protected override string SaveChildPermissionAction => ActionCode.SaveChildArticlePermission;

        protected override string MultipleChangeAction => ActionCode.MultipleChangeChildArticlePermissions;

        protected override string AllChangeAction => ActionCode.ChangeAllChildArticlePermissions;

        protected override string ChangeAction => ActionCode.ChangeChildArticlePermission;

        protected override string ControllerName => "ArticlePermission";
    }
}
