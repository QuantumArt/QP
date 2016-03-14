using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Validators;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.Extensions.Helpers;
using B = Quantumart.QP8.BLL;
using C = Quantumart.QP8.Constants;


namespace Quantumart.QP8.WebMvc.ViewModels.CustomAction
{
	public class CustomActionViewModel : EntityViewModel
	{
		public enum SelectionMode
		{
			ShowExpectSelected,
			HideExceptSelected
		}
		
		private ICustomActionService service;		

		public new B.CustomAction Data
		{
			get
			{
				return (B.CustomAction)EntityData;
			}
			set
			{
				EntityData = value;
			}
		}

		#region Methods

		public static CustomActionViewModel Create(B.CustomAction customAction, string tabId, int parentId, ICustomActionService service)
		{
			var model =  EntityViewModel.Create<CustomActionViewModel>(customAction, tabId, parentId);
			model.service = service;
			model.Init();
			return model;
		}

		/// <summary>
		/// Инициализирует View Model
		/// </summary>
		private void Init()
		{			
			if (!IsNew)
			{
				CustomActionTypeId = Data.Action.TypeId;
				CustomActionEntityTypeId = Data.Action.EntityTypeId;
				Order = Data.Order;
			}					 			

			SelectedSiteIDs = Data.Sites.Select(s => s.Id).ToArray();
			SelectedContentIDs = Data.Contents.Select(c => c.Id).ToArray();
			SelectedActions = (Data.ParentActions != null) ? 
				Data.ParentActions.Select(c => new QPCheckedItem { Value = c.ToString() }).ToList() : 
				Enumerable.Empty<QPCheckedItem>().ToList();
		}

		/// <summary>
		/// Устанавливает свойства модели, которые не могут быть установлены автоматически
		/// </summary>
		internal void DoCustomBinding()
		{												
			Data.Action.TypeId = CustomActionTypeId ?? 0;
			Data.Action.EntityTypeId = CustomActionEntityTypeId ?? 0;
			Data.Order = Order ?? 0;
			Data.Sites = service.GetSites(SelectedSiteIDs);
			Data.Contents = service.GetContents(SelectedContentIDs);			
		}


		public override void Validate(ModelStateDictionary modelState)
		{
			base.Validate(modelState);
			if (Data.Action.EntityTypeId > 0 && Data.ShowInToolbar && !SelectedActions.Any())
				modelState.AddModelError("SelectedActions", CustomActionStrings.ToolbarButtonParentActionNotSelected);
		}

		#endregion

		#region Override
		public override string EntityTypeCode
		{
			get { return C.EntityTypeCode.CustomAction; }
		}

		public override string ActionCode
		{
			get
			{
				if (this.IsNew)
				{
					return C.ActionCode.AddNewCustomAction;
				}
				else
				{
					return C.ActionCode.CustomActionsProperties;
				}
			}
		} 
		#endregion
		
		#region List Items Gettrs
		public IEnumerable<ListItem> ActionTypeList
		{
			get { return service.GetActionTypeList(); }
		}
		
		public IEnumerable<ListItem> EntityTypeList
		{
			get { return service.GetEntityTypeList(); }
		}

		public List<ListItem> SiteSelectionModes
		{
			get
			{
				return new List<ListItem>() {
                    new ListItem(SelectionMode.ShowExpectSelected.ToString(), CustomActionStrings.ShowExpectSelectedSites),
                    new ListItem(SelectionMode.HideExceptSelected.ToString(), CustomActionStrings.HideExceptSelectedSites),                    
                };
			}
		}

		public List<ListItem> ContentSelectionModes
		{
			get
			{
				return new List<ListItem>() {
                    new ListItem(SelectionMode.ShowExpectSelected.ToString(), CustomActionStrings.ShowExpectSelectedContents),
                    new ListItem(SelectionMode.HideExceptSelected.ToString(), CustomActionStrings.HideExceptSelectedContents),                    
                };
			}
		}

		public IEnumerable<ListItem> SelectedSiteListItems 
		{ 
			get
			{				
				return Data.Sites
					.Select(s => new ListItem(s.Id.ToString(), s.Name))
					.ToArray();
			}
		}

		public IEnumerable<ListItem> SelectedContentListItems
		{
			get
			{				
				return Data.Contents
					.Select(s => new ListItem(s.Id.ToString(), String.Format("{0}.{1}", s.Site.Name, s.Name)))
					.ToArray();
			}
		}
		#endregion

		#region Element IDs
		public string CustomActionEntityTypesElementId { get { return UniqueId("customActionEntityTypesCombo"); } }
		public string ContentsElementId { get { return UniqueId("contentsPicker"); } }
		public string ToolbarButtonParentActionsElementId { get { return UniqueId("toolbarButtonParentActionsCombo"); } }
		public string IsInterfaceElementId { get { return UniqueId("isInterfaceCheckbox"); } }
		public string ActionWindowPanelElementId { get { return UniqueId("actionWindowPanel"); } }
		public string PreActionPanelElementId { get { return UniqueId("preActionPanel"); } } 
		#endregion

		#region Properties
		[LocalizedDisplayName("CustomActionType", NameResourceType = typeof(CustomActionStrings))]
		[RequiredValidator(MessageTemplateResourceName = "ActionTypeNotSelected", MessageTemplateResourceType = typeof(CustomActionStrings))]
		public int? CustomActionTypeId { get; set; }

		[LocalizedDisplayName("CustomActionEntityType", NameResourceType = typeof(CustomActionStrings))]
		[RequiredValidator(MessageTemplateResourceName = "EntityTypeNotSelected", MessageTemplateResourceType = typeof(CustomActionStrings))]
		public int? CustomActionEntityTypeId { get; set; }

		[LocalizedDisplayName("SiteSelectionMode", NameResourceType = typeof(CustomActionStrings))]
		public SelectionMode SiteSelectionMode 
		{ 
			get {return Data.SiteExcluded ? SelectionMode.HideExceptSelected : SelectionMode.ShowExpectSelected; }
			set {Data.SiteExcluded = (value == SelectionMode.HideExceptSelected);} 
		}

		[LocalizedDisplayName("ContentSelectionMode", NameResourceType = typeof(CustomActionStrings))]
		public SelectionMode ContentSelectionMode
		{
			get { return Data.ContentExcluded ? SelectionMode.HideExceptSelected : SelectionMode.ShowExpectSelected; }
			set { Data.ContentExcluded = (value == SelectionMode.HideExceptSelected); }
		}

		[LocalizedDisplayName("SelectedSiteIDs", NameResourceType = typeof(CustomActionStrings))]
		public IEnumerable<int> SelectedSiteIDs { get; set; }

		[LocalizedDisplayName("SelectedContentIDs", NameResourceType = typeof(CustomActionStrings))]
		public IEnumerable<int> SelectedContentIDs { get; set; }

		[LocalizedDisplayName("ToolbarButtonParentActionId", NameResourceType = typeof(CustomActionStrings))]
		public IList<QPCheckedItem> SelectedActions { get; set; }

		public int[] SelectedActionsIds { get { return SelectedActions.Select(c => int.Parse(c.Value)).ToArray(); } }

		public string SelectedActionsString { get { return String.Join(",", SelectedActionsIds);  } }

		[LocalizedDisplayName("Order", NameResourceType = typeof(CustomActionStrings))]
		public int? Order { get; set; }
		
		#endregion		
	}
}