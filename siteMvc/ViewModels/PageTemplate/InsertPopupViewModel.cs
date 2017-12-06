using System.Collections.Generic;
using System.Linq;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Validators;
using Quantumart.QP8.WebMvc.Extensions.Helpers;

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
            _templateId = templateId;
            _service = service;
            _pageId = pageId;
            _contentId = contentId;
            InitDdls();
        }

        private readonly IPageTemplateService _service;

        [LocalizedDisplayName("Function", NameResourceType = typeof(TemplateStrings))]
        public int? SelectedFunction { get; set; }

        [LocalizedDisplayName("ContainerProp", NameResourceType = typeof(TemplateStrings))]
        public int? SelectedContainerProp { get; set; }

        [LocalizedDisplayName("PageObject", NameResourceType = typeof(TemplateStrings))]
        public int? SelectedPageObject { get; set; }

        [LocalizedDisplayName("TemplateObject", NameResourceType = typeof(TemplateStrings))]
        public int? SelectedTemplateObject { get; set; }

        [LocalizedDisplayName("RestTemplateObject", NameResourceType = typeof(TemplateStrings))]
        public int? SelectedRestObject { get; set; }

        [LocalizedDisplayName("Field", NameResourceType = typeof(TemplateStrings))]
        public int? SelectdField { get; set; }

        public IEnumerable<ListItem> Functions { get; set; }

        public IEnumerable<ListItem> ContainerProps { get; set; }

        public IEnumerable<ListItem> TemplateObjects { get; set; }

        public IEnumerable<ListItem> RestTemplateObjects { get; set; }

        public IEnumerable<ListItem> PageObjects { get; set; }

        private IEnumerable<BllObject> BllTemplateObjects { get; set; }

        private IEnumerable<TemplateObjectFormatDto> BllRestTemplateObjects { get; set; }

        private IEnumerable<BllObject> BllPageObjects { get; set; }

        public IEnumerable<ListItem> Fields { get; set; }

        public bool IsContainer { get; set; }

        public bool IsForm { get; set; }

        private readonly int _templateId;

        private int? _pageId;

        private int? _contentId;

        public bool IsPage => _pageId.HasValue;

        private void InitDdls()
        {
            BllTemplateObjects = _service.GetAllTemplateObjects(_templateId).OrderBy(x => x.Name);
            BllRestTemplateObjects = _service.GetRestTemplateObjects(_templateId);
            if (_pageId.HasValue)
            {
                BllPageObjects = _service.GetAllPageObjects(_pageId.Value).OrderBy(x => x.Name);
            }

            PageObjects = HighlightedTAreaToolbarHelper.GenerateObjectListItems(BllPageObjects);
            TemplateObjects = HighlightedTAreaToolbarHelper.GenerateObjectListItems(BllTemplateObjects);
            RestTemplateObjects = HighlightedTAreaToolbarHelper.GenerateRestObjectListItems(BllRestTemplateObjects);

            if (IsContainer || IsForm)
            {
                var content = _service.ReadContentProperties(_contentId.Value);
                var fields = ServiceField.CreateAll()
                    .Select(f => new ListItem { Text = f.ColumnName, Value = f.ColumnName })
                    .Concat(content.Fields.Select(x => new ListItem { Text = x.Name, Value = x.Name }))
                    .ToList();

                fields.Insert(0, new ListItem { Text = TemplateStrings.SelectField, Value = string.Empty });
                Fields = fields;
            }
        }
    }
}
