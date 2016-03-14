using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using B = Quantumart.QP8.BLL;
using C = Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.WebMvc.Extensions.Helpers;
using Quantumart.QP8.BLL;
using System.Dynamic;

namespace Quantumart.QP8.WebMvc.ViewModels
{
	public class ContentViewModel : ContentViewModelBase
	{

		public readonly string SplitBlock = "split";
		public readonly string VersionsBlock = "versions";
		public readonly string XamlValidationPanel = "xamlvalidation";
		public readonly string ContextBlock = "context";
				

		#region creation
		
		public static ContentViewModel Create(B.Content content, string tabId, int parentId)
		{
			return EntityViewModel.Create<ContentViewModel>(content, tabId, parentId);
		}

		#endregion

		#region read-only members

		public override string EntityTypeCode
		{
			get
			{
				return C.EntityTypeCode.Content;
			}
		}

		public override string ActionCode
		{
			get
			{
				if (this.IsNew)
				{
					return C.ActionCode.AddNewContent;
				}
				else
				{
					return C.ActionCode.ContentProperties;
				}
			}
		}

		public string SiteName
		{
			get
			{
				return Data.Site.Name;
			}
		}
		

		public List<ListItem> Workflows
		{
			get
			{
				List<ListItem> result = Data.Site.Workflows.Select(n => new ListItem(n.Id.ToString(), n.Name, SplitBlock)).ToList();
				result.Add(new ListItem(WorkflowBind.UnassignedId.ToString(), ContentStrings.NoWorkflow));
				return result;
			}
		}

		public override ExpandoObject MainComponentOptions
		{
			get
			{
				dynamic result = base.MainComponentOptions;
				if (Data.HasAggregatedFields) // запретить агрегированным контентам менять некоторые поля
				{
					result.disabledFields = new string[] { "Data.AutoArchive", "Data.WorkflowBinding.WorkflowId", "Data.WorkflowBinding.IsAsync" };
				}
				return result;
			}
		}
		#endregion		
	}
}