using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.BLL.Services.ArticleServices;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Configuration;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Utils;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.Extensions.ModelBinders;
using Quantumart.QP8.WebMvc.Infrastructure.ActionFilters;
using Quantumart.QP8.WebMvc.Infrastructure.ActionResults;
using Quantumart.QP8.WebMvc.Infrastructure.Enums;
using Quantumart.QP8.WebMvc.ViewModels;
using Quantumart.QP8.WebMvc.ViewModels.User;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class UserController : AuthQpController
    {
        private readonly IUserService _service;

        public UserController(IUserService service, IArticleService dbArticleService, QPublishingOptions options)
            : base(dbArticleService, options)
        {
            _service = service;
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.Users)]
        [BackendActionContext(ActionCode.Users)]
        public async Task<ActionResult> Index(string tabId, int parentId)
        {
            var result = _service.InitList(parentId);
            var model = UserListViewModel.Create(result, tabId, parentId);
            return await JsonHtml("Index", model);
        }

        [HttpPost]
        [ActionAuthorize(ActionCode.Users)]
        [BackendActionContext(ActionCode.Users)]
        public ActionResult _Index(
            string tabId,
            int parentId,
            int page,
            int pageSize,
            [Bind(Prefix = "searchQuery")] [ModelBinder(typeof(JsonStringModelBinder<UserListFilter>))] UserListFilter filter,
            string orderBy)
        {
            var listCommand = GetListCommand(page, pageSize, orderBy);
            var serviceResult = _service.List(listCommand, filter);
            return new TelerikResult(serviceResult.Data, serviceResult.TotalRecords);
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        public async Task<ActionResult> SearchBlock(string hostId)
        {
            var model = new UserSearchBlockViewModel(hostId);
            return await JsonHtml("SearchBlock", model);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.MultipleSelectUser)]
        [BackendActionContext(ActionCode.MultipleSelectUser)]
        public async Task<ActionResult> MultipleSelect(string tabId, int parentId, [FromBody] SelectedItemsViewModel selModel)
        {
            var model = UserSelectableListViewModel.Create(tabId, parentId, selModel.Ids, true);
            return await JsonHtml("MultipleSelectIndex", model);
        }

        [HttpPost]
        [ActionAuthorize(ActionCode.MultipleSelectUser)]
        [BackendActionContext(ActionCode.MultipleSelectUser)]
        public ActionResult _MultipleSelect(
            string tabId, [FromForm(Name="IDs")]string ids, int page, int pageSize, string orderBy,
            [Bind(Prefix = "searchQuery")]
            [ModelBinder(typeof(JsonStringModelBinder<UserListFilter>))] UserListFilter filter)
        {
            var selectedIDs = Converter.ToInt32Collection(ids, ',');
            var listCommand = GetListCommand(page, pageSize, orderBy);
            var serviceResult = _service.List(listCommand, filter, selectedIDs);
            return new TelerikResult(serviceResult.Data, serviceResult.TotalRecords);
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.SelectUser)]
        [BackendActionContext(ActionCode.SelectUser)]
        public async Task<ActionResult> Select(string tabId, int parentId, int id)
        {
            var model = UserSelectableListViewModel.Create(tabId, parentId, new[] { id }, false);
            return await JsonHtml("MultipleSelectIndex", model);
        }

        /// <param name="IDs">
        /// Идентификатор выбранного компонента: BackendEntityGrid сериализует один или несколько выбранных Id
        /// в строку через запятую. Т.о. для единственного Id, строковое представление совпадает с числовым.
        /// </param>
        [HttpPost]
        [ActionAuthorize(ActionCode.MultipleSelectUser)]
        [BackendActionContext(ActionCode.MultipleSelectUser)]
        public ActionResult _Select(
            string tabId,
            int page,
            int pageSize,
            [Bind(Prefix = "searchQuery")] [ModelBinder(typeof(JsonStringModelBinder<UserListFilter>))] UserListFilter filter,
            string orderBy,
            int IDs = 0)
        {
            var listCommand = GetListCommand(page, pageSize, orderBy);
            var serviceResult = _service.List(listCommand, filter, new[] { IDs });
            return new TelerikResult(serviceResult.Data, serviceResult.TotalRecords);
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.AddNewUser)]
        [BackendActionContext(ActionCode.AddNewUser)]
        public async Task<ActionResult> New(string tabId, int parentId)
        {
            var user = _service.GetUserToAdd();
            var model = UserViewModel.Create(user, tabId, parentId, _service);
            return await JsonHtml("Properties", model);
        }

        [HttpPost, Record]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.AddNewUser)]
        [BackendActionContext(ActionCode.AddNewUser)]
        [BackendActionLog]
        public async Task<ActionResult> New(string tabId, int parentId, IFormCollection collection)
        {
            var user = _service.GetUserToAdd();
            var model = UserViewModel.Create(user, tabId, parentId, _service);

            await TryUpdateModelAsync(model);

            if (ModelState.IsValid)
            {
                model.Data = _service.SaveProperties(model.Data);
                PersistResultId(model.Data.Id);
                AppendFormGuidsFromIds("ContentDefaultFilter.ArticleIDs", "ContentDefaultFilter.ArticleUniqueIDs");
                return Redirect("Properties", new { tabId, parentId, id = model.Data.Id, successfulActionCode = ActionCode.SaveUser });
            }

            return await JsonHtml("Properties", model);
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.UserProperties)]
        [BackendActionContext(ActionCode.UserProperties)]
        public async Task<ActionResult> Properties(string tabId, int parentId, int id, string successfulActionCode)
        {
            var user = _service.ReadProperties(id);
            var model = UserViewModel.Create(user, tabId, parentId, _service);
            model.SuccesfulActionCode = successfulActionCode;
            return await JsonHtml("Properties", model);
        }

        [HttpPost, Record(ActionCode.UserProperties)]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.UpdateUser)]
        [BackendActionContext(ActionCode.UpdateUser)]
        [BackendActionLog]
        public async Task<ActionResult> Properties(string tabId, int parentId, int id, IFormCollection collection)
        {
            var user = _service.ReadProperties(id);
            var model = UserViewModel.Create(user, tabId, parentId, _service);

            await TryUpdateModelAsync(model);

            if (ModelState.IsValid)
            {
                model.Data = _service.UpdateProperties(model.Data);
                AppendFormGuidsFromIds("ContentDefaultFilter.ArticleIDs", "ContentDefaultFilter.ArticleUniqueIDs");
                return Redirect("Properties", new { tabId, parentId, id = model.Data.Id, successfulActionCode = ActionCode.UpdateUser });
            }

            return await JsonHtml("Properties", model);
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

        public async Task<ActionResult> Profile(string tabId, int parentId, string successfulActionCode)
        {
            var user = _service.ReadProfile(QPContext.CurrentUserId);
            var model = ProfileViewModel.Create(user, tabId, parentId, _service);
            model.SuccesfulActionCode = successfulActionCode;
            return await JsonHtml("Profile", model);
        }

        [HttpPost]
        public async Task<ActionResult> Profile(string tabId, int parentId, IFormCollection collection)
        {
            var user = _service.ReadProfile(QPContext.CurrentUserId);
            var model = ProfileViewModel.Create(user, tabId, parentId, _service);
            await TryUpdateModelAsync(model);
            if (ModelState.IsValid)
            {
                _service.UpdateProfile(model.Data);
                return Redirect("Profile", new { successfulActionCode = ActionCode.UpdateProfile });
            }

            return await JsonHtml("Profile", model);
        }

        [HttpGet]
        public async Task<ActionResult> ChangePassword()
        {
            string tabId = "0"; int parentId = 0;
            var user = _service.ReadProfile(QPContext.CurrentUserId);
            var model = ProfileViewModel.Create(user, tabId, parentId, _service);
            return await JsonHtml("ChangePasswordPopup", model);
        }

        [HttpPost]
        public async Task<ActionResult> ChangePassword(string tabId, [FromBody] User currentUser)
        {
            var parentId = 0;
            var user = _service.ReadProfile(QPContext.CurrentUserId);
            user.NewPassword = currentUser.NewPassword;
            user.NewPasswordCopy = currentUser.NewPasswordCopy;
            var model = ProfileViewModel.Create(user, tabId, parentId, _service);

            ModelState.Clear();

            await TryUpdateModelAsync(model);

            if (ModelState.IsValid)
            {
                model.Data.MustChangePassword = false;
                var result = _service.UpdateProfile(model.Data);
                model.SuccesfulActionCode = ActionCode.UpdateProfile;
                return Json(new { success = true, isChanging = true });

            }
            return await JsonHtml("ChangePasswordPopup", model);
        }
    }
}
