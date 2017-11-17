using System.Diagnostics.CodeAnalysis;
using System.Web.Mvc;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.BLL.Services.ArticleServices;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Utils;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.Extensions.Helpers;
using Quantumart.QP8.WebMvc.Extensions.ModelBinders;
using Quantumart.QP8.WebMvc.Infrastructure.ActionFilters;
using Quantumart.QP8.WebMvc.Infrastructure.Enums;
using Quantumart.QP8.WebMvc.ViewModels;
using Quantumart.QP8.WebMvc.ViewModels.User;
using Telerik.Web.Mvc;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class UserController : QPController
    {
        private readonly IUserService _service;

        public UserController(IUserService service, IArticleService dbArticleService)
            : base(dbArticleService)
        {
            _service = service;
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.Users)]
        [BackendActionContext(ActionCode.Users)]
        public ActionResult Index(string tabId, int parentId)
        {
            var result = _service.InitList(parentId);
            var model = UserListViewModel.Create(result, tabId, parentId);
            return JsonHtml("Index", model);
        }

        [HttpPost]
        [GridAction(EnableCustomBinding = true)]
        [ActionAuthorize(ActionCode.Users)]
        [BackendActionContext(ActionCode.Users)]
        public ActionResult _Index(
            string tabId,
            int parentId,
            GridCommand command,
            [Bind(Prefix = "searchQuery")] [ModelBinder(typeof(JsonStringModelBinder<UserListFilter>))] UserListFilter filter)
        {
            var serviceResult = _service.List(command.GetListCommand(), filter);
            return View(new GridModel { Data = serviceResult.Data, Total = serviceResult.TotalRecords });
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        public ActionResult SearchBlock(string hostId)
        {
            var model = new UserSearchBlockViewModel(hostId);
            return JsonHtml("SearchBlock", model);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.MultipleSelectUser)]
        [BackendActionContext(ActionCode.MultipleSelectUser)]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public ActionResult MultipleSelect(string tabId, int parentId, int[] IDs)
        {
            var model = UserSelectableListViewModel.Create(tabId, parentId, IDs, true);
            return JsonHtml("MultipleSelectIndex", model);
        }

        [HttpPost]
        [GridAction(EnableCustomBinding = true)]
        [ActionAuthorize(ActionCode.MultipleSelectUser)]
        [BackendActionContext(ActionCode.MultipleSelectUser)]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public ActionResult _MultipleSelect(
            string tabId,
            string IDs,
            GridCommand command,
            [Bind(Prefix = "searchQuery")] [ModelBinder(typeof(JsonStringModelBinder<UserListFilter>))] UserListFilter filter)
        {
            var selectedIDs = Converter.ToInt32Collection(IDs, ',');
            var serviceResult = _service.List(command.GetListCommand(), filter, selectedIDs);
            return View(new GridModel { Data = serviceResult.Data, Total = serviceResult.TotalRecords });
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.SelectUser)]
        [BackendActionContext(ActionCode.SelectUser)]
        public ActionResult Select(string tabId, int parentId, int id)
        {
            var model = UserSelectableListViewModel.Create(tabId, parentId, new[] { id }, false);
            return JsonHtml("MultipleSelectIndex", model);
        }

        [HttpPost]
        [GridAction(EnableCustomBinding = true)]
        [ActionAuthorize(ActionCode.MultipleSelectUser)]
        [BackendActionContext(ActionCode.MultipleSelectUser)]
        public ActionResult _Select(
            string tabId,
            int id,
            GridCommand command,
            [Bind(Prefix = "searchQuery")] [ModelBinder(typeof(JsonStringModelBinder<UserListFilter>))] UserListFilter filter)
        {
            var serviceResult = _service.List(command.GetListCommand(), filter, new[] { id });
            return View(new GridModel { Data = serviceResult.Data, Total = serviceResult.TotalRecords });
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.AddNewUser)]
        [BackendActionContext(ActionCode.AddNewUser)]
        public ActionResult New(string tabId, int parentId)
        {
            var user = _service.GetUserToAdd();
            var model = UserViewModel.Create(user, tabId, parentId, _service);
            return JsonHtml("Properties", model);
        }

        [HttpPost, Record]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.AddNewUser)]
        [BackendActionContext(ActionCode.AddNewUser)]
        [BackendActionLog]
        public ActionResult New(string tabId, int parentId, FormCollection collection)
        {
            var user = _service.GetUserToAdd();
            var model = UserViewModel.Create(user, tabId, parentId, _service);

            TryUpdateModel(model);
            model.Validate(ModelState);
            if (ModelState.IsValid)
            {
                model.Data = _service.SaveProperties(model.Data);
                PersistResultId(model.Data.Id);
                AppendFormGuidsFromIds("ContentDefaultFilter.ArticleIDs", "ContentDefaultFilter.ArticleUniqueIDs");
                return Redirect("Properties", new { tabId, parentId, id = model.Data.Id, successfulActionCode = ActionCode.SaveUser });
            }

            return JsonHtml("Properties", model);
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.UserProperties)]
        [BackendActionContext(ActionCode.UserProperties)]
        public ActionResult Properties(string tabId, int parentId, int id, string successfulActionCode)
        {
            var user = _service.ReadProperties(id);
            var model = UserViewModel.Create(user, tabId, parentId, _service);
            model.SuccesfulActionCode = successfulActionCode;
            return JsonHtml("Properties", model);
        }

        [HttpPost, Record(ActionCode.UserProperties)]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.UpdateUser)]
        [BackendActionContext(ActionCode.UpdateUser)]
        [BackendActionLog]
        public ActionResult Properties(string tabId, int parentId, int id, FormCollection collection)
        {
            var user = _service.ReadProperties(id);
            var model = UserViewModel.Create(user, tabId, parentId, _service);

            TryUpdateModel(model);
            model.Validate(ModelState);
            if (ModelState.IsValid)
            {
                model.Data = _service.UpdateProperties(model.Data);
                AppendFormGuidsFromIds("ContentDefaultFilter.ArticleIDs", "ContentDefaultFilter.ArticleUniqueIDs");
                return Redirect("Properties", new { tabId, parentId, id = model.Data.Id, successfulActionCode = ActionCode.UpdateUser });
            }

            return JsonHtml("Properties", model);
        }

        [HttpPost, Record]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.RemoveUser)]
        [BackendActionContext(ActionCode.RemoveUser)]
        [BackendActionLog]
        public ActionResult Remove(int id)
        {
            var result = _service.Remove(id);
            return JsonMessageResult(result);
        }

        [HttpPost, Record]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.CreateLikeUser)]
        [BackendActionContext(ActionCode.CreateLikeUser)]
        [BackendActionLog]
        public ActionResult Copy(int id)
        {
            var result = _service.Copy(id);
            PersistResultId(result.Id);
            PersistFromId(id);
            return JsonMessageResult(result.Message);
        }

#pragma warning disable CS0108 // Member hides inherited member; missing new keyword

        // TODO: RENAME
        public ActionResult Profile(string tabId, int parentId, string successfulActionCode)
        {
            var user = _service.ReadProfile(QPContext.CurrentUserId);
            var model = ProfileViewModel.Create(user, tabId, parentId, _service);
            model.SuccesfulActionCode = successfulActionCode;
            return JsonHtml("Profile", model);
        }

        [HttpPost]
        public ActionResult Profile(string tabId, int parentId, FormCollection collection)
        {
            var user = _service.ReadProfile(QPContext.CurrentUserId);
            var model = ProfileViewModel.Create(user, tabId, parentId, _service);
            TryUpdateModel(model);
            model.Validate(ModelState);
            if (ModelState.IsValid)
            {
                _service.UpdateProfile(model.Data);
                return Redirect("Profile", new { successfulActionCode = ActionCode.UpdateProfile });
            }

            return JsonHtml("Profile", model);
        }
    }
}
