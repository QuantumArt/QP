using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Validators;
using Quantumart.QP8.WebMvc.Extensions.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Quantumart.QP8.WebMvc.ViewModels.PageTemplate
{
	public class InsertPopupViewModel
	{
		public InsertPopupViewModel(int templateId, int? languageId, string assemblingType, bool presentationOrCodeBehind, bool isContainer, bool isForm, int? contentId, int? pageId, IPageTemplateService service)
		{
			Functions = HighlightedTAreaToolbarHelper.FunctionsAsListItems(languageId, assemblingType, presentationOrCodeBehind, isContainer);
			ContainerProps = HighlightedTAreaToolbarHelper.ContainerInfoPropertiesAsListItems(languageId, assemblingType, presentationOrCodeBehind);
			IsContainer = isContainer;
			IsForm = isForm;
			_TemplateId = templateId;
			_service = service;
			_PageId = pageId;
			_contentId = contentId;
			InitDDLs();
		}

		private IPageTemplateService _service;

		[LocalizedDisplayName("Function", NameResourceType = typeof(TemplateStrings))]
		public int? selectedFunction { get; set; }

		[LocalizedDisplayName("ContainerProp", NameResourceType = typeof(TemplateStrings))]
		public int? selectedContainerProp { get; set; }

		[LocalizedDisplayName("PageObject", NameResourceType = typeof(TemplateStrings))]
		public int? selectedPageObject { get; set;}

		[LocalizedDisplayName("TemplateObject", NameResourceType = typeof(TemplateStrings))]
		public int? selectedTemplateObject { get; set; }

		[LocalizedDisplayName("RestTemplateObject", NameResourceType = typeof(TemplateStrings))]
		public int? selectedRestObject { get; set; }

		[LocalizedDisplayName("Field", NameResourceType = typeof(TemplateStrings))]
		public int? selectdField { get; set; }

		public IEnumerable<ListItem> Functions { get; set; }

		public IEnumerable<ListItem> ContainerProps { get; set; }

		public IEnumerable<ListItem> TemplateObjects { get; set; }

		public IEnumerable<ListItem> RestTemplateObjects { get; set; }

		public IEnumerable<ListItem> PageObjects { get; set; }

		private IEnumerable<BllObject> _TemplateObjects { get; set; }

		private IEnumerable<TemplateObjectFormatDto> _RestTemplateObjects { get; set; }

		private IEnumerable<BllObject> _PageObjects { get; set; }

		public IEnumerable<ListItem> Fields { get; set; }

		public bool IsContainer { get; set; }		

		public bool IsForm { get; set; }

		private int _TemplateId;

		private int? _PageId;

		private int? _contentId;

		public bool IsPage { get { return _PageId.HasValue; } }

		private void InitDDLs()
		{
			_TemplateObjects = _service.GetAllTemplateObjects(_TemplateId).OrderBy(x => x.Name);
			_RestTemplateObjects = _service.GetRestTemplateObjects(_TemplateId);
			if(_PageId.HasValue)
				_PageObjects = _service.GetAllPageObjects(_PageId.Value).OrderBy(x => x.Name);

			PageObjects = HighlightedTAreaToolbarHelper.GenerateObjectListItems(_PageObjects);
			TemplateObjects = HighlightedTAreaToolbarHelper.GenerateObjectListItems(_TemplateObjects);
			RestTemplateObjects = HighlightedTAreaToolbarHelper.GenerateRestObjectListItems(_RestTemplateObjects);

			if (IsContainer || IsForm)
			{
				var content = _service.ReadContentProperties(_contentId.Value);
				var fields = ServiceField.CreateAll().Select(f => new ListItem { Text = f.ColumnName, Value = f.ColumnName })
					.Concat(content.Fields.Select(x => new ListItem { Text = x.Name, Value = x.Name })).ToList();
				fields.Insert(0, new ListItem { Text = TemplateStrings.SelectField, Value = string.Empty });
				Fields = fields;
			}
		}
	}
}