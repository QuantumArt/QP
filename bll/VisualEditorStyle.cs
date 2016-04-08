using Quantumart.QP8.BLL.Helpers.VisualEditor;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Utils;
using Quantumart.QP8.Validators;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Quantumart.QP8.BLL
{
    public class VisualEditorStyle : EntityObject
    {
        internal VisualEditorStyle()
        { }

        internal static VisualEditorStyle Create()
        {
            return new VisualEditorStyle
            {
                Order = VisualEditorRepository.GetStyleMaxOrder() + 1
            };
        }

        public VisualEditorStyle Init()
        {
            _attributeItems = new InitPropertyValue<IEnumerable<VeStyleAggregationListItem>>(() => VeStyleAggregationListHelper.Deserialize(Attributes));
            _stylesItems = new InitPropertyValue<IEnumerable<VeStyleAggregationListItem>>(() => VeStyleAggregationListHelper.Deserialize(Styles));
            On = true;
            return this;
        }

        [LocalizedDisplayName("Tag", NameResourceType = typeof(VisualEditorStrings))]
        [MaxLengthValidator(20, MessageTemplateResourceName = "TagMaxLengthExceeded", MessageTemplateResourceType = typeof(VisualEditorStrings))]
        [RequiredValidator(MessageTemplateResourceName = "TagNotEntered", MessageTemplateResourceType = typeof(VisualEditorStrings))]
        [FormatValidator(RegularExpressions.InvalidFieldName, Negated = true, MessageTemplateResourceName = "TagInvalidFormat", MessageTemplateResourceType = typeof(VisualEditorStrings))]
        public string Tag { get; set; }

        [LocalizedDisplayName("Order", NameResourceType = typeof(VisualEditorStrings))]
        public int Order { get; set; }

        [MaxLengthValidator(20, MessageTemplateResourceName = "OverrideTagMaxLengthExceeded", MessageTemplateResourceType = typeof(VisualEditorStrings))]
        [FormatValidator(RegularExpressions.InvalidFieldName, Negated = true, MessageTemplateResourceName = "OverrideTagInvalidFormat", MessageTemplateResourceType = typeof(VisualEditorStrings))]
        [LocalizedDisplayName("OverridesTag", NameResourceType = typeof(VisualEditorStrings))]
        public string OverridesTag { get; set; }

        [LocalizedDisplayName("IsFormat", NameResourceType = typeof(VisualEditorStrings))]
        public bool IsFormat { get; set; }

        [LocalizedDisplayName("IsSystem", NameResourceType = typeof(VisualEditorStrings))]
        public bool IsSystem { get; set; }

        public string Attributes { get; set; }

        public string Styles { get; set; }

        public bool On { get; set; }

        public override string EntityTypeCode => Constants.EntityTypeCode.VisualEditorStyle;

        public override int ParentEntityId => 1;

        private InitPropertyValue<IEnumerable<VeStyleAggregationListItem>> _stylesItems;

        [LocalizedDisplayName("Styles", NameResourceType = typeof(VisualEditorStrings))]
        public IEnumerable<VeStyleAggregationListItem> StylesItems
        {
            get { return _stylesItems.Value; }
            set { _stylesItems.Value = value; }
        }

        private InitPropertyValue<IEnumerable<VeStyleAggregationListItem>> _attributeItems;

        [LocalizedDisplayName("Attributes", NameResourceType = typeof(VisualEditorStrings))]
        public IEnumerable<VeStyleAggregationListItem> AttributeItems
        {
            get { return _attributeItems.Value; }
            set { _attributeItems.Value = value; }
        }

        public void DoCustomBinding(List<VeStyleAggregationListItem> jsonStyles, List<VeStyleAggregationListItem> jsonAttributes)
        {
            foreach (var attr in jsonAttributes)
            {
                attr.Invalid = false;
            }

            foreach (var style in jsonStyles)
            {
                style.Invalid = false;
            }

            StylesItems = jsonStyles;
            AttributeItems = jsonAttributes;
            Styles = VeStyleAggregationListHelper.Serialize(jsonStyles);
            Attributes = VeStyleAggregationListHelper.Serialize(jsonAttributes);
        }

        public override void Validate()
        {
            var errors = new RulesException<VisualEditorStyle>();
            base.Validate(errors);

            var duplicateStyleNames = StylesItems.GroupBy(s => s.Name).Where(g => g.Count() > 1).Select(x => x.Key).ToArray();
            var duplicateAttributeNames = AttributeItems.GroupBy(a => a.Name).Where(g => g.Count() > 1).Select(x => x.Key).ToArray();

            var stylesArray = StylesItems.ToArray();
            for (var i = 0; i < stylesArray.Length; i++)
            {
                ValidateVeStyleAggregationListItem(stylesArray[i], errors, i + 1, duplicateStyleNames,
                    VisualEditorStrings.Style, VisualEditorStrings.StylesMessage);
            }

            var attributesArray = AttributeItems.ToArray();
            for (var i = 0; i < attributesArray.Length; i++)
            {
                ValidateVeStyleAggregationListItem(attributesArray[i], errors, i + 1, duplicateAttributeNames,
                    VisualEditorStrings.Attribute, VisualEditorStrings.AttributesMessage);
            }

            if (!errors.IsEmpty)
            {
                throw errors;
            }
        }

        private static void ValidateVeStyleAggregationListItem(VeStyleAggregationListItem item, RulesException errors, int index,
            IEnumerable<string> duplicateNames, string typeName, string typesName)
        {
            if (!string.IsNullOrWhiteSpace(item.Name) && duplicateNames.Contains(item.Name))
            {
                errors.ErrorForModel(string.Format(VisualEditorStrings.VeStyleAggrListItemNameDuplicate, typeName, index, typesName));
                item.Invalid = true;
            }

            if (string.IsNullOrWhiteSpace(item.Name))
            {
                errors.ErrorForModel(string.Format(VisualEditorStrings.VeStyleAggrListItemNameRequired, typeName, index));
                item.Invalid = true;
            }

            if (string.IsNullOrWhiteSpace(item.ItemValue))
            {
                errors.ErrorForModel(string.Format(VisualEditorStrings.VeStyleAggrListItemValueRequired, typeName, index));
                item.Invalid = true;
            }

            if (!string.IsNullOrWhiteSpace(item.Name) && Regex.IsMatch(item.Name, RegularExpressions.InvalidEntityName))
            {
                errors.ErrorForModel(string.Format(VisualEditorStrings.VeStyleAggrListItemNameInvalidFormat, typeName, index));
                item.Invalid = true;
            }

            if (item.Name.Length > 255)
            {
                errors.ErrorForModel(string.Format(VisualEditorStrings.VeStyleAggrListItemNameMaxLengthExceeded, typeName, index));
                item.Invalid = true;
            }

            if (item.ItemValue.Length > 255)
            {
                errors.ErrorForModel(string.Format(VisualEditorStrings.VeStyleAggrListItemValueMaxLengthExceeded, typeName, index));
                item.Invalid = true;
            }
        }
    }
}
