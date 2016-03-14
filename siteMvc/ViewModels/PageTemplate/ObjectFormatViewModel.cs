using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using B = Quantumart.QP8.BLL;
using C = Quantumart.QP8.Constants;

namespace Quantumart.QP8.WebMvc.ViewModels.PageTemplate
{
	public class ObjectFormatViewModel : LockableEntityViewModel
	{						
		private IFormatService _service;		

		private int _parentId;

		bool _pageOrTemplate;

		public override string EntityTypeCode
		{
			get { return _pageOrTemplate ? Constants.EntityTypeCode.PageObjectFormat : Constants.EntityTypeCode.TemplateObjectFormat; }
		}

		internal static ObjectFormatViewModel Create(BLL.ObjectFormat format, string tabId, int parentId, BLL.Services.IFormatService _formatService, bool pageOrTemplate)
		{
			var model = EntityViewModel.Create<ObjectFormatViewModel>(format, tabId, parentId);
			model._service = _formatService;
			model._parentId = parentId;
			model._pageOrTemplate = pageOrTemplate;			
			return model;
		}

		public new B.ObjectFormat Data
		{
			get
			{
				return (B.ObjectFormat)EntityData;
			}
			set
			{
				EntityData = value;
			}
		}

		public override string ActionCode
		{
			get
			{
				if (IsNew) { return _pageOrTemplate ? C.ActionCode.AddNewPageObjectFormat : C.ActionCode.AddNewTemplateObjectFormat; }
				else { return _pageOrTemplate ? C.ActionCode.PageObjectFormatProperties : C.ActionCode.TemplateObjectFormatProperties; }
			}
		}

		public override string CaptureLockActionCode
		{
			get
			{
				return Data.PageOrTemplate ? C.ActionCode.CaptureLockPageObjectFormat : C.ActionCode.CaptureLockTemplateObjectFormat;
			}
		}

		private List<B.ListItem> _languages;
		public List<B.ListItem> Languages
		{
			get
			{
				if (_languages == null)
				{
					_languages = _service.GetNetLanguagesAsListItems().ToList();
				}
				return _languages;
			}
		}

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