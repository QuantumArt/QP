using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using C = Quantumart.QP8.Constants;
using Quantumart.QP8.BLL;
using System.Web.Mvc;
using Quantumart.QP8.BLL.Helpers;

namespace Quantumart.QP8.WebMvc.ViewModels.VisualEditor
{
	public class VisualEditorStyleViewModel : EntityViewModel
	{
		public override string EntityTypeCode
		{
			get { return C.EntityTypeCode.VisualEditorStyle; }
		}

		public override string ActionCode
		{
			get { return IsNew ? C.ActionCode.AddNewVisualEditorStyle : C.ActionCode.VisualEditorStyleProperties; }
		}

		public new VisualEditorStyle Data
		{
			get
			{
				return (VisualEditorStyle)EntityData;
			}
			set
			{
				EntityData = value;
			}
		}

		public string AggregationListItems_Data_AttributeItems { get; set; }

		public string AggregationListItems_Data_StylesItems { get; set; }

		public static VisualEditorStyleViewModel Create(VisualEditorStyle style, string tabId, int parentId)
		{
			var model = EntityViewModel.Create<VisualEditorStyleViewModel>(style, tabId, parentId);
			return model;
		}		

		private List<VeStyleAggregationListItem> _JsonStyles;

		private List<VeStyleAggregationListItem> _JsonAttributes;

		internal void DoCustomBinding()
		{
			var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
			_JsonStyles = serializer.Deserialize<List<VeStyleAggregationListItem>>(AggregationListItems_Data_StylesItems);
			_JsonAttributes = serializer.Deserialize<List<VeStyleAggregationListItem>>(AggregationListItems_Data_AttributeItems);

			Data.DoCustomBinding(_JsonStyles, _JsonAttributes);
		}

		public override void Validate(ModelStateDictionary modelState)
		{
			foreach (var attr in Data.AttributeItems)
				attr.Invalid = false;
			foreach (var style in Data.StylesItems)
				style.Invalid = false;
			base.Validate(modelState);
		}
	}
}