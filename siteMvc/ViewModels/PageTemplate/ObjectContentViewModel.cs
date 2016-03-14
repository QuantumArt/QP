using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.ViewModels.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using C = Quantumart.QP8.Constants;

namespace Quantumart.QP8.WebMvc.ViewModels.PageTemplate
{
	public class ObjectContentViewModel : ContentSelectableListViewModel
	{
		public ObjectContentViewModel(ContentInitListResult result, string tabId, int parentId, int[] IDs, ContentSelectMode selectMode) : base(result, tabId, parentId, IDs) 
		{
			_selectMode = selectMode;
		}

		private ContentSelectMode _selectMode;

		public override string ActionCode
		{
			get
			{
				if (_selectMode == ContentSelectMode.ForForm)
					return C.ActionCode.SelectContentForObjectForm;
				else
					return C.ActionCode.SelectContentForObjectContainer;
			}
		}


		public override string GetDataAction
		{
			get
			{
				if (_selectMode == ContentSelectMode.ForForm)
					return "_SelectForObjectForm";
				else
					return "_SelectForObjectContainer";
			}
		}
	}
}