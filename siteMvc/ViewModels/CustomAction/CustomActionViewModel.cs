using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Validators;
using Quantumart.QP8.WebMvc.Extensions.Helpers;
using Quantumart.QP8.WebMvc.ViewModels.Abstract;

namespace Quantumart.QP8.WebMvc.ViewModels.CustomAction
{
    public class CustomActionViewModel : EntityViewModel
    {
        private ICustomActionService _service;

        public new BLL.CustomAction Data
        {
            get
            {
                return (BLL.CustomAction)EntityData;
            }
            set
            {
                EntityData = value;
            }
        }

        public static CustomActionViewModel Create(BLL.CustomAction customAction, string tabId, int parentId, ICustomActionService service)
        {
            var model = Create<CustomActionViewModel>(customAction, tabId, parentId);
            model._service = service;
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
            SelectedActions = Data.ParentActions?.Select(c => new QPCheckedItem { Value = c.ToString() }).ToList() ?? Enumerable.Empty<QPCheckedItem>().ToList();
        }

        /// <summary>
        /// Устанавливает свойства модели, которые не могут быть установлены автоматически
        /// </summary>
        internal void DoCustomBinding()
        {
            Data.Action.TypeId = CustomActionTypeId ?? 0;
            Data.Action.EntityTypeId = CustomActionEntityTypeId ?? 0;
            Data.Order = Order ?? 0;
            Data.Sites = _service.GetSites(SelectedSiteIDs);
            Data.Contents = _service.GetContents(SelectedContentIDs);
        }


        public override void Validate(ModelStateDictionary modelState)
        {
            base.Validate(modelState);
            if (Data.Action.EntityTypeId > 0 && Data.ShowInToolbar && !SelectedActions.Any())
            {
                modelState.AddModelError("SelectedActions", CustomActionStrings.ToolbarButtonParentActionNotSelected);
            }
        }

        public override string EntityTypeCode => Constants.EntityTypeCode.CustomAction;

        public override string ActionCode => IsNew ? Constants.ActionCode.AddNewCustomAction : Constants.ActionCode.CustomActionsProperties;

        public IEnumerable<BLL.ListItem> ActionTypeList => _service.GetActionTypeList();

        public IEnumerable<BLL.ListItem> EntityTypeList => _service.GetEntityTypeList();

        public List<BLL.ListItem> SiteSelectionModes => new List<BLL.ListItem>
        {
            new BLL.ListItem(SelectionMode.ShowExpectSelected.ToString(), CustomActionStrings.ShowExpectSelectedSites),
            new BLL.ListItem(SelectionMode.HideExceptSelected.ToString(), CustomActionStrings.HideExceptSelectedSites)
        };

        public List<BLL.ListItem> ContentSelectionModes => new List<BLL.ListItem>
        {
            new BLL.ListItem(SelectionMode.ShowExpectSelected.ToString(), CustomActionStrings.ShowExpectSelectedContents),
            new BLL.ListItem(SelectionMode.HideExceptSelected.ToString(), CustomActionStrings.HideExceptSelectedContents)
        };

        public IEnumerable<BLL.ListItem> SelectedSiteListItems
        {
            get
            {
                return Data.Sites
                    .Select(s => new BLL.ListItem(s.Id.ToString(), s.Name))
                    .ToArray();
            }
        }

        public IEnumerable<BLL.ListItem> SelectedContentListItems
        {
            get
            {
                return Data.Contents
                    .Select(s => new BLL.ListItem(s.Id.ToString(), $"{s.Site.Name}.{s.Name}"))
                    .ToArray();
            }
        }

        public string CustomActionEntityTypesElementId => UniqueId("customActionEntityTypesCombo");

        public string ContentsElementId => UniqueId("contentsPicker");

        public string ToolbarButtonParentActionsElementId => UniqueId("toolbarButtonParentActionsCombo");

        public string IsInterfaceElementId => UniqueId("isInterfaceCheckbox");

        public string ActionWindowPanelElementId => UniqueId("actionWindowPanel");

        public string PreActionPanelElementId => UniqueId("preActionPanel");

        [LocalizedDisplayName("CustomActionType", NameResourceType = typeof(CustomActionStrings))]
        [RequiredValidator(MessageTemplateResourceName = "ActionTypeNotSelected", MessageTemplateResourceType = typeof(CustomActionStrings))]
        public int? CustomActionTypeId { get; set; }

        [LocalizedDisplayName("CustomActionEntityType", NameResourceType = typeof(CustomActionStrings))]
        [RequiredValidator(MessageTemplateResourceName = "EntityTypeNotSelected", MessageTemplateResourceType = typeof(CustomActionStrings))]
        public int? CustomActionEntityTypeId { get; set; }

        [LocalizedDisplayName("SiteSelectionMode", NameResourceType = typeof(CustomActionStrings))]
        public SelectionMode SiteSelectionMode
        {
            get { return Data.SiteExcluded ? SelectionMode.HideExceptSelected : SelectionMode.ShowExpectSelected; }
            set { Data.SiteExcluded = value == SelectionMode.HideExceptSelected; }
        }

        [LocalizedDisplayName("ContentSelectionMode", NameResourceType = typeof(CustomActionStrings))]
        public SelectionMode ContentSelectionMode
        {
            get { return Data.ContentExcluded ? SelectionMode.HideExceptSelected : SelectionMode.ShowExpectSelected; }
            set { Data.ContentExcluded = value == SelectionMode.HideExceptSelected; }
        }

        [LocalizedDisplayName("SelectedSiteIDs", NameResourceType = typeof(CustomActionStrings))]
        public IEnumerable<int> SelectedSiteIDs { get; set; }

        [LocalizedDisplayName("SelectedContentIDs", NameResourceType = typeof(CustomActionStrings))]
        public IEnumerable<int> SelectedContentIDs { get; set; }

        [LocalizedDisplayName("ToolbarButtonParentActionId", NameResourceType = typeof(CustomActionStrings))]
        public IList<QPCheckedItem> SelectedActions { get; set; }

        public int[] SelectedActionsIds { get { return SelectedActions.Select(c => int.Parse(c.Value)).ToArray(); } }

        public string SelectedActionsString => string.Join(",", SelectedActionsIds);

        [LocalizedDisplayName("Order", NameResourceType = typeof(CustomActionStrings))]
        public int? Order { get; set; }
    }
}
