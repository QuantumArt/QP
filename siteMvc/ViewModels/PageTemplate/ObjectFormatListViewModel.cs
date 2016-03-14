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
	public class ObjectFormatListViewModel : ListViewModel
	{
		private bool _isTemplateObjectFormats { get; set; }//else it`s page object formats

		public IEnumerable<ObjectFormatListItem> Data { get; set; }

		public string GettingDataActionName
		{
			get
			{
				return _isTemplateObjectFormats ? "_IndexTemplateObjectFormats" : "_IndexPageObjectFormats";
			}
		}

		public override string EntityTypeCode
		{
			get { return _isTemplateObjectFormats ? C.EntityTypeCode.TemplateObjectFormat : C.EntityTypeCode.PageObjectFormat; }
		}

		public override string ActionCode
		{
			get { return _isTemplateObjectFormats ? C.ActionCode.TemplateObjectFormats : C.ActionCode.PageObjectFormats; }
		}

		public override string AddNewItemActionCode
		{
			get { return _isTemplateObjectFormats ? C.ActionCode.AddNewTemplateObjectFormat : C.ActionCode.AddNewPageObjectFormat; }
		}		

		public override string AddNewItemText
		{
			get
			{
				return TemplateStrings.AddNewFormat;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="result"></param>
		/// <param name="tabId"></param>
		/// <param name="parentId"></param>
		/// <param name="isTemplateObjectFormats">isTemplateObjectFormats</param>
		/// <returns></returns>
		public static ObjectFormatListViewModel Create(FormatInitListResult result, string tabId, int parentId, bool isTemplateObjectFormats)
		{
			var model = ViewModel.Create<ObjectFormatListViewModel>(tabId, parentId);
			model._isTemplateObjectFormats = isTemplateObjectFormats;
			model.ShowAddNewItemButton = result.IsAddNewAccessable && !model.IsWindow;
			return model;
		}
	}
}