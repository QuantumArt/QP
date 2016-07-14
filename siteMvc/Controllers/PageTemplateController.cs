using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Extensions.ActionFilters;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.Extensions.Helpers;
using Quantumart.QP8.WebMvc.ViewModels.PageTemplate;
using System.Web.Mvc;
using Telerik.Web.Mvc;
using System.Linq;


namespace Quantumart.QP8.WebMvc.Controllers
{
    [ValidateInput(false)]
    public class PageTemplateController : QPController
    {
        IPageTemplateService _pageTemplateService;

        public PageTemplateController(IPageTemplateService pageTemplateService)
		{
            this._pageTemplateService = pageTemplateService;
		}

        #region list actions
        [HttpGet]
        [ExceptionResult(ExceptionResultMode.UIAction)]
        [ActionAuthorize(ActionCode.Templates)]
		[BackendActionContext(ActionCode.Templates)]
        public ActionResult IndexTemplates(string tabId, int parentId)
        {
            PageTemplateInitListResult result = _pageTemplateService.InitTemplateList(parentId);
            PageTemplateListViewModel model = PageTemplateListViewModel.Create(result, tabId, parentId);
            return this.JsonHtml("Index", model);
        }

        [HttpPost]
        [GridAction(EnableCustomBinding = true)]
		[ActionAuthorize(ActionCode.Templates)]
		[BackendActionContext(ActionCode.Templates)]
        public ActionResult _IndexTemplates(string tabId, int parentId, GridCommand command)
        {
            ListResult<PageTemplateListItem> serviceResult = _pageTemplateService.GetPageTemplatesBySiteId(command.GetListCommand(), parentId);
            return View(new GridModel() { Data = serviceResult.Data, Total = serviceResult.TotalRecords });
        }               		

		#endregion

		#region create actions
		[HttpGet]
        [ExceptionResult(ExceptionResultMode.UIAction)]
        [ActionAuthorize(ActionCode.AddNewPageTemplate)]
        [EntityAuthorize(ActionTypeCode.Update, EntityTypeCode.PageTemplate, "parentId")]
        [BackendActionContext(ActionCode.AddNewPageTemplate)]
        public ActionResult NewPageTemplate(string tabId, int parentId)
        {
            PageTemplate template = _pageTemplateService.NewPageTemplateProperties(parentId);
            PageTemplateViewModel model = PageTemplateViewModel.Create(template, tabId, parentId, _pageTemplateService);
            return this.JsonHtml("Properties", model);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.UIAction)]
        [ConnectionScope(ConnectionScopeMode.TransactionOn)]
        [ActionAuthorize(ActionCode.AddNewPageTemplate)]
        [BackendActionContext(ActionCode.AddNewPageTemplate)]
        [BackendActionLog]
		[Record]
        public ActionResult NewPageTemplate(string tabId, int parentId, FormCollection collection)
        {
            PageTemplate template = _pageTemplateService.NewPageTemplatePropertiesForUpdate(parentId);
            PageTemplateViewModel model = PageTemplateViewModel.Create(template, tabId, parentId, _pageTemplateService);
            TryUpdateModel(model);
            model.Validate(ModelState);
            if (ModelState.IsValid)
            {
                model.Data = _pageTemplateService.SavePageTemplateProperties(model.Data);
				this.PersistResultId(model.Data.Id);
                return Redirect("PageTemplateProperties", new { tabId = tabId, parentId = parentId, id = model.Data.Id, successfulActionCode = Constants.ActionCode.SavePageTemplate });
            }
            else
                return this.JsonHtml("Properties", model);
        }
		
        #endregion
				
