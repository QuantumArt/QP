using System.Collections.Generic;
using System.Linq;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.WebMvc.ViewModels.Abstract;

namespace Quantumart.QP8.WebMvc.ViewModels.PageTemplate
{
    public class ObjectFormatViewModel : LockableEntityViewModel
    {
        private IFormatService _service;

        private bool _pageOrTemplate;

        public override string EntityTypeCode => _pageOrTemplate ? Constants.EntityTypeCode.PageObjectFormat : Constants.EntityTypeCode.TemplateObjectFormat;

        internal static ObjectFormatViewModel Create(ObjectFormat format, string tabId, int parentId, IFormatService formatService, bool pageOrTemplate)
        {
            var model = Create<ObjectFormatViewModel>(format, tabId, parentId);
            model._service = formatService;
            model._pageOrTemplate = pageOrTemplate;
            return model;
        }

        public ObjectFormat Data
        {
            get => (ObjectFormat)EntityData;
            set => EntityData = value;
        }

        public override string ActionCode
        {
            get
            {
                if (IsNew)
                {
                    return _pageOrTemplate ? Constants.ActionCode.AddNewPageObjectFormat : Constants.ActionCode.AddNewTemplateObjectFormat;
                }

                return _pageOrTemplate ? Constants.ActionCode.PageObjectFormatProperties : Constants.ActionCode.TemplateObjectFormatProperties;
            }
        }

        public override string CaptureLockActionCode => Data.PageOrTemplate ? Constants.ActionCode.CaptureLockPageObjectFormat : Constants.ActionCode.CaptureLockTemplateObjectFormat;

        private List<ListItem> _languages;

        public List<ListItem> Languages => _languages ?? (_languages = _service.GetNetLanguagesAsListItems().ToList());

        public int TemplateId
        {
            get
            {
                var obj = _service.ReadObjectProperties(Data.ObjectId, false);
                return obj.PageTemplateId;
            }
        }

        public int SiteId
        {
            get
            {
                var obj = _service.ReadObjectProperties(Data.ObjectId, false);
                var template = _service.ReadPageTemplateProperties(obj.PageTemplateId, false);
                return template.SiteId;
            }
        }
    }
}
