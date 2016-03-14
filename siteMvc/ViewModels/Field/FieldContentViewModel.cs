using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.WebMvc.ViewModels.Content;
using C = Quantumart.QP8.Constants;

namespace Quantumart.QP8.WebMvc.ViewModels.Field
{
	public class FieldContentViewModel: ContentSelectableListViewModel
	{
		public FieldContentViewModel(ContentInitListResult result, string tabId, int parentId, int[] IDs) : base(result, tabId, parentId, IDs) { }


		public override string ActionCode
		{
			get
			{
				return C.ActionCode.SelectContentForField;
			}
		}

		public override string GetDataAction
		{
			get
			{
				return "_SelectForField";
			}
		}
	}
}