using System.Diagnostics.CodeAnalysis;
using System.Web.Mvc;
using Quantumart.QP8.BLL.Services.EntityPermissions;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Controllers.Base;
using Quantumart.QP8.WebMvc.Infrastructure.ActionFilters;
using Quantumart.QP8.WebMvc.Infrastructure.Enums;
using Telerik.Web.Mvc;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class ArticlePermissionController : PermissionWithChildControllerBase
    {
        public ArticlePermissionController(IPermissionService service, IChildEntityPermissionService childContentService)
            : base(service, childContentService)
        {
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.ArticlePermissions)]
        [BackendActionContext(ActionCode.ArticlePermissions)]
        public override ActionResult Index(string tabId, int parentId) => base.Index(tabId, parentId);

        [HttpPost]
        [GridAction(EnableCustomBinding = true)]
        [ActionAuthorize(ActionCode.ArticlePermissions)]
        [BackendActionContext(ActionCode.ArticlePermissions)]
        public override ActionResult _Index(string tabId, int parentId, GridCommand command) => base._Index(tabId, parentId, command);

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.AddNewArticlePermission)]
        [BackendActionContext(ActionCode.AddNewArticlePermission)]
        public override ActionResult New(string tabId, int parentId) => base.New(tabId, parentId);

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.AddNewArticlePermission)]
        [BackendActionContext(ActionCode.AddNewArticlePermission)]
        [BackendActionLog]
        [Record]
        public override ActionResult New(string tabId, int parentId, FormCollection collection) => base.New(tabId, parentId, collection);

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.ArticlePermissionProperties)]
        [BackendActionContext(ActionCode.ArticlePermissionProperties)]
        public override ActionResult Properties(string tabId, int parentId, int id, string successfulActionCode) => base.Properties(tabId, parentId, id, successfulActionCode);

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.UpdateArticlePermission)]
        [BackendActionContext(ActionCode.UpdateArticlePermission)]
        [BackendActionLog]
        [Record(ActionCode.ArticlePermissionProperties)]
        public override ActionResult Properties(string tabId, int parentId, int id, FormCollection collection) => base.Properties(tabId, parentId, id, collection);

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.MultipleRemoveArticlePermission)]
        [BackendActionContext(ActionCode.MultipleRemoveArticlePermission)]
        [BackendActionLog]
        [Record]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public override ActionResult MultipleRemove(int parentId, int[] IDs) => base.MultipleRemove(parentId, IDs);

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
        public override ActionResult ChildIndex(string tabId, int parentId) => base.ChildIndex(tabId, parentId);

        [HttpPost]
        [GridAction(EnableCustomBinding = true)]
        [ActionAuthorize(ActionCode.ChildArticlePermissions)]
        [BackendActionContext(ActionCode.ChildArticlePermissions)]
        public override ActionResult _ChildIndex(string tabId, int parentId, int? userId, int? groupId, GridCommand command) => base._ChildIndex(tabId, parentId, userId, groupId, command);

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.MultipleChangeChildArticlePermissions)]
        [BackendActionContext(ActionCode.MultipleChangeChildArticlePermissions)]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public override ActionResult MultipleChangeAsChild(string tabId, int parentId, int[] IDs, int? userId, int? groupId) => base.MultipleChangeAsChild(tabId, parentId, IDs, userId, groupId);

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.MultipleChangeChildArticlePermissions)]
        [BackendActionContext(ActionCode.MultipleChangeChildArticlePermissions)]
        [BackendActionLog]
        [Record]
        public override ActionResult SaveMultipleChangeAsChild(string tabId, int parentId, FormCollection collection) => base.SaveMultipleChangeAsChild(tabId, parentId, collection);

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.ChangeAllChildArticlePermissions)]
        [BackendActionContext(ActionCode.ChangeAllChildArticlePermissions)]
        public override ActionResult AllChangeAsChild(string tabId, int parentId, int? userId, int? groupId) => base.AllChangeAsChild(tabId, parentId, userId, groupId);

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.ChangeAllChildArticlePermissions)]
        [BackendActionContext(ActionCode.ChangeAllChildArticlePermissions)]
        [BackendActionLog]
        [Record(ActionCode.ChangeAllChildArticlePermissions)]
        public override ActionResult AllChangeAsChild(string tabId, int parentId, FormCollection collection) => base.AllChangeAsChild(tabId, parentId, collection);

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.ChangeChildArticlePermission)]
        [BackendActionContext(ActionCode.ChangeChildArticlePermission)]
        public override ActionResult ChangeAsChild(string tabId, int parentId, int id, int? userId, int? groupId) => base.ChangeAsChild(tabId, parentId, id, userId, groupId);

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.ChangeChildArticlePermission)]
        [BackendActionContext(ActionCode.ChangeChildArticlePermission)]
        [BackendActionLog]
        [Record(ActionCode.ChangeChildArticlePermission)]
        public override ActionResult ChangeAsChild(string tabId, int parentId, FormCollection collection) => base.ChangeAsChild(tabId, parentId, collection);

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.MultipleRemoveChildArticlePermissions)]
        [BackendActionContext(ActionCode.MultipleRemoveChildArticlePermissions)]
        [BackendActionLog]
        [Record]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public override ActionResult MultipleRemoveAsChild(int parentId, int[] IDs, int? userId, int? groupId) => base.MultipleRemoveAsChild(parentId, IDs, userId, groupId);

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.RemoveAllChildArticlePermissions)]
        [BackendActionContext(ActionCode.RemoveAllChildArticlePermissions)]
        [BackendActionLog]
        [Record]
        public override ActionResult AllRemoveAsChild(int parentId, int? userId, int? groupId) => base.AllRemoveAsChild(parentId, userId, groupId);

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.RemoveChildArticlePermission)]
        [BackendActionContext(ActionCode.RemoveChildArticlePermission)]
        [BackendActionLog]
        [Record]
        public override ActionResult RemoveAsChild(int parentId, int id, int? userId, int? groupId) => base.RemoveAsChild(parentId, id, userId, groupId);

        protected override string SaveChildPermissionAction => ActionCode.SaveChildArticlePermission;

        protected override string MultipleChangeAction => ActionCode.MultipleChangeChildArticlePermissions;

        protected override string AllChangeAction => ActionCode.ChangeAllChildArticlePermissions;

        protected override string ChangeAction => ActionCode.ChangeChildArticlePermission;

        protected override string ControllerName => "ArticlePermission";
    }
}
