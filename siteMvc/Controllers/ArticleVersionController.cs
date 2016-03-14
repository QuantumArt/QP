using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Data;
using System.Web;
using System.Web.Mvc;
using AutoMapper;
using Telerik.Web.Mvc;
using Telerik.Web.Mvc.UI;
using Quantumart.QP8;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.BLL.Exceptions;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Utils;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.Extensions.ActionResults;
using Quantumart.QP8.WebMvc.Extensions.Helpers;
using Quantumart.QP8.WebMvc.ViewModels;
using Quantumart.QP8.WebMvc.Extensions.ActionFilters;

namespace Quantumart.QP8.WebMvc.Controllers
{
	[ValidateInput(false)]    
	public class ArticleVersionController : QPController
	{
		#region list actions

		[HttpGet]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ActionAuthorize(ActionCode.ArticleVersions)]
		[EntityAuthorize(ActionTypeCode.Read, EntityTypeCode.Article, "parentId")]
		[BackendActionContext(ActionCode.ArticleVersions)]
		public ActionResult Index(string tabId, int parentId)
		{
			ArticleVersionListViewModel model = ArticleVersionListViewModel.Create(tabId, parentId);
			return this.JsonHtml("Index", model);
		}

		[HttpPost]
		[GridAction(EnableCustomBinding = true)]
		[ActionAuthorize(ActionCode.ArticleVersions)]
		[BackendActionContext(ActionCode.ArticleVersions)]
		public ActionResult _Index(string tabId, int parentId, GridCommand command)
		{
			List<ArticleVersion> serviceResult = ArticleVersionService.List(parentId, command.GetListCommand());
			List<ArticleVersionListItem> result = Mapper.Map<List<ArticleVersion>, List<ArticleVersionListItem>>(serviceResult);
			return View(new GridModel() { Data = result, Total = result.Count });
		}

		#endregion

		#region forms actions

		[HttpGet]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ActionAuthorize(ActionCode.PreviewArticleVersion)]
		[EntityAuthorize(ActionTypeCode.Read, EntityTypeCode.Article, "parentId")]
		[BackendActionContext(ActionCode.PreviewArticleVersion)]
		public ActionResult Properties(string tabId, int parentId, int id, string successfulActionCode, bool? boundToExternal)
		{
			ArticleVersion version = ArticleVersionService.Read(id, parentId);
			ArticleVersionViewModel model = ArticleVersionViewModel.Create(version, tabId, parentId, successfulActionCode, boundToExternal);
			return this.JsonHtml("Properties", model);
		}

		[HttpGet]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ActionAuthorize(ActionCode.CompareArticleVersionWithCurrent)]
		[EntityAuthorize(ActionTypeCode.Read, EntityTypeCode.Article, "parentId")]
		[BackendActionContext(ActionCode.CompareArticleVersionWithCurrent)]
		public ActionResult CompareWithCurrent(string tabId, int parentId, int id, bool? boundToExternal)
		{
			ArticleVersion version = ArticleVersionService.GetMergedVersion(new int[] { id, ArticleVersion.CurrentVersionId }, parentId);
			ArticleVersionViewModel model = ArticleVersionViewModel.Create(version, tabId, parentId, boundToExternal);
			model.ViewType = ArticleVersionViewType.CompareWithCurrent;
			return this.JsonHtml("Properties", model);
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ActionAuthorize(ActionCode.CompareArticleVersions)]
		[EntityAuthorize(ActionTypeCode.Read, EntityTypeCode.Article, "parentId")]
		[BackendActionContext(ActionCode.CompareArticleVersions)]
		public ActionResult Compare(string tabId, int parentId, int[] IDs, bool? boundToExternal)
		{
			ArticleVersion version = ArticleVersionService.GetMergedVersion(IDs, parentId);
			ArticleVersionViewModel model = ArticleVersionViewModel.Create(version, tabId, parentId, boundToExternal);
			model.ViewType = ArticleVersionViewType.CompareVersions;
			return this.JsonHtml("Properties", model);
		}

		#endregion

		#region non-interface actions

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ConnectionScope(ConnectionScopeMode.TransactionOn)]
		[ActionAuthorize(ActionCode.RestoreArticleVersion)]
		[BackendActionContext(ActionCode.RestoreArticleVersion)]
		[BackendActionLog]
		[Record(ActionCode.PreviewArticleVersion)]
		public ActionResult Properties(string tabId, int parentId, int id, string backendActionCode, bool? boundToExternal, FormCollection collection)
		{
			ArticleVersion version = ArticleVersionService.Read(id);
			ArticleVersionViewModel model = ArticleVersionViewModel.Create(version, tabId, parentId, boundToExternal);
			TryUpdateModel(model);
			model.Validate(ModelState);

			if (ModelState.IsValid)
			{
				model.Data.Article = ArticleVersionService.Restore(model.Data, boundToExternal, this.IsReplayAction());
				return Redirect("Properties", new 
				{ 
					tabId = tabId, 
					parentId = parentId, 
					id = id, 
					successfulActionCode = backendActionCode,
					boundToExternal = boundToExternal
				});
			}
			else
				return (ActionResult)this.JsonHtml("Properties", model);
		}


		[HttpPost]
		[ExceptionResult(ExceptionResultMode.OperationAction)]
		[ConnectionScope(ConnectionScopeMode.TransactionOn)]
		[ActionAuthorize(ActionCode.RemoveArticleVersion)]
		[BackendActionContext(ActionCode.RemoveArticleVersion)]
		[BackendActionLog]
		[Record]
		public ActionResult Remove(int id, bool? boundToExternal)
		{
			MessageResult result = ArticleVersionService.Remove(id, boundToExternal);
			return this.JsonMessageResult(result);
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.OperationAction)]
		[ConnectionScope(ConnectionScopeMode.TransactionOn)]
		[ActionAuthorize(ActionCode.MultipleRemoveArticleVersion)]
		[BackendActionContext(ActionCode.MultipleRemoveArticleVersion)]
		[BackendActionLog]
		[Record]
		public ActionResult MultipleRemove(int[] IDs, bool? boundToExternal)
		{
			MessageResult result = ArticleVersionService.MultipleRemove(IDs, boundToExternal);
			return this.JsonMessageResult(result);
		}

		#endregion


    }
}