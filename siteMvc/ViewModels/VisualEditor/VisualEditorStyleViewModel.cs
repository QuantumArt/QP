using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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

        [Required]
        public VisualEditorStyle Data
        {
            get => (VisualEditorStyle)EntityData;
            set => EntityData = value;
        }

        public string AggregationListItemsDataAttributeItems { get; set; }

        public string AggregationListItemsDataStylesItems { get; set; }

        public static VisualEditorStyleViewModel Create(VisualEditorStyle style, string tabId, int parentId) => Create<VisualEditorStyleViewModel>(style, tabId, parentId);

        public override void DoCustomBinding()
        {
            _jsonStyles = JsonConvert.DeserializeObject<List<VeStyleAggregationListItem>>(AggregationListItemsDataStylesItems);
            _jsonAttributes = JsonConvert.DeserializeObject<List<VeStyleAggregationListItem>>(AggregationListItemsDataAttributeItems);
            Data.DoCustomBinding(_jsonStyles, _jsonAttributes);
        }

        public override void Validate()
        {
            foreach (var attr in Data.AttributeItems)
            {
                attr.Invalid = false;
            }

            foreach (var style in Data.StylesItems)
            {
                style.Invalid = false;
            }
            base.Validate();
        }
    }
}
