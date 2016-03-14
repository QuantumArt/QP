using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Extensions.ActionFilters;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.Extensions.Helpers;
using Quantumart.QP8.WebMvc.ViewModels;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.WebMvc.Hubs;
using System.Web.Script.Serialization;

namespace Quantumart.QP8.WebMvc.Controllers
{
	public class DbController : QPController
	{
		private readonly ICommunicationService _communicationService;
		public DbController(ICommunicationService communicationService)
		{
			_communicationService = communicationService;
		}

		[HttpGet]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ActionAuthorize(ActionCode.DbSettings)]
		[BackendActionContext(ActionCode.DbSettings)]
		public ActionResult Settings(string tabId, int parentId, string successfulActionCode)
		{
			Db db = DbService.ReadSettings();
			DbViewModel model = EntityViewModel.Create<DbViewModel>(db, tabId, parentId);
			model.SuccesfulActionCode = successfulActionCode;
			return this.JsonHtml("Settings", model);
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ActionAuthorize(ActionCode.UpdateDbSettings)]
		[BackendActionContext(ActionCode.UpdateDbSettings)]
		[BackendActionLog]
		[ValidateInput(false)]
		public ActionResult Settings(string tabId, int parentId, FormCollection collection)
		{
			Db db = DbService.ReadSettingsForUpdate();
			DbViewModel model = EntityViewModel.Create<DbViewModel>(db, tabId, parentId);
			TryUpdateModel(model);
			model.Validate(ModelState);
			if (ModelState.IsValid)
			{
				object message = null;
				bool needSendMessage = false;

				if (model.Data.RecordActions)
				{
					new RecordReplayHelper().Clear(HttpContext, model.OverrideRecordsFile);

					if (model.OverrideRecordsUser || model.Data.SingleUserId == null)
					{
						model.Data.SingleUserId = QPContext.CurrentUserId;
						needSendMessage = true;
						message = new
						{
							userId = QPContext.CurrentUserId,
							userName = QPContext.CurrentUserName
						};

					}
				}
				else
				{
					needSendMessage = true;
					model.Data.SingleUserId = null;
				}

				model.Data = DbService.UpdateSettings(model.Data);

				if (needSendMessage)
				{
					_communicationService.Send("singleusermode", message);
				}

				return Redirect("Settings", new { successfulActionCode = Constants.ActionCode.UpdateDbSettings });
			}
			else
			{
				return JsonHtml("Settings", model);
			}
		}
	}
}