        #region properties region
        [HttpGet]
        [ExceptionResult(ExceptionResultMode.UIAction)]
        [ActionAuthorize(ActionCode.PageTemplateProperties)]
        [EntityAuthorize(ActionTypeCode.Read, EntityTypeCode.PageTemplate, "id")]
        [BackendActionContext(ActionCode.PageTemplateProperties)]
        public ActionResult PageTemplateProperties(string tabId, int parentId, int id, string successfulActionCode)
        {
            PageTemplate template = _pageTemplateService.ReadPageTemplateProperties(id);
            ViewData[SpecialKeys.IsEntityReadOnly] = template.LockedByAnyoneElse;
            PageTemplateViewModel model = PageTemplateViewModel.Create(template, tabId, parentId, _pageTemplateService);
            model.SuccesfulActionCode = successfulActionCode;
            return this.JsonHtml("Properties", model);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.UIAction)]
        [ConnectionScope(ConnectionScopeMode.TransactionOn)]
        [ActionAuthorize(ActionCode.UpdatePageTemplate)]
        [BackendActionContext(ActionCode.UpdatePageTemplate)]
        [BackendActionLog]
		[Record(ActionCode.PageTemplateProperties)]
        public ActionResult PageTemplateProperties(string tabId, int parentId, int id, FormCollection collection)
        {
            PageTemplate template = _pageTemplateService.ReadPageTemplatePropertiesForUpdate(id);
            PageTemplateViewModel model = PageTemplateViewModel.Create(template, tabId, parentId, _pageTemplateService);
            TryUpdateModel(model);
            model.Validate(ModelState);
            if (ModelState.IsValid)
            {				
                model.Data = _pageTemplateService.UpdatePageTemplateProperties(model.Data);				
                return Redirect("PageTemplateProperties", new { tabId = tabId, parentId = parentId, id = model.Data.Id, successfulActionCode = Constants.ActionCode.UpdatePageTemplate });
            }
            else
                return this.JsonHtml("Properties", model);
        }       				

		

		#endregion

		#region remove region
		[HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope(ConnectionScopeMode.TransactionOn)]
        [ActionAuthorize(ActionCode.RemovePageTemplate)]
        [BackendActionContext(ActionCode.RemovePageTemplate)]
        [BackendActionLog]
		[Record]
        public ActionResult RemovePageTemplate(int id)
        {
            MessageResult result = _pageTemplateService.RemovePageTemplate(id);
            return this.JsonMessageResult(result);
        }				

		
        #endregion

		#region cancel actions
		[HttpPost]
		[ExceptionResult(ExceptionResultMode.OperationAction)]
		[ConnectionScope(ConnectionScopeMode.TransactionOn)]
		[ActionAuthorize(ActionCode.CancelTemplate)]
		[BackendActionContext(ActionCode.CancelTemplate)]
		public ActionResult CancelTemplate(int id)
		{
			_pageTemplateService.CancelTemplate(id);
			return Json(null);
		}
		
		#endregion				
		
		[HttpPost]
		public JsonResult GetFieldsByContentId(int contentId)
		{
			var content = _pageTemplateService.GetContentById(contentId);
			bool hasWorkflow;
			var statuses = _pageTemplateService.GetStatusIdsByContentId(contentId, out hasWorkflow);
			return new JsonResult
			{
				Data = new
				{
					success = true,
					fields = string.Join(",", ServiceField.CreateAll().Select(f => f.ColumnName).Concat(content.Fields.Select(x => x.Name))),
					statuses = statuses,
					hasWorkflow = hasWorkflow
				},
				JsonRequestBehavior = JsonRequestBehavior.DenyGet
			};
		}

		#region parentAssembling

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.OperationAction)]
		[NoTransactionConnectionScopeAttribute]
		[ActionAuthorize(ActionCode.AssemblePageFromPageObject)]
		[BackendActionContext(ActionCode.AssemblePageFromPageObject)]
		[BackendActionLog]
		public ActionResult AssemblePageFromPageObject(string tabId, int parentId, int id)
		{
			return Json(_pageTemplateService.AssemblePageFromPageObject(parentId));
		}

		[HttpPost]
		public ActionResult AssemblePageFromPageObjectPreAction(string tabId, int parentId, int id)
		{
			return Json(_pageTemplateService.AssemblePageFromPageObjectPreAction(parentId));
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.OperationAction)]
		[NoTransactionConnectionScopeAttribute]
		[ActionAuthorize(ActionCode.AssemblePageFromPageObjectFormat)]
		[BackendActionContext(ActionCode.AssemblePageFromPageObjectFormat)]
		[BackendActionLog]
		public ActionResult AssemblePageFromPageObjectFormat(string tabId, int parentId, int id)
		{
			return Json(_pageTemplateService.AssemblePageFromPageObjectFormat(parentId));
		}

		[HttpPost]
		public ActionResult AssemblePageFromPageObjectFormatPreAction(string tabId, int parentId, int id)
		{
			return Json(_pageTemplateService.AssemblePageFromPageObjectFormatPreAction(parentId));
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.OperationAction)]
		[NoTransactionConnectionScopeAttribute]
		[ActionAuthorize(ActionCode.AssemblePageFromPageObjectList)]
		[BackendActionContext(ActionCode.AssemblePageFromPageObjectList)]
		[BackendActionLog]
		public ActionResult AssemblePageFromPageObjectList(string tabId, int parentId, int id)
		{
			return Json(_pageTemplateService.AssemblePageFromPageObjectList(parentId));
		}

		[HttpPost]
		public ActionResult AssemblePageFromPageObjectListPreAction(string tabId, int parentId, int id)
		{
			return Json(_pageTemplateService.AssemblePageFromPageObjectListPreAction(parentId));
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.OperationAction)]
		[NoTransactionConnectionScopeAttribute]
		[ActionAuthorize(ActionCode.AssembleObjectFromPageObjectFormat)]
		[BackendActionContext(ActionCode.AssembleObjectFromPageObjectFormat)]
		[BackendActionLog]
		public ActionResult AssembleObjectFromPageObjectFormat(string tabId, int parentId, int id)
		{
			return Json(_pageTemplateService.AssembleObjectFromPageObjectFormat(parentId));
		}

		[HttpPost]
		public ActionResult AssembleObjectFromPageObjectFormatPreAction(string tabId, int parentId, int id)
		{
			return Json(_pageTemplateService.AssembleObjectFromPageObjectFormatPreAction(parentId));
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.OperationAction)]
		[NoTransactionConnectionScopeAttribute]
		[ActionAuthorize(ActionCode.AssembleObjectFromPageObjectFormat)]
		[BackendActionContext(ActionCode.AssembleObjectFromPageObjectFormat)]
		[BackendActionLog]
		public ActionResult AssembleObjectFromTemplateObjectFormat(string tabId, int parentId, int id)
		{
			return Json(_pageTemplateService.AssembleObjectFromTemplateObjectFormat(parentId));
		}

		[HttpPost]
		public ActionResult AssembleObjectFromTemplateObjectFormatPreAction(string tabId, int parentId, int id)
		{
			return Json(_pageTemplateService.AssembleObjectFromTemplateObjectFormatPreAction(parentId));
		}

		[HttpGet]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ActionAuthorize(ActionCode.SearchInCode)]
		[BackendActionContext(ActionCode.SearchInCode)]
		public ActionResult Formats(string tabId, int parentId, int id)
		{
			SearchInFormatsViewModel model = SearchInFormatsViewModel.Create(tabId, parentId, id, _pageTemplateService);
			return this.JsonHtml("Formats", model);
		}

		[ActionAuthorize(ActionCode.SearchInCode)]
		[BackendActionContext(ActionCode.SearchInCode)]
		[GridAction(EnableCustomBinding = true)]		
		public ActionResult _Formats(string tabId, int parentId, int id, int? templateId, int? pageId, string filterVal, GridCommand command)
		{
			ListResult<ObjectFormatSearchResultListItem> list = _pageTemplateService.FormatSearch(command.GetListCommand(), id, templateId, pageId, filterVal);
			return View(new GridModel()
			{
				Data = list.Data,
				Total = list.TotalRecords
			});
		}

		[HttpGet]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ActionAuthorize(ActionCode.SearchInTemplates)]
		[BackendActionContext(ActionCode.SearchInTemplates)]
		public ActionResult Templates(string tabId, int parentId)
		{
			SearchInTemplatesViewModel model = SearchInTemplatesViewModel.Create(tabId, parentId);
			return this.JsonHtml("Templates", model);
		}

		[ActionAuthorize(ActionCode.SearchInTemplates)]
		[BackendActionContext(ActionCode.SearchInTemplates)]
		[GridAction(EnableCustomBinding = true)]
		public ActionResult _Templates(string tabId, int parentId, int id, string filterVal, GridCommand command)
		{
			ListResult<PageTemplateSearchListItem> list = _pageTemplateService.TemplateSearch(command.GetListCommand(), id, filterVal);
			return View(new GridModel()
			{
				Data = list.Data,
				Total = list.TotalRecords
			});
		}

		[HttpGet]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ActionAuthorize(ActionCode.SearchInObjects)]
		[BackendActionContext(ActionCode.SearchInObjects)]
		public ActionResult Parameters(string tabId, int parentId, int id)
		{
			SearchInObjectsViewModel model = SearchInObjectsViewModel.Create(tabId, parentId, id, _pageTemplateService);
			return this.JsonHtml("Objects", model);
		}

		[ActionAuthorize(ActionCode.SearchInObjects)]
		[BackendActionContext(ActionCode.SearchInObjects)]
		[GridAction(EnableCustomBinding = true)]
		public ActionResult _Parameters(string tabId, int parentId, int id, int? templateId, int? pageId, string filterVal, GridCommand command)
		{
			ListResult<ObjectSearchListItem> list = _pageTemplateService.ObjectSearch(command.GetListCommand(), id, templateId, pageId, filterVal);
			return View(new GridModel()
			{
				Data = list.Data,
				Total = list.TotalRecords
			});
		}
		#endregion

		
		[HttpPost]
		[ExceptionResult(ExceptionResultMode.OperationAction)]
		[ConnectionScope(ConnectionScopeMode.TransactionOn)]
		[ActionAuthorize(ActionCode.CaptureLockTemplate)]
		[BackendActionContext(ActionCode.CaptureLockTemplate)]
		[BackendActionLog]
		public ActionResult CaptureLockTemplate(int id)
		{			
			_pageTemplateService.CaptureLockTemplate(id);
			return Json(null);
		}
		

		#region Higlighted toolbar
		
		[HttpPost]
		public JsonResult GetDefaultCode(int formatId)
		{
			string defaultCode = _pageTemplateService.ReadDefaultCode(formatId);						
			return new JsonResult
			{
				Data = new
				{
					success = true,
					code = defaultCode					
				},
				JsonRequestBehavior = JsonRequestBehavior.AllowGet
			};
		}

		[HttpPost]
		public JsonResult GetDefaultPresentation(int formatId)
		{
			string defaultCode = _pageTemplateService.ReadDefaultPresentation(formatId);
			return new JsonResult
			{
				Data = new
				{
					success = true,
					code = defaultCode
				},
				JsonRequestBehavior = JsonRequestBehavior.DenyGet
			};
		}

		[HttpPost]
		public JsonResult GetInsertPopUpMarkUp(int templateId, int? formatId, bool presentationOrCodeBehind)
		{
			int? languageId;
			string assemblingType;
			bool isContainer;
			bool isForm;
			int? pageId = null;
			int? contentId = null;			

			if (formatId.HasValue)
			{
				var format = _pageTemplateService.ReadFormatProperties(formatId.Value, true, false);
				languageId = format.NetLanguageId;
				assemblingType = _pageTemplateService.ReadPageTemplateProperties(templateId).Site.AssemblingType;
				var obj = _pageTemplateService.ReadObjectProperties(format.ObjectId, false);
				isContainer = obj.IsObjectContainerType;
				pageId = obj.PageId;
				isForm = obj.IsObjectFormType;
				if (isForm)
					contentId = obj.ContentForm.ContentId;
				else if (isContainer)
					contentId = obj.Container.ContentId;
			}

			else
			{
				var template = _pageTemplateService.ReadPageTemplateProperties(templateId);
				languageId = template.NetLanguageId;
				assemblingType = template.Site.AssemblingType;
				isContainer = false;
				isForm = false;
				contentId = null;
			}			

			return new JsonResult
			{
				Data = new{
				html = this.RenderPartialView("InsertPopupWindow", new InsertPopupViewModel(templateId,
				languageId, assemblingType, presentationOrCodeBehind, isContainer, isForm, contentId, pageId, _pageTemplateService))
				},
				JsonRequestBehavior = JsonRequestBehavior.DenyGet
			};
		}

		[HttpPost]
		public JsonResult GetHTAToolbarMarkUp(bool presentationOrCodeBehind, int? formatId, int? templateId)
		{
			return new JsonResult
			{
				Data = new
				{
					html = this.RenderPartialView("HTAreaToolbar", new HTAreaToolbarViewModel(presentationOrCodeBehind, formatId, templateId))
				},
				JsonRequestBehavior = JsonRequestBehavior.DenyGet
			};
		}

		#endregion
	}
}