using System.Collections.Generic;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Resources;
using Quantumart.QP8.WebMvc.ViewModels.Abstract;

namespace Quantumart.QP8.WebMvc.ViewModels.Field
{
    public class FieldListViewModel : ListViewModel
    {
        public List<FieldListItem> Data { get; set; }

        public string ParentName { get; set; }

        public static FieldListViewModel Create(FieldInitListResult result, string tabId, int parentId)
        {
            var model = Create<FieldListViewModel>(tabId, parentId);
            model.IsVirtual = result.IsVirtual;
            model.ShowAddNewItemButton = result.IsAddNewAccessable && !model.IsWindow;
            return model;
        }

        public override string EntityTypeCode => IsVirtual ? Constants.EntityTypeCode.VirtualField : Constants.EntityTypeCode.Field;

        public override string ActionCode => !IsVirtual ? Constants.ActionCode.Fields : Constants.ActionCode.VirtualFields;

        public override string ContextMenuCode => IsVirtual ? Constants.EntityTypeCode.VirtualField : Constants.EntityTypeCode.Field;

        public override string AddNewItemText => ContentStrings.Link_AddNewField;

        public override string AddNewItemActionCode => Constants.ActionCode.AddNewField;
    }
}

