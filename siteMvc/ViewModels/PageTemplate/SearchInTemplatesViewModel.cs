using Quantumart.QP8.Resources;
using Quantumart.QP8.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Quantumart.QP8.WebMvc.ViewModels.PageTemplate
{
	public sealed class SearchInTemplatesViewModel : ListViewModel
	{
		#region creation

		public static SearchInTemplatesViewModel Create(string tabId, int parentId)
		{
			SearchInTemplatesViewModel model = ViewModel.Create<SearchInTemplatesViewModel>(tabId, parentId);
			return model;
		}

		#endregion

		public override string EntityTypeCode
		{
			get { return Constants.EntityTypeCode.PageTemplate; }
		}

		public override string ActionCode
		{
			get { return Constants.ActionCode.SearchInTemplates; }
		}


		public string GridElementId
		{
			get
			{
				return UniqueId("Grid");
			}
		}

		public string FilterElementId { get { return UniqueId("Filter"); } }

		public override string ContextMenuCode
		{
			get
			{
				return String.Empty;
			}
		}

		public override bool LinkOpenNewTab
		{
			get
			{
				return true;
			}
		}

		public override string ActionCodeForLink
		{
			get
			{
				return Constants.ActionCode.PageTemplateProperties;
			}
		}
	}
}