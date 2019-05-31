using System.Web.Mvc;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.Extensions.Helpers;
using Quantumart.QP8.WebMvc.Infrastructure.ActionFilters;
using Quantumart.QP8.WebMvc.Infrastructure.ActionResults;
using Quantumart.QP8.WebMvc.Infrastructure.Enums;
using Quantumart.QP8.WebMvc.ViewModels.UserGroup;
using Telerik.Web.Mvc;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class UserGroupController : QPController
    {
        private readonly IUserGroupService _service;

        public UserGroupController(IUserGroupService service)
        {
            _service = service;
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.UserGroups)]
        [BackendActionContext(ActionCode.UserGroups)]
        public ActionResult Index(string tabId, int parentId)
        {
            var result = _service.InitList(parentId);
            var model = UserGroupListViewModel.Create(result, tabId, parentId);
            return JsonHtml("Index", model);
        }

        [HttpPost]
        [ActionAuthorize(ActionCode.UserGroups)]
        [BackendActionContext(ActionCode.UserGroups)]
        public ActionResult _Index(
            string tabId,
            int parentId,
            int page,
            int pageSize,
            string orderBy = "")
        {
            var serviceResult = _service.List(new ListCommand
            {
                StartPage = page,
                PageSize = pageSize,
                SortExpression = GridExtensions.ToSqlSortExpression(orderBy)
            });
            return new TelerikResult(serviceResult.Data, serviceResult.TotalRecords);
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.UserGroups)]
        [BackendActionContext(ActionCode.UserGroups)]
        public ActionResult Tree(string tabId, int parentId)
        {
            var result = _service.InitTree(parentId);
            var model = UserGroupTreeViewModel.Create(result, tabId, parentId);
            return JsonHtml("Tree", model);
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.SelectUserGroup)]
        [BackendActionContext(ActionCode.SelectUserGroup)]
        public ActionResult Select(string tabId, int parentId, int id)
        {
            var model = UserGroupSelectableListViewModel.Create(tabId, parentId, new[] { id });
            return JsonHtml("SelectIndex", model);
        }

        [HttpPost]
        [GridAction(EnableCustomBinding = true)]
        [ActionAuthorize(ActionCode.SelectUserGroup)]
        [BackendActionContext(ActionCode.SelectUserGroup)]
        public ActionResult _Select(
            string tabId,
            int parentId,
            int page,
            int pageSize,
            int IDs,
            string orderBy = "")
        {
            var serviceResult = _service.List(new ListCommand
            {
                StartPage = page,
                PageSize = pageSize,
                SortExpression = GridExtensions.ToSqlSortExpression(orderBy)
            }, new[] { IDs });
            return new TelerikResult(serviceResult.Data, serviceResult.TotalRecords);
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.AddNewUserGroup)]
        [BackendActionContext(ActionCode.AddNewUserGroup)]
        public ActionResult New(string tabId, int parentId)
        {
            var group = _service.NewProperties();
            var model = UserGroupViewModel.Create(group, tabId, parentId, _service);
            return JsonHtml("Properties", model);
        }

        [HttpPost, Record]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.AddNewUserGroup)]
        [BackendActionContext(ActionCode.AddNewUserGroup)]
        [BackendActionLog]
        public ActionResult New(string tabId, int parentId, FormCollection collection)
        {
            var group = _service.NewProperties();
            var model = UserGroupViewModel.Create(group, tabId, parentId, _service);

            TryUpdateModel(model);
            model.Validate(ModelState);
            if (ModelState.IsValid)
            {
                model.Data = _service.SaveProperties(model.Data);
                PersistResultId(model.Data.Id);
                return Redirect("Properties", new { tabId, parentId, id = model.Data.Id, successfulActionCode = ActionCode.SaveUserGroup });
            }

            return JsonHtml("Properties", model);
        }

        [HttpPost, Record]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.CreateLikeUserGroup)]
        [BackendActionContext(ActionCode.CreateLikeUserGroup)]
        [BackendActionLog]
        public ActionResult Copy(int id)
        {
            var result = _service.Copy(id);
            PersistResultId(result.Id);
            PersistFromId(id);
            return JsonMessageResult(result.Message);
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.UserGroupProperties)]
        [BackendActionContext(ActionCode.UserGroupProperties)]
        public ActionResult Properties(string tabId, int parentId, int id, string successfulActionCode)
        {
            var group = _service.ReadProperties(id);
            ViewData[SpecialKeys.IsEntityReadOnly] = group.IsReadOnly;
            var model = UserGroupViewModel.Create(group, tabId, parentId, _service);
            model.SuccesfulActionCode = successfulActionCode;
            return JsonHtml("Properties", model);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.UpdateUserGroup)]
        [BackendActionContext(ActionCode.UpdateUserGroup)]
        [BackendActionLog]
        [Record(ActionCode.UserGroupProperties)]
        public ActionResult Properties(string tabId, int parentId, int id, FormCollection collection)
        {
            var group = _service.ReadProperties(id);
            var model = UserGroupViewModel.Create(group, tabId, parentId, _service);
            TryUpdateModel(model);
            model.Validate(ModelState);
            if (ModelState.IsValid)
            {
                model.Data = _service.UpdateProperties(model.Data);
                return Redirect("Properties", new { tabId, parentId, id = model.Data.Id, successfulActionCode = ActionCode.UpdateUserGroup });
            }

            return JsonHtml("Properties", model);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.RemoveUserGroup)]
        [BackendActionContext(ActionCode.RemoveUserGroup)]
        [BackendActionLog]
        [Record]
        public ActionResult Remove(int id)
        {
            var result = _service.Remove(id);
            return JsonMessageResult(result);
        }

        public ActionResult RemovePreAction(int id) => Json(_service.RemovePreAction(id));
    }
}
