using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using Quantumart.QP8.BLL.Helpers.VisualEditor;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.BLL.Services.VisualEditor
{
    public class VisualEditorStyle : EntityObject
    {
        private const string Separator = ";";
        private const string ItemSeparator = ":";

        internal VisualEditorStyle()
        {
        }

        internal static VisualEditorStyle Create() => new VisualEditorStyle
        {
            Order = VisualEditorRepository.GetStyleMaxOrder() + 1
        };

        public VisualEditorStyle Init()
        {
            _attributeItems = new InitPropertyValue<IEnumerable<VeStyleAggregationListItem>>(() => Deserialize(Attributes));
            _stylesItems = new InitPropertyValue<IEnumerable<VeStyleAggregationListItem>>(() => Deserialize(Styles));
            On = true;
            return this;
        }

        [Display(Name = "Tag", ResourceType = typeof(VisualEditorStrings))]
        [StringLength(20, ErrorMessageResourceName = "TagMaxLengthExceeded", ErrorMessageResourceType = typeof(VisualEditorStrings))]
        [Required(ErrorMessageResourceName = "TagNotEntered", ErrorMessageResourceType = typeof(VisualEditorStrings))]
        [RegularExpression(RegularExpressions.FieldName, ErrorMessageResourceName = "TagInvalidFormat", ErrorMessageResourceType = typeof(VisualEditorStrings))]
        public string Tag { get; set; }

        [Display(Name = "Order", ResourceType = typeof(VisualEditorStrings))]
        public int Order { get; set; }

        [StringLength(20, ErrorMessageResourceName = "OverrideTagMaxLengthExceeded", ErrorMessageResourceType = typeof(VisualEditorStrings))]
        [RegularExpression(RegularExpressions.FieldName, ErrorMessageResourceName = "OverrideTagInvalidFormat", ErrorMessageResourceType = typeof(VisualEditorStrings))]
        [Display(Name = "OverridesTag", ResourceType = typeof(VisualEditorStrings))]
        public string OverridesTag { get; set; }

        [Display(Name = "IsFormat", ResourceType = typeof(VisualEditorStrings))]
        public bool IsFormat { get; set; }

        [Display(Name = "IsSystem", ResourceType = typeof(VisualEditorStrings))]
        public bool IsSystem { get; set; }

        public string Attributes { get; set; }

        public string Styles { get; set; }

        public bool On { get; set; }

        public override string EntityTypeCode => Constants.EntityTypeCode.VisualEditorStyle;

        public override int ParentEntityId => 1;

        private InitPropertyValue<IEnumerable<VeStyleAggregationListItem>> _stylesItems;

        [Display(Name = "Styles", ResourceType = typeof(VisualEditorStrings))]
        public IEnumerable<VeStyleAggregationListItem> StylesItems
        {
            get => _stylesItems.Value;
            set => _stylesItems.Value = value;
        }

        private InitPropertyValue<IEnumerable<VeStyleAggregationListItem>> _attributeItems;

        [Display(Name = "Attributes", ResourceType = typeof(VisualEditorStrings))]
        public IEnumerable<VeStyleAggregationListItem> AttributeItems
        {
            get => _attributeItems.Value;
            set => _attributeItems.Value = value;
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
            Styles = Serialize(jsonStyles);
            Attributes = Serialize(jsonAttributes);
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

            if (!string.IsNullOrWhiteSpace(item.Name) && !Regex.IsMatch(item.Name, RegularExpressions.EntityName))
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

        private static string Serialize(IEnumerable<VeStyleAggregationListItem> items)
        {
            return string.Join(Separator, items.Select(x => $"{x.Name}:{x.ItemValue}"));
        }

        private static IEnumerable<VeStyleAggregationListItem> Deserialize(string str)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return Enumerable.Empty<VeStyleAggregationListItem>();
            }

            return str.Split(Separator.ToCharArray()).Select(x =>
            {
                var strings = x.Split(ItemSeparator.ToCharArray());
                return new VeStyleAggregationListItem { Name = strings[0], ItemValue = strings[1] };
            });
        }
    }
}
