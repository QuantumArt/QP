using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using C = Quantumart.QP8.Constants;

namespace Quantumart.QP8.WebMvc.ViewModels.PageTemplate
{
	public class ObjectFormatVersionListViewModel : ListViewModel
	{
		private bool _isTemplateObjectFormats { get; set; }//else it`s page object formats

		public IEnumerable<ObjectFormatVersionListItem> Data { get; set; }

		public string GettingDataActionName
		{
			get
			{
				return _isTemplateObjectFormats ? "_IndexTemplateObjectFormatVersions" : "_IndexPageObjectFormatVersions";
			}
		}

		public override string EntityTypeCode
		{
			get { return _isTemplateObjectFormats ? C.EntityTypeCode.TemplateObjectFormatVersion : C.EntityTypeCode.PageObjectFormatVersion; }
		}

		public override string ActionCode
		{
			get { return _isTemplateObjectFormats ? C.ActionCode.TemplateObjectFormatVersions : C.ActionCode.PageObjectFormatVersions; }
		}

		public override string AddNewItemText
		{
			get
			{
				return string.Empty;
			}
		}

		
		public override string AddNewItemActionCode
		{
			get { return _isTemplateObjectFormats ?  C.ActionCode.AddNewTemplateObjectFormatVersion : C.ActionCode.AddNewPageObjectFormatVersion; ; }
		}		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="result"></param>
		/// <param name="tabId"></param>
		/// <param name="parentId"></param>
		/// <param name="isTemplateObjectFormats">isTemplateObjectFormats</param>
		/// <returns></returns>
		public static ObjectFormatVersionListViewModel Create(FormatVersionInitListResult result, string tabId, int parentId, bool isTemplateObjectFormats)
		{
			var model = ViewModel.Create<ObjectFormatVersionListViewModel>(tabId, parentId);
			model._isTemplateObjectFormats = isTemplateObjectFormats;
			return model;
		}
	}
}