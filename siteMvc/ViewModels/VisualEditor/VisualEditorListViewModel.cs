using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Quantumart.QP8.BLL.Services.DTO;
using C = Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;
using Quantumart.QP8.BLL.ListItems;

namespace Quantumart.QP8.WebMvc.ViewModels.VisualEditor
{
	public class VisualEditorListViewModel : ListViewModel
	{
		public IEnumerable<VisualEditorPluginListItem> Data { get; set; }

		public string GettingDataActionName
		{
			get
			{
				return "_Index";
			}
		}

		public static VisualEditorListViewModel Create(VisualEditorInitListResult result, string tabId, int parentId)
		{
			var model = ViewModel.Create<VisualEditorListViewModel>(tabId, parentId);
			model.ShowAddNewItemButton = result.IsAddNewAccessable && !model.IsWindow;
			return model;
		}

		public override string EntityTypeCode
		{
			get { return C.EntityTypeCode.VisualEditorPlugin; }
		}

		public override string ActionCode
		{
			get { return C.ActionCode.VisualEditorPlugins; }
		}

		public override string AddNewItemActionCode
		{
			get
			{
				return C.ActionCode.AddNewVisualEditorPlugin;
			}
		}

		public override string AddNewItemText
		{
			get
			{
				return VisualEditorStrings.AddNewVisualEditorPlugin;
			}
		}


	}
}