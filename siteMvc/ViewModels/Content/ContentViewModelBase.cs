using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.Resources;
using Quantumart.QP8.WebMvc.Extensions.Helpers;
using B = Quantumart.QP8.BLL;

namespace Quantumart.QP8.WebMvc.ViewModels
{
	public abstract class ContentViewModelBase : EntityViewModel
	{
		public readonly string ClassBlock = "class";

		public new B.Content Data
		{
			get
			{
				return (B.Content)EntityData;
			}
			set
			{
				EntityData = value;
			}
		}

		public List<ListItem> Groups
		{
			get
			{
				return Data.Site.ContentGroups.Select(n => new ListItem(n.Id.ToString(), n.Name)).ToList();
			}
		}

		public bool GroupChanged { get; set; }

		public override ExpandoObject MainComponentOptions
		{
			get
			{
				dynamic result = base.MainComponentOptions;
				result.groupChanged = GroupChanged;
				return result;
			}
		}

		/// <summary>
		/// Возвращает список контентов доступных для выбора в качестве родительского
		/// </summary>
		public IEnumerable<B.ListItem> GetContentsForParent()
		{
			var contents = ContentService.GetContentsForParentContent(Data.SiteId, Data.Id).ToArray();
			return new[] { new ListItem("", ContentStrings.SelectParentContent) }.Concat(contents);
		}

		public SelectOptions SelectParentOptions
		{
			get
			{
				SelectOptions options = new SelectOptions();
				options.ReadOnly = !Data.IsNew;
				return options;
			}
		}
		
		public SelectOptions SelectGroupOptions
		{
			get
			{
				SelectOptions options = new SelectOptions();
				options.EntityDataListArgs = new EntityDataListArgs();
				options.EntityDataListArgs.EntityTypeCode = Constants.EntityTypeCode.ContentGroup;
				options.EntityDataListArgs.ParentEntityId = Data.SiteId;
				options.EntityDataListArgs.EntityId = Data.Id;
				options.EntityDataListArgs.AddNewActionCode = Constants.ActionCode.AddNewContentGroup;
				options.EntityDataListArgs.ReadActionCode = Constants.ActionCode.ContentGroupProperties;				
				return options;
			}
		}
	}
}