using Quantumart.QP8.BLL.Helpers.VisualEditor;
using Quantumart.QP8.BLL.Services.VisualEditor;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace Quantumart.QP8.WebMvc.ViewModels.VisualEditor
{
    public class VisualEditorStyleViewModel : EntityViewModel
    {
        private List<VeStyleAggregationListItem> _jsonStyles;

        private List<VeStyleAggregationListItem> _jsonAttributes;

        public override string EntityTypeCode => Constants.EntityTypeCode.VisualEditorStyle;

        public override string ActionCode => IsNew ? Constants.ActionCode.AddNewVisualEditorStyle : Constants.ActionCode.VisualEditorStyleProperties;

        public new VisualEditorStyle Data
        {
            get { return (VisualEditorStyle)EntityData; }
            set { EntityData = value; }
        }

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public string AggregationListItems_Data_AttributeItems { get; set; }

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public string AggregationListItems_Data_StylesItems { get; set; }

        public static VisualEditorStyleViewModel Create(VisualEditorStyle style, string tabId, int parentId)
        {
            return Create<VisualEditorStyleViewModel>(style, tabId, parentId);
        }

        internal void DoCustomBinding()
        {
            var serializer = new JavaScriptSerializer();
            _jsonStyles = serializer.Deserialize<List<VeStyleAggregationListItem>>(AggregationListItems_Data_StylesItems);
            _jsonAttributes = serializer.Deserialize<List<VeStyleAggregationListItem>>(AggregationListItems_Data_AttributeItems);
            Data.DoCustomBinding(_jsonStyles, _jsonAttributes);
        }

        public override void Validate(ModelStateDictionary modelState)
        {
            foreach (var attr in Data.AttributeItems)
            {
                attr.Invalid = false;
            }

            foreach (var style in Data.StylesItems)
            {
                style.Invalid = false;
            }

            base.Validate(modelState);
        }
    }
}
