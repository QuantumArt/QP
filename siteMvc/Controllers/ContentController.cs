using System.Linq;
using System.Web.Mvc;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Extensions.ActionFilters;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.Extensions.Helpers;
using Quantumart.QP8.WebMvc.Extensions.ModelBinders;
using Quantumart.QP8.WebMvc.ViewModels;
using Telerik.Web.Mvc;
using System;
using Quantumart.QP8.Resources;
using Quantumart.QP8.WebMvc.ViewModels.Content;
using Quantumart.QP8.BLL.Exceptions;
using Quantumart.QP8.Utils;
using Quantumart.QP8.WebMvc.ViewModels.PageTemplate;
using Quantumart.QP8.WebMvc.ViewModels.CustomAction;
using Quantumart.QP8.WebMvc.ViewModels.Field;
using Quantumart.QP8.WebMvc.ViewModels.Workflow;
using Quantumart.QP8.WebMvc.ViewModels.VirtualContent;

namespace Quantumart.QP8.WebMvc.Controllers
{
	public class ContentController : QPController
	{

		#region list actions

		#region Contents
		[HttpGet]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ActionAuthorize(ActionCode.Contents)]
		[BackendActionContext(ActionCode.Contents)]
		public ActionResult Index(string tabId, int parentId)
		{
			ContentInitListResult result = ContentService.InitList(parentId);
			ContentListViewModel model = ContentListViewModel.Create(result, tabId, parentId);
			return this.JsonHtml("Index", model);
		}

		[HttpPost]
		[GridAction(EnableCustomBinding = true)]
		[ActionAuthorize(ActionCode.Contents)]
		[BackendActionContext(ActionCode.Contents)]
		public ActionResult _Index(string tabId, int parentId, GridCommand command, 
			[Bind(Prefix = "searchQuery")] [ModelBinder(typeof(JsonStringModelBinder<ContentListFilter>))] ContentListFilter filter)
		{
			filter = (filter ?? ContentListFilter.Empty);			
			filter.SiteId = parentId > 0 ? (int?)parentId : null;
			ListResult<ContentListItem> serviceResult = ContentService.List(filter, command.GetListCommand());
			return View(new GridModel() { Data = serviceResult.Data, Total = serviceResult.TotalRecords });
		}
		#endregion

		#region Virtual Content		
		[HttpGet]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ActionAuthorize(ActionCode.VirtualContents)]
		[BackendActionContext(ActionCode.VirtualContents)]
		public ActionResult VirtualIndex(string tabId, int parentId)
		{
			ContentInitListResult result = ContentService.InitList(parentId, true);
			ContentListViewModel model = ContentListViewModel.Create(result, tabId, parentId);			
			return this.JsonHtml("Index", model);
		}

		[HttpPost]
		[GridAction(EnableCustomBinding = true)]
		[ActionAuthorize(ActionCode.VirtualContents)]
		[BackendActionContext(ActionCode.VirtualContents)]
		public ActionResult _VirtualIndex(string tabId, int parentId, GridCommand command)
		{
            ContentListFilter normFilter = ContentListFilter.Empty;
            normFilter.SiteId = parentId;
            ListResult<ContentListItem> serviceResult = VirtualContentService.List(normFilter, command.GetListCommand());
			return View(new GridModel() { Data = serviceResult.Data, Total = serviceResult.TotalRecords });
		}
		#endregion

		#endregion

		#region form actions		
		[HttpGet]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ConnectionScope(ConnectionScopeMode.TransactionOn)]
		[ActionAuthorize(ActionCode.AddNewContent)]
		[EntityAuthorize(ActionTypeCode.Update, EntityTypeCode.Site, "parentId")]
		[BackendActionContext(ActionCode.AddNewContent)]		
		public ActionResult New(string tabId, int parentId, int? groupId)
		{
			Content content = ContentService.New(parentId, groupId);
			ContentViewModel model = ContentViewModel.Create(content, tabId, parentId);
			return this.JsonHtml("Properties", model);
		}


		[HttpPost]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ConnectionScope(ConnectionScopeMode.TransactionOn)]
		[ActionAuthorize(ActionCode.AddNewContent)]
		[BackendActionContext(ActionCode.AddNewContent)]
		[BackendActionLog]
		[ValidateInput(false)]
		[Record]	
		public ActionResult New(string tabId, int parentId, string backendActionCode, FormCollection collection)
		{
			Content content = ContentService.NewForSave(parentId);
			ContentViewModel model = ContentViewModel.Create(content, tabId, parentId);
			TryUpdateModel(model);
			model.Validate(ModelState);
			if (ModelState.IsValid)
			{
				try
				{
					model.Data = ContentService.Save(model.Data);
					this.PersistResultId(model.Data.Id);
					this.PersistFieldIds(model.Data.GetFieldIds());
					this.PersistLinkIds(model.Data.GetLinkIds());
				}
				catch (VirtualContentProcessingException vcpe)
				{
					if (IsReplayAction())
						throw;
					ModelState.AddModelError("VirtualContentProcessingException", vcpe.Message);
					return this.JsonHtml("Properties", model);
				}
				return this.Redirect("Properties", new { tabId = tabId, parentId = parentId, id = model.Data.Id, successfulActionCode = backendActionCode });
			}
			else
				return this.JsonHtml("Properties", model);
		}

		[HttpGet]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ConnectionScope(ConnectionScopeMode.TransactionOn)]
		[ActionAuthorize(ActionCode.ContentProperties)]
		[EntityAuthorize(ActionTypeCode.Read, EntityTypeCode.Content, "id")]
		[BackendActionContext(ActionCode.ContentProperties)]
		public ActionResult Properties(string tabId, int parentId, int id, string successfulActionCode, bool? groupChanged)
		{
			Content content = ContentService.Read(id);
			ContentViewModel model = ContentViewModel.Create(content, tabId, parentId);
			model.GroupChanged = groupChanged.HasValue ? groupChanged.Value : false;
			model.SuccesfulActionCode = successfulActionCode;
			return this.JsonHtml("Properties", model);
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ConnectionScope(ConnectionScopeMode.TransactionOn)]
		[ActionAuthorize(ActionCode.UpdateContent)]
		[BackendActionContext(ActionCode.UpdateContent)]
		[BackendActionLog]
		[ValidateInput(false)]
		[Record(ActionCode.ContentProperties)]
		public ActionResult Properties(string tabId, int parentId, int id, string backendActionCode, FormCollection collection)
		{
			Content content = ContentService.ReadForUpdate(id);
			ContentViewModel model = ContentViewModel.Create(content, tabId, parentId);
			int oldGroupId = model.Data.GroupId;
			TryUpdateModel(model);
			model.Validate(ModelState);
			if (ModelState.IsValid)
			{
				try
				{
					model.Data = ContentService.Update(model.Data);
				}
				catch (VirtualContentProcessingException vcpe)
				{
					if (IsReplayAction())
						throw;
					ModelState.AddModelError("VirtualContentProcessingException", vcpe.Message);
					return this.JsonHtml("Properties", model);
				}
				return this.Redirect("Properties", new { tabId = tabId, parentId = parentId, id = model.Data.Id, successfulActionCode = backendActionCode, groupChanged = oldGroupId != model.Data.GroupId });
			}
			else
				return this.JsonHtml("Properties", model);
		}

		[HttpGet]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ConnectionScope(ConnectionScopeMode.TransactionOn)]
		[ActionAuthorize(ActionCode.AddNewContentGroup)]
		[EntityAuthorize(ActionTypeCode.Update, EntityTypeCode.Site, "parentId")]
		[BackendActionContext(ActionCode.AddNewContentGroup)]
		public ActionResult NewGroup(string tabId, int parentId)
		{
			ContentGroup group = ContentService.NewGroup(parentId);
			ContentGroupViewModel model = ContentGroupViewModel.Create(group, tabId, parentId);
			return this.JsonHtml("GroupProperties", model);
		}


		[HttpPost]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ConnectionScope(ConnectionScopeMode.TransactionOn)]
		[ActionAuthorize(ActionCode.AddNewContentGroup)]
		[BackendActionContext(ActionCode.AddNewContentGroup)]
		[BackendActionLog]
		[Record]
		public ActionResult NewGroup(string tabId, int parentId, FormCollection collection)
		{
			ContentGroup group = ContentService.NewGroupForSave(parentId);
			ContentGroupViewModel model = ContentGroupViewModel.Create(group, tabId, parentId);
			TryUpdateModel(model);
			model.Validate(ModelState);
			if (ModelState.IsValid)
			{
				model.Data = ContentService.SaveGroup(model.Data);
				this.PersistResultId(model.Data.Id);
				return Redirect("GroupProperties", new { tabId = tabId, parentId = parentId, id = model.Data.Id, successfulActionCode = Constants.ActionCode.SaveContentGroup });
			}
			else
				return JsonHtml("GroupProperties", model);
		}

		[HttpGet]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ConnectionScope(ConnectionScopeMode.TransactionOn)]
		[ActionAuthorize(ActionCode.ContentGroupProperties)]
		[BackendActionContext(ActionCode.ContentGroupProperties)]
		public ActionResult GroupProperties(string tabId, int parentId, int id, string successfulActionCode)
		{
			ContentGroup group = ContentService.ReadGroup(id, parentId);
			ContentGroupViewModel model = ContentGroupViewModel.Create(group, tabId, parentId);
			model.SuccesfulActionCode = successfulActionCode;
			return this.JsonHtml("GroupProperties", model);
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ConnectionScope(ConnectionScopeMode.TransactionOn)]
		[ActionAuthorize(ActionCode.UpdateContentGroup)]
		[BackendActionContext(ActionCode.UpdateContentGroup)]
		[BackendActionLog]
		[Record(ActionCode.ContentGroupProperties)]
		public ActionResult GroupProperties(string tabId, int parentId, int id, FormCollection collection)
		{
			ContentGroup group = ContentService.ReadGroupForUpdate(id, parentId);
			ContentGroupViewModel model = ContentGroupViewModel.Create(group, tabId, parentId);
			TryUpdateModel(model);
			model.Validate(ModelState);
			if (ModelState.IsValid)
			{
				model.Data = ContentService.UpdateGroup(model.Data);
				return Redirect("GroupProperties", new { tabId = tabId, parentId = parentId, id = model.Data.Id, successfulActionCode = Constants.ActionCode.UpdateContentGroup });
			}
			else
				return JsonHtml("GroupProperties", model);
		}			

		#endregion

		#region non-interface actions

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.OperationAction)]
		[ConnectionScope(ConnectionScopeMode.TransactionOn)]
		[ActionAuthorize(ActionCode.CreateLikeContent)]
		[BackendActionContext(ActionCode.CreateLikeContent)]
		[BackendActionLog]
		[Record]
		public ActionResult Copy(int id, int? forceId, string forceFieldIds, string forceLinkIds)
		{
			ContentCopyResult result = ContentService.Copy(id, forceId, RecordReplayHelper.ToIntArray(forceFieldIds), RecordReplayHelper.ToIntArray(forceLinkIds));
			this.PersistResultId(result.Id);
			this.PersistFromId(id);
			this.PersistFieldIds(result.FieldIds);
			this.PersistLinkIds(result.LinkIds);
			return this.JsonMessageResult(result.Message);
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ConnectionScope(ConnectionScopeMode.TransactionOn)]
		[EntityAuthorize(ActionTypeCode.Update, EntityTypeCode.Content, "Id")]
		[ActionAuthorize(ActionCode.EnableArticlesPermissions)]
		[BackendActionContext(ActionCode.EnableArticlesPermissions)]
		[BackendActionLog]
		[Record]
		public ActionResult EnableArticlePermissions(int id)
		{
			MessageResult result = ContentService.EnableArticlePermissions(id);
			return this.JsonMessageResult(result);
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.OperationAction)]
		[ConnectionScope(ConnectionScopeMode.TransactionOn)]
		[EntityAuthorize(ActionTypeCode.Remove, EntityTypeCode.Content, "Id")]
		[ActionAuthorize(ActionCode.SimpleRemoveContent)]
		[BackendActionContext(ActionCode.SimpleRemoveContent)]
		[BackendActionLog]
		public ActionResult SimpleRemove(int id)
		{
			MessageResult result = ContentService.SimpleRemove(id);
			return this.JsonMessageResult(result);
		}

		#endregion

		#region helper actions

		[HttpGet]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		public ActionResult SearchBlock(int id, string actionCode, string hostId)
		{
			ContentSearchBlockViewModel model = new ContentSearchBlockViewModel(id, actionCode, hostId);
			return this.JsonHtml("SearchBlock", model);
		}		

		#endregion

		#region library actions

		[HttpGet]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ActionAuthorize(ActionCode.ContentLibrary)]
		[BackendActionContext(ActionCode.ContentLibrary)]		
		public ActionResult Library(string tabId, int parentId, int id, int? filterFileTypeId, string subFolder, bool allowUpload = true)
		{
			LibraryResult result = ContentService.Library(id, subFolder);
			LibraryViewModel model = LibraryViewModel.Create(result, tabId, id, filterFileTypeId, allowUpload, LibraryMode.Content);
			return this.JsonHtml("Library", model);
		}

		[EntityAuthorize(ActionTypeCode.List, EntityTypeCode.ContentFolder, "gridParentId")]
		[GridAction(EnableCustomBinding = true)]
		public ActionResult _Files(GridCommand command, int gridParentId, [ModelBinder(typeof(JsonStringModelBinder<LibraryFileFilter>))] LibraryFileFilter searchQuery)
		{
			ListResult<FolderFile> serviceResult = ContentService.GetFileList(command.GetListCommand(), gridParentId, searchQuery);
			return View(new GridModel() { Data = serviceResult.Data, Total = serviceResult.TotalRecords });
		}

		/// <summary>
		/// Получить список файлов в папке
		/// </summary>
		/// <param name="folderId"></param>
		/// <param name="pageSize"></param>
		/// <param name="pageNumber"></param>
		/// <returns></returns>
		[HttpGet]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[EntityAuthorize(ActionTypeCode.List, EntityTypeCode.ContentFolder, "folderId")]
		public JsonResult _FileList(int folderId, int? fileTypeId, string fileNameFilter, int pageSize, int pageNumber, int fileShortNameLength = 15)
		{
			var serviceResult = ContentService.GetFileList(new ListCommand { PageSize = pageSize, StartPage = pageNumber + 1 }, folderId,
				new LibraryFileFilter
				{
					FileType = (FolderFileType?)fileTypeId,
					FileNameFilter = fileNameFilter
				});

			return new JsonResult
			{
				Data = new
				{
					success = true,
					data = new ListResult<FileListItem>
					{
						Data = serviceResult.Data.Select(f => FileListItem.Create(f, fileShortNameLength)).ToList(),
						TotalRecords = serviceResult.TotalRecords
					}
				},
				JsonRequestBehavior = JsonRequestBehavior.AllowGet
			};
		}

		[HttpGet]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[EntityAuthorize(ActionTypeCode.Read, EntityTypeCode.ContentFolder, "folderId")]
		public JsonResult _FolderPath(int folderId)
		{
			Folder folder = ContentFolderService.GetById(folderId);
			return new JsonResult
			{
				Data = new
				{
					success = true,
					path = folder.PathInfo.Path,
					url = folder.PathInfo.Url,
					libraryPath = folder.Path,
				},
				JsonRequestBehavior = JsonRequestBehavior.AllowGet
			};
		}

		#endregion

		#region fields actions
		[HttpGet]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[EntityAuthorize(ActionTypeCode.Read, EntityTypeCode.Content, "contentId")]
		public ActionResult _RelateableFields(int? contentId, int? fieldId)
		{			
			if(contentId == null)
				return new JsonResult
				{
					Data = new
					{
						success = true						
					},
					JsonRequestBehavior = JsonRequestBehavior.AllowGet
				};

			var content = ContentService.Read(contentId.Value);
			if(content == null)
				throw new ArgumentException(String.Format(ContentStrings.ContentNotFound, contentId.Value));

			Func<Field, bool> fieldFilter = f => true;
			if (fieldId.HasValue)
				fieldFilter = f => !f.IsNew && f.Id != fieldId.Value;
			
			return new JsonResult
			{
				Data = new
				{
					success = true,
					data = content.RelateableFields
							.Where(fieldFilter)
							.Select(f => new {id = f.Id, text = f.Name })
				},
				JsonRequestBehavior = JsonRequestBehavior.AllowGet
			};
		}

		[HttpGet]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[EntityAuthorize(ActionTypeCode.Read, EntityTypeCode.Content, "contentId")]
		public ActionResult _ClassifierFields(int contentId)
		{
			var content = ContentService.Read(contentId);
			if (content == null)
				throw new ArgumentException(String.Format(ContentStrings.ContentNotFound, contentId));

			return new JsonResult
			{
				Data = new
				{
					success = true,
					data = content.Fields
						.Where(f => f.IsClassifier)
						.OrderBy(f => f.Id)
						.Select(f => new { id = f.Id, text = f.Name })
				},
				JsonRequestBehavior = JsonRequestBehavior.AllowGet
			};			
		}		
		#endregion

		#region select actions

		#region single actions

		[HttpGet]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ActionAuthorize(ActionCode.SelectContent)]
		[BackendActionContext(ActionCode.SelectContent)]
		public ActionResult Select(string tabId, int parentId, int[] IDs)
		{
			ContentInitListResult result = ContentService.InitList(parentId);
			ContentSelectableListViewModel model = new ContentSelectableListViewModel(result, tabId, parentId, IDs);
			return this.JsonHtml("SelectIndex", model);
		}

		[HttpPost]
		[GridAction(EnableCustomBinding = true)]
		[ActionAuthorize(ActionCode.SelectContent)]
		[BackendActionContext(ActionCode.SelectContent)]
		public ActionResult _Select(string tabId, int parentId, int id, GridCommand command,
			[Bind(Prefix = "searchQuery")] [ModelBinder(typeof(JsonStringModelBinder<ContentListFilter>))] ContentListFilter filter)
		{
			filter = filter ?? new ContentListFilter();
			filter.SiteId = parentId;
			ListResult<ContentListItem> serviceResult = ContentService.List(filter, command.GetListCommand(), id);
			return View(new GridModel() { Data = serviceResult.Data, Total = serviceResult.TotalRecords });
		}

		[HttpGet]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ActionAuthorize(ActionCode.SelectContentForObjectContainer)]
		[BackendActionContext(ActionCode.SelectContentForObjectContainer)]
		public ActionResult SelectForObjectContainer(string tabId, int parentId, int id)
		{
			ContentInitListResult result = ContentService.InitListForObject();
			ObjectContentViewModel model = new ObjectContentViewModel(result, tabId, parentId, new[] { id }, ContentSelectMode.ForContainer);
			return this.JsonHtml("SelectIndex", model);
		}

		[HttpPost]
		[GridAction(EnableCustomBinding = true)]
		[ActionAuthorize(ActionCode.SelectContentForObjectContainer)]
		[BackendActionContext(ActionCode.SelectContentForObjectContainer)]
		public ActionResult _SelectForObjectContainer(string tabId, int id, GridCommand command, int parentId,
			[Bind(Prefix = "searchQuery")] [ModelBinder(typeof(JsonStringModelBinder<ContentListFilter>))] ContentListFilter filter)
		{
			filter = filter ?? new ContentListFilter();
			filter.SiteId = parentId;
			ListResult<ContentListItem> serviceResult = ContentService.ListForContainer(filter, command.GetListCommand(), id);
			return View(new GridModel() { Data = serviceResult.Data, Total = serviceResult.TotalRecords });
		}

		[HttpGet]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ActionAuthorize(ActionCode.SelectContentForObjectForm)]
		[BackendActionContext(ActionCode.SelectContentForObjectForm)]
		public ActionResult SelectForObjectForm(string tabId, int parentId, int id)
		{
			ContentInitListResult result = ContentService.InitListForObject();
			ObjectContentViewModel model = new ObjectContentViewModel(result, tabId, parentId, new[] { id }, ContentSelectMode.ForForm);
			return this.JsonHtml("SelectIndex", model);
		}

		[HttpPost]
		[GridAction(EnableCustomBinding = true)]
		[ActionAuthorize(ActionCode.SelectContentForObjectForm)]
		[BackendActionContext(ActionCode.SelectContentForObjectForm)]
		public ActionResult _SelectForObjectForm(string tabId, int id, GridCommand command, int parentId,
			[Bind(Prefix = "searchQuery")] [ModelBinder(typeof(JsonStringModelBinder<ContentListFilter>))] ContentListFilter filter)
		{
			filter = filter ?? new ContentListFilter();
			filter.SiteId = parentId;
			ListResult<ContentListItem> serviceResult = ContentService.ListForForm(filter, command.GetListCommand(), id);
			return View(new GridModel() { Data = serviceResult.Data, Total = serviceResult.TotalRecords });
		}

		[HttpGet]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ActionAuthorize(ActionCode.SelectContentForJoin)]
		[BackendActionContext(ActionCode.SelectContentForJoin)]
		public ActionResult SelectForJoin(string tabId, int parentId, int id)
		{
			ContentInitListResult result = ContentService.InitList(parentId);
			JoinContentViewModel model = new JoinContentViewModel(result, tabId, parentId, new[] { id });
			return this.JsonHtml("SelectIndex", model);
		}

		[HttpPost]
		[GridAction(EnableCustomBinding = true)]
		[ActionAuthorize(ActionCode.SelectContentForJoin)]
		[BackendActionContext(ActionCode.SelectContentForJoin)]
		public ActionResult _SelectForJoin(string tabId, int parentId, int id, GridCommand command,
			[Bind(Prefix = "searchQuery")] [ModelBinder(typeof(JsonStringModelBinder<ContentListFilter>))] ContentListFilter filter)
		{
			filter = filter ?? new ContentListFilter();
			filter.SiteId = parentId;
			ListResult<ContentListItem> serviceResult = ContentService.ListForJoin(filter, command.GetListCommand(), id);
			return View(new GridModel() { Data = serviceResult.Data, Total = serviceResult.TotalRecords });
		}

		[HttpGet]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ActionAuthorize(ActionCode.SelectContentForField)]
		[BackendActionContext(ActionCode.SelectContentForField)]
		public ActionResult SelectForField(string tabId, int parentId, int id)
		{
			ContentInitListResult result = ContentService.InitList(parentId);
			ContentSelectableListViewModel model = new FieldContentViewModel(result, tabId, parentId, new[] { id });
			return this.JsonHtml("SelectIndex", model);
		}

		[HttpPost]
		[GridAction(EnableCustomBinding = true)]
		[ActionAuthorize(ActionCode.SelectContentForField)]
		[BackendActionContext(ActionCode.SelectContentForField)]
		public ActionResult _SelectForField(string tabId, int parentId, int id, GridCommand command,
			[Bind(Prefix = "searchQuery")] [ModelBinder(typeof(JsonStringModelBinder<ContentListFilter>))] ContentListFilter filter)
		{
			filter = filter ?? new ContentListFilter();
			filter.SiteId = parentId;
			ListResult<ContentListItem> serviceResult = ContentService.ListForField(filter, command.GetListCommand(), id);
			return View(new GridModel() { Data = serviceResult.Data, Total = serviceResult.TotalRecords });
		}


		#endregion

		#region multiple actions

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ActionAuthorize(ActionCode.MultipleSelectContent)]
		[BackendActionContext(ActionCode.MultipleSelectContent)]
		public ActionResult MultipleSelect(string tabId, int parentId, int[] IDs)
		{
			ContentInitListResult result = ContentService.InitList(parentId);
			ContentSelectableListViewModel model = new ContentSelectableListViewModel(result, tabId, parentId, IDs);
			model.IsMultiple = true;
			return this.JsonHtml("MultiSelectIndex", model);
		}

		[HttpPost]
		[GridAction(EnableCustomBinding = true)]
		[ActionAuthorize(ActionCode.MultipleSelectContent)]
		[BackendActionContext(ActionCode.MultipleSelectContent)]
		public ActionResult _MultipleSelect(string tabId, int parentId, string IDs, GridCommand command,
			[Bind(Prefix = "searchQuery")] [ModelBinder(typeof(JsonStringModelBinder<ContentListFilter>))] ContentListFilter filter)
		{
			filter = filter ?? new ContentListFilter();
			filter.SiteId = parentId;
			ListResult<ContentListItem> serviceResult = ContentService.List(filter, command.GetListCommand(), Converter.ToInt32Collection(IDs, ','));
			return View(new GridModel() { Data = serviceResult.Data, Total = serviceResult.TotalRecords });
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ActionAuthorize(ActionCode.MultipleSelectContentForCustomAction)]
		[BackendActionContext(ActionCode.MultipleSelectContentForCustomAction)]
		public ActionResult MultipleSelectForCustomAction(string tabId, int parentId, int[] IDs)
		{
			ContentInitListResult result = ContentService.InitList(parentId);
			ContentSelectableListViewModel model = new CustomActionContentViewModel(result, tabId, parentId, IDs);
			model.IsMultiple = true;
			return this.JsonHtml("MultiSelectIndex", model);
		}

		[HttpPost]
		[GridAction(EnableCustomBinding = true)]
		[ActionAuthorize(ActionCode.MultipleSelectContentForCustomAction)]
		[BackendActionContext(ActionCode.MultipleSelectContentForCustomAction)]
		public ActionResult _MultipleSelectForCustomAction(string tabId, int parentId, string IDs, GridCommand command,
			[Bind(Prefix = "searchQuery")] [ModelBinder(typeof(JsonStringModelBinder<ContentListFilter>))] ContentListFilter filter, string customFilter)
		{
			filter = filter ?? new ContentListFilter();
			if (!String.IsNullOrEmpty(customFilter))
				filter.CustomFilter = customFilter;
			ListResult<ContentListItem> serviceResult = ContentService.ListForCustomAction(filter, command.GetListCommand(), Converter.ToInt32Collection(IDs, ','));
			return View(new GridModel() { Data = serviceResult.Data, Total = serviceResult.TotalRecords });
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ActionAuthorize(ActionCode.MultipleSelectContentForWorkflow)]
		[BackendActionContext(ActionCode.MultipleSelectContentForWorkflow)]
		public ActionResult MultipleSelectForWorkflow(string tabId, int parentId, int[] IDs)
		{
			ContentInitListResult result = ContentService.InitList(parentId);
			WorkflowContentViewModel model = new WorkflowContentViewModel(result, tabId, parentId, IDs);
			model.IsMultiple = true;
			return this.JsonHtml("MultiSelectIndex", model);
		}

		[HttpPost]
		[GridAction(EnableCustomBinding = true)]
		[ActionAuthorize(ActionCode.MultipleSelectContentForWorkflow)]
		[BackendActionContext(ActionCode.MultipleSelectContentForWorkflow)]
		public ActionResult _MultipleSelectForWorkflow(string tabId, int parentId, string IDs, GridCommand command,
			[Bind(Prefix = "searchQuery")] [ModelBinder(typeof(JsonStringModelBinder<ContentListFilter>))] ContentListFilter filter, string customFilter)
		{
			filter = filter ?? new ContentListFilter();
			filter.SiteId = parentId;
			filter.CustomFilter = customFilter;
			ListResult<ContentListItem> serviceResult = ContentService.ListForWorkflow(filter, command.GetListCommand(), Converter.ToInt32Collection(IDs, ','));
			return View(new GridModel() { Data = serviceResult.Data, Total = serviceResult.TotalRecords });
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ActionAuthorize(ActionCode.MultipleSelectContentForUnion)]
		[BackendActionContext(ActionCode.MultipleSelectContentForUnion)]
		public ActionResult MultipleSelectForUnion(string tabId, int parentId, int[] IDs)
		{
			ContentInitListResult result = ContentService.InitList(parentId);
			UnionContentViewModel model = new UnionContentViewModel(result, tabId, parentId, IDs);
			model.IsMultiple = true;
			return this.JsonHtml("MultiSelectIndex", model);
		}

		[HttpPost]
		[GridAction(EnableCustomBinding = true)]
		[ActionAuthorize(ActionCode.MultipleSelectContentForUnion)]
		[BackendActionContext(ActionCode.MultipleSelectContentForUnion)]
		public ActionResult _MultipleSelectForUnion(string tabId, int parentId, string IDs, GridCommand command,
			[Bind(Prefix = "searchQuery")] [ModelBinder(typeof(JsonStringModelBinder<ContentListFilter>))] ContentListFilter filter)
		{
			filter = filter ?? new ContentListFilter();
			filter.SiteId = parentId;
			ListResult<ContentListItem> serviceResult = ContentService.ListForUnion(filter, command.GetListCommand(), Converter.ToInt32Collection(IDs, ','));
			return View(new GridModel() { Data = serviceResult.Data, Total = serviceResult.TotalRecords });
		}

		#endregion

		#endregion

	}
}
