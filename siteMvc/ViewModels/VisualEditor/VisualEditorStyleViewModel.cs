using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using Quantumart.QP8.BLL.Helpers.VisualEditor;
using Quantumart.QP8.BLL.Services.VisualEditor;
using Quantumart.QP8.WebMvc.ViewModels.Abstract;

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
            get => (VisualEditorStyle)EntityData;
            set => EntityData = value;
        }

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public string AggregationListItemsDataAttributeItems { get; set; }

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public string AggregationListItemsDataStylesItems { get; set; }

        public static VisualEditorStyleViewModel Create(VisualEditorStyle style, string tabId, int parentId) => Create<VisualEditorStyleViewModel>(style, tabId, parentId);

        internal void DoCustomBinding()
        {
            _jsonStyles = JsonConvert.DeserializeObject<List<VeStyleAggregationListItem>>(AggregationListItemsDataStylesItems);
            _jsonAttributes = JsonConvert.DeserializeObject<List<VeStyleAggregationListItem>>(AggregationListItemsDataAttributeItems);
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
