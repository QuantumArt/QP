using System.Collections.Generic;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL.Services.DTO;
using C = Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.WebMvc.ViewModels.VisualEditor
{
	public class VisualEditorStyleListViewModel : ListViewModel
	{
		public IEnumerable<VisualEditorPluginListItem> Data { get; set; }

		public string GettingDataActionName
		{
			get
			{
				return "_Index";
			}
		}

		public static VisualEditorStyleListViewModel Create(VisualEditorStyleInitListResult result, string tabId, int parentId)
		{
			var model = ViewModel.Create<VisualEditorStyleListViewModel>(tabId, parentId);
			model.ShowAddNewItemButton = result.IsAddNewAccessable && !model.IsWindow;
			return model;
		}

		public override string EntityTypeCode
		{
			get { return C.EntityTypeCode.VisualEditorStyle; }
		}

		public override string ActionCode
		{
			get { return C.ActionCode.VisualEditorStyles; }
		}

		public override string AddNewItemActionCode
		{
			get
			{
				return C.ActionCode.AddNewVisualEditorStyle;
			}
		}

		public override string AddNewItemText
		{
			get
			{
				return VisualEditorStrings.AddNewVisualEditorStyle;
			}
		}
	}
}