using System.Collections.Generic;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL.Services.DTO;

namespace Quantumart.QP8.WebMvc.ViewModels.Field
{
    public class FieldSelectableListViewModel : ListViewModel
    {
        public List<FieldListItem> Data { get; set; }

        public string ParentName { get; set; }

        public string GroupName { get; set; }

        public bool IsMultiple { get; set; }

        public FieldSelectableListViewModel(FieldInitListResult result, string tabId, int parentId, int[] ids, string actionCode)
        {
            ParentEntityId = parentId;
            TabId = tabId;
            ParentName = result.ParentName;
            SelectedIDs = ids;
            AutoGenerateLink = false;
            ShowAddNewItemButton = !IsWindow;
            _actionCode = actionCode;
        }

        public sealed override string EntityTypeCode => Constants.EntityTypeCode.Field;

        public override bool AllowMultipleEntitySelection
        {
            get
            {
                return IsMultiple;
            }
            set
            {
            }
        }

        private readonly string _actionCode;

        public override string ActionCode => _actionCode;

        public virtual string GetDataAction
        {
            get
            {
                if (_actionCode == Constants.ActionCode.MultipleSelectFieldForExport)
                {
                    return "_MultipleSelectForExport";
                }

                if (_actionCode == Constants.ActionCode.MultipleSelectFieldForExportExpanded)
                {
                    return "_MultipleSelectForExportExpanded";
                }

                return string.Empty;
            }
        }
    }
}
