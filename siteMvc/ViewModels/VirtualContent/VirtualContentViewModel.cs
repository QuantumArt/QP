using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using C = Quantumart.QP8.Constants;
using B = Quantumart.QP8.BLL;
using Quantumart.QP8.BLL;
using Quantumart.QP8.WebMvc.Extensions.Helpers;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Validators;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Utils;
using Quantumart.QP8.BLL.Helpers;

namespace Quantumart.QP8.WebMvc.ViewModels.VirtualContent
{
	public class VirtualContentViewModel : ContentViewModelBase
	{		

		#region creation

		public static VirtualContentViewModel Create(B.Content content, string tabId, int parentId)
		{
			var viewModel = EntityViewModel.Create<VirtualContentViewModel>(content, tabId, parentId);
			viewModel.Init();
			return viewModel;
		}		

		#endregion

		#region read-only members		
		public override string EntityTypeCode
		{
			get
			{
				return C.EntityTypeCode.VirtualContent;
			}
		}

		public override string ActionCode
		{
			get
			{
				if (this.IsNew)
				{					
					return C.ActionCode.AddNewVirtualContents;
				}
				else
				{
					return C.ActionCode.VirtualContentProperties;
				}
			}
		}
		#endregion		
			
		[LocalizedDisplayName("JoinFields", NameResourceType = typeof(ContentStrings))]
		public IList<QPTreeCheckedNode> JoinFields {get; set;}
		[LocalizedDisplayName("ToRebuild", NameResourceType = typeof(ContentStrings))]
		public bool ToBuild { get; set; }

		public string JoinRootElementId { get { return UniqueId("JoinRootSelect"); } }

		public string JoinFieldsElementId { get { return UniqueId("JoinFieldsTree"); } }

		public string UnionSourcesElementId { get { return UniqueId("UnionSources"); } }

		public string VTypeRadioContainerElementId { get { return UniqueId("VRadioContainer"); } }

		/// <summary>
		/// Используется ли альтернативный запрос
		/// </summary>
		[LocalizedDisplayName("IsAltUserQueryUsed", NameResourceType = typeof(ContentStrings))]
		public bool IsAltUserQueryUsed { get; set; }

		#region Methods

		#region Init Model
		private void Init()
		{
			if (Data.IsNew)
				Data.VirtualType = VirtualType.Join;

			JoinFields = new List<QPTreeCheckedNode>(0);
			if (Data.VirtualType == VirtualType.Join)
				JoinFields = B.Content.VirtualFieldNode.Linearize(Data.VirtualJoinFieldNodes).Select(vfn => new QPTreeCheckedNode { Value = vfn.TreeId }).ToList();

			IsAltUserQueryUsed = !String.IsNullOrWhiteSpace(Data.UserQueryAlternative);
			
			ToBuild = IsNew;			
		}
		#endregion

		#region Update
		internal void Update()
		{
			// Если не перестраиваем контент - то удалить список Join полей
			if (!ToBuild)
			{
				JoinFields = new List<QPTreeCheckedNode>(0);
				Data.VirtualJoinFieldNodes = Enumerable.Empty<B.Content.VirtualFieldNode>();
			}
			else
			{
				if (Data.VirtualType == VirtualType.Join)
				{
					Data.VirtualJoinFieldNodes = B.Content.VirtualFieldNode.Parse(
						B.Content.VirtualFieldNode.NormalizeFieldTreeIdSeq(
							JoinFields.Select(f => f.Value)));
				}
				else
				{
					JoinFields = new List<QPTreeCheckedNode>(0);
					Data.VirtualJoinFieldNodes = Enumerable.Empty<B.Content.VirtualFieldNode>();
					Data.JoinRootId = null;
				}

				if (Data.VirtualType != VirtualType.Union)
				{
					Data.UnionSourceContentIDs = Enumerable.Empty<int>();
				}

				if (Data.VirtualType != VirtualType.UserQuery)
				{
					Data.UserQuery = null;
					Data.UserQueryAlternative = null;
				}
				else if (Data.VirtualType == VirtualType.UserQuery && !IsAltUserQueryUsed)
				{
					Data.UserQueryAlternative = null;
				}

			}
		}
		#endregion
		
		#region List<ListItem> Providers
		/// <summary>
		/// Возвращает список контентов доступных для связи с текущим контентом
		/// </summary>
		public IEnumerable<B.ListItem> GetAcceptableContentForVirtualJoin()
		{
			var currentContentSiteId = this.Data.SiteId;
			var contentForJoin = VirtualContentService.GetAcceptableContentForVirtualJoin(currentContentSiteId)
				.Select(c => { c.DependentItemIDs = new[] { "JoinFieldsPanel" }; c.HasDependentItems = true; return c; })
				.ToArray();
			
			return new[] { new ListItem("", ContentStrings.SelectContent) }.Concat(contentForJoin);
		}

		public IEnumerable<B.ListItem> GetVirtualTypes()
		{
			return new[]
			{
				new ListItem(VirtualType.Join.ToString(), B.Content.GetVirtualTypeString(VirtualType.Join), "JoinTypePanel"),
				new ListItem(VirtualType.Union.ToString(), B.Content.GetVirtualTypeString(VirtualType.Union), "UnionTypePanel"),
				new ListItem(VirtualType.UserQuery.ToString(), B.Content.GetVirtualTypeString(VirtualType.UserQuery), "UserQueryTypePanel")
			};
		}

		public IEnumerable<B.ListItem> GetContentsForUnion()
		{
			List<int> callIds = new List<int>();
			if (Data.UnionSourceContentIDs != null)
				callIds.AddRange(Data.UnionSourceContentIDs);
			return ContentService.GetContentsForUnion(Data.SiteId, callIds);
		}

		public QPSelectListItem JoinListItem
		{
			get
			{
				return Data.JoinRootId.HasValue ?
					new QPSelectListItem { Value = Data.JoinRootId.Value.ToString(), Text = ContentService.GetNameById(Data.JoinRootId.Value), Selected = true } :
					null;
			}
		}

		#endregion

		#endregion				
	}
}