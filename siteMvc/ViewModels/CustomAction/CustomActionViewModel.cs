using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Utils.Binders;
using Quantumart.QP8.WebMvc.Extensions.Helpers;
using Quantumart.QP8.WebMvc.ViewModels.Abstract;

namespace Quantumart.QP8.WebMvc.ViewModels.CustomAction
{
    public class CustomActionViewModel : EntityViewModel
    {
        private ICustomActionService _service;

        public BLL.CustomAction Data
        {
            get => (BLL.CustomAction)EntityData;
            set => EntityData = value;
        }

        public static CustomActionViewModel Create(BLL.CustomAction customAction, string tabId, int parentId, ICustomActionService service)
        {
            var model = Create<CustomActionViewModel>(customAction, tabId, parentId);
            model._service = service;
            model.Init();
            return model;
        }

        private void Init()
        {
            if (!IsNew)
            {
                CustomActionTypeId = Data.Action.TypeId;
                CustomActionEntityTypeId = Data.Action.EntityTypeId;
                Order = Data.Order;
            }

            SelectedSiteIDs = Data.SiteIds.ToArray();
            SelectedContentIDs = Data.ContentIds.ToArray();
            SelectedActions = Data.ParentActions?.Select(c => new QPCheckedItem { Value = c.ToString() }).ToList() ?? Enumerable.Empty<QPCheckedItem>().ToList();
        }

        public override void DoCustomBinding()
        {
            base.DoCustomBinding();

            Data.Action.TypeId = CustomActionTypeId ?? 0;
            Data.Action.EntityTypeId = CustomActionEntityTypeId ?? 0;
            Data.Order = Order ?? 0;
            Data.SiteIds = SelectedSiteIDs;
            Data.ContentIds = SelectedContentIDs;
        }

        public override IEnumerable<ValidationResult> ValidateViewModel()
        {
            if (Data.Action.EntityTypeId > 0 && Data.ShowInToolbar && !SelectedActions.Any())
            {
                yield return new ValidationResult(CustomActionStrings.ToolbarButtonParentActionNotSelected, new[] {"SelectedActions"});
            }
        }

        public override string EntityTypeCode => Constants.EntityTypeCode.CustomAction;

        public override string ActionCode => IsNew ? Constants.ActionCode.AddNewCustomAction : Constants.ActionCode.CustomActionsProperties;

        public IEnumerable<ListItem> ActionTypeList => _service.GetActionTypeList();

        public IEnumerable<ListItem> EntityTypeList => _service.GetEntityTypeList();

        public List<ListItem> SiteSelectionModes => new List<ListItem>
        {
            new ListItem(SelectionMode.ShowExpectSelected.ToString(), CustomActionStrings.ShowExpectSelectedSites),
            new ListItem(SelectionMode.HideExceptSelected.ToString(), CustomActionStrings.HideExceptSelectedSites)
        };

        public List<ListItem> ContentSelectionModes => new List<ListItem>
        {
            new ListItem(SelectionMode.ShowExpectSelected.ToString(), CustomActionStrings.ShowExpectSelectedContents),
            new ListItem(SelectionMode.HideExceptSelected.ToString(), CustomActionStrings.HideExceptSelectedContents)
        };

        public IEnumerable<ListItem> SelectedSiteListItems
        {
            get
            {
                return _service.GetSites(Data)
                    .Select(s => new ListItem(s.Id.ToString(), s.Name))
                    .ToArray();
            }
        }

        public IEnumerable<ListItem> SelectedContentListItems
        {
            get
            {
                return _service.GetContents(Data)
                    .Select(s => new ListItem(s.Id.ToString(), $"{s.Site.Name}.{s.Name}"))
                    .ToArray();
            }
        }

        public string CustomActionEntityTypesElementId => UniqueId("customActionEntityTypesCombo");

        public string ContentsElementId => UniqueId("contentsPicker");

        public string ToolbarButtonParentActionsElementId => UniqueId("toolbarButtonParentActionsCombo");

        public string IsInterfaceElementId => UniqueId("isInterfaceCheckbox");

        public string ActionWindowPanelElementId => UniqueId("actionWindowPanel");

        public string PreActionPanelElementId => UniqueId("preActionPanel");

        [Display(Name = "CustomActionType", ResourceType = typeof(CustomActionStrings))]
        [Required(ErrorMessageResourceName = "ActionTypeNotSelected", ErrorMessageResourceType = typeof(CustomActionStrings))]
        public int? CustomActionTypeId { get; set; }

        [Display(Name = "CustomActionEntityType", ResourceType = typeof(CustomActionStrings))]
        [Required(ErrorMessageResourceName = "EntityTypeNotSelected", ErrorMessageResourceType = typeof(CustomActionStrings))]
        public int? CustomActionEntityTypeId { get; set; }

        [Display(Name = "SiteSelectionMode", ResourceType = typeof(CustomActionStrings))]
        public SelectionMode SiteSelectionMode
        {
            get => Data.SiteExcluded ? SelectionMode.HideExceptSelected : SelectionMode.ShowExpectSelected;
            set => Data.SiteExcluded = value == SelectionMode.HideExceptSelected;
        }

        [Display(Name = "ContentSelectionMode", ResourceType = typeof(CustomActionStrings))]
        public SelectionMode ContentSelectionMode
        {
            get => Data.ContentExcluded ? SelectionMode.HideExceptSelected : SelectionMode.ShowExpectSelected;
            set => Data.ContentExcluded = value == SelectionMode.HideExceptSelected;
        }

        [Display(Name = "SelectedSiteIDs", ResourceType = typeof(CustomActionStrings))]
        [ModelBinder(BinderType = typeof(IdArrayBinder))]
        public IEnumerable<int> SelectedSiteIDs { get; set; }

        [Display(Name = "SelectedContentIDs", ResourceType = typeof(CustomActionStrings))]
        [ModelBinder(BinderType = typeof(IdArrayBinder))]
        public IEnumerable<int> SelectedContentIDs { get; set; }

        [Display(Name = "ToolbarButtonParentActionId", ResourceType = typeof(CustomActionStrings))]
        public IList<QPCheckedItem> SelectedActions { get; set; }

        public int[] SelectedActionsIds
        {
            get { return SelectedActions.Select(c => int.Parse(c.Value)).ToArray(); }
        }

        public string SelectedActionsString => string.Join(",", SelectedActionsIds);

        [Display(Name = "Order", ResourceType = typeof(CustomActionStrings))]
        public int? Order { get; set; }
    }
}
