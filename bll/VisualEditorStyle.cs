using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Quantumart.QP8.Validators;
using Quantumart.QP8.Resources;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.Utils;
using Quantumart.QP8.Constants;
using System.Text.RegularExpressions;
using System.ComponentModel;
using Quantumart.QP8.BLL.Repository;

namespace Quantumart.QP8.BLL
{
	public class VisualEditorStyle : EntityObject
	{
		#region creation
		internal VisualEditorStyle()
		{
			
		}

		internal static VisualEditorStyle Create()
		{
			return new VisualEditorStyle
			{
				Order = VisualEditorRepository.GetStyleMaxOrder() + 1
			};
		}

		public VisualEditorStyle Init()
		{
			_AttributeItems = new InitPropertyValue<IEnumerable<VeStyleAggregationListItem>>(() => VeStyleAggregationListHelper.Deserialize(Attributes));
			_StylesItems = new InitPropertyValue<IEnumerable<VeStyleAggregationListItem>>(() => VeStyleAggregationListHelper.Deserialize(Styles));
			On = true;
			return this;
		}
		#endregion

		#region properties
		#region simple read-write

		[LocalizedDisplayName("Tag", NameResourceType = typeof(VisualEditorStrings))]
		[MaxLengthValidator(20, MessageTemplateResourceName = "TagMaxLengthExceeded", MessageTemplateResourceType = typeof(VisualEditorStrings))]
		[RequiredValidator(MessageTemplateResourceName = "TagNotEntered", MessageTemplateResourceType = typeof(VisualEditorStrings))]
		[FormatValidator(Constants.RegularExpressions.InvalidFieldName, Negated = true, MessageTemplateResourceName = "TagInvalidFormat",
			MessageTemplateResourceType = typeof(VisualEditorStrings))]
		public string Tag { get; set; }

		[LocalizedDisplayName("Order", NameResourceType = typeof(VisualEditorStrings))]
		public int Order { get; set; }

		[MaxLengthValidator(20, MessageTemplateResourceName = "OverrideTagMaxLengthExceeded", MessageTemplateResourceType = typeof(VisualEditorStrings))]		
		[FormatValidator(Constants.RegularExpressions.InvalidFieldName, Negated = true, MessageTemplateResourceName = "OverrideTagInvalidFormat",
			MessageTemplateResourceType = typeof(VisualEditorStrings))]
		[LocalizedDisplayName("OverridesTag", NameResourceType = typeof(VisualEditorStrings))]
		public string OverridesTag { get; set; }

		[LocalizedDisplayName("IsFormat", NameResourceType = typeof(VisualEditorStrings))]
		public bool IsFormat { get; set; }

		[LocalizedDisplayName("IsSystem", NameResourceType = typeof(VisualEditorStrings))]
		public bool IsSystem { get; set; }
		
		public string Attributes { get; set; }
		
		public string Styles { get; set; }
		
		public bool On { get; set; }

		#endregion
		#region simple read-only
		public override string EntityTypeCode
		{
			get
			{
				return Constants.EntityTypeCode.VisualEditorStyle;
			}
		}

		public override int ParentEntityId
		{
			get
			{
				return 1;				
			}
		}
		#endregion
		private InitPropertyValue<IEnumerable<VeStyleAggregationListItem>> _StylesItems;

		[LocalizedDisplayName("Styles", NameResourceType = typeof(VisualEditorStrings))]
		public IEnumerable<VeStyleAggregationListItem> StylesItems
		{
			get { return _StylesItems.Value; }
			set { _StylesItems.Value = value; }
		}

		private InitPropertyValue<IEnumerable<VeStyleAggregationListItem>> _AttributeItems;

		[LocalizedDisplayName("Attributes", NameResourceType = typeof(VisualEditorStrings))]
		public IEnumerable<VeStyleAggregationListItem> AttributeItems
		{
			get {return _AttributeItems.Value;}
			set { _AttributeItems.Value = value; }
		}

		#endregion

		public void DoCustomBinding(List<VeStyleAggregationListItem> _JsonStyles, List<VeStyleAggregationListItem> _JsonAttributes)
		{
			foreach (var attr in _JsonAttributes)
				attr.Invalid = false;
			foreach (var style in _JsonStyles)
				style.Invalid = false;
			StylesItems = _JsonStyles;
			AttributeItems = _JsonAttributes;
			Styles = VeStyleAggregationListHelper.Serialize(_JsonStyles);
			Attributes = VeStyleAggregationListHelper.Serialize(_JsonAttributes);
		}

		public override void Validate()
		{			
			RulesException<VisualEditorStyle> errors = new RulesException<VisualEditorStyle>();
			base.Validate(errors);

			var duplicateStyleNames = StylesItems.GroupBy(s => s.Name).Where(g => g.Count() > 1).Select(x => x.Key).ToArray();
			var duplicateAttributeNames = AttributeItems.GroupBy(a => a.Name).Where(g => g.Count() > 1).Select(x => x.Key).ToArray();

			var stylesArray = StylesItems.ToArray();
			for (int i = 0; i < stylesArray.Length; i++)
				ValidateVeStyleAggregationListItem(stylesArray[i], errors, i + 1, duplicateStyleNames,
					VisualEditorStrings.Style, VisualEditorStrings.StylesMessage);
			
			var attributesArray = AttributeItems.ToArray();
			for (int i = 0; i < attributesArray.Length; i++)
				ValidateVeStyleAggregationListItem(attributesArray[i], errors, i + 1, duplicateAttributeNames,
					VisualEditorStrings.Attribute, VisualEditorStrings.AttributesMessage);
			
			if (!errors.IsEmpty)
				throw errors;
		}

		private void ValidateVeStyleAggregationListItem(VeStyleAggregationListItem item, RulesException<VisualEditorStyle> errors, int index,
			string[] duplicateNames, string typeName, string typesName)
		{
			if (!string.IsNullOrWhiteSpace(item.Name) && duplicateNames.Contains(item.Name))
			{
				errors.ErrorForModel(String.Format(VisualEditorStrings.VeStyleAggrListItemNameDuplicate, typeName, index, typesName));
				item.Invalid = true;
			}

			if (string.IsNullOrWhiteSpace(item.Name))
			{
				errors.ErrorForModel(String.Format(VisualEditorStrings.VeStyleAggrListItemNameRequired, typeName, index));
				item.Invalid = true;
			}

			if (string.IsNullOrWhiteSpace(item.ItemValue))
			{
				errors.ErrorForModel(String.Format(VisualEditorStrings.VeStyleAggrListItemValueRequired, typeName, index));
				item.Invalid = true;
			}

			if (!string.IsNullOrWhiteSpace(item.Name) && Regex.IsMatch(item.Name, RegularExpressions.InvalidEntityName))
			{
				errors.ErrorForModel(String.Format(VisualEditorStrings.VeStyleAggrListItemNameInvalidFormat, typeName, index));
				item.Invalid = true;
			}

			if (item.Name.Length > 255)
			{
				errors.ErrorForModel(String.Format(VisualEditorStrings.VeStyleAggrListItemNameMaxLengthExceeded, typeName, index));
				item.Invalid = true;
			}

			if (item.ItemValue.Length > 255)
			{
				errors.ErrorForModel(String.Format(VisualEditorStrings.VeStyleAggrListItemValueMaxLengthExceeded, typeName, index));
				item.Invalid = true;
			}			
		}
	}
}
