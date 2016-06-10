using Quantumart.QP8.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using C = Quantumart.QP8.Constants;

namespace Quantumart.QP8.BLL.ListItems
{
    public class FieldListItem
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string ContentName { get; set; }

        public string FieldName { get; set; }

        public string FriendlyName { get; set; }

        public string Description { get; set; }

        public string Created { get; set; }

        public string Modified { get; set; }

        public string LastModifiedByUser { get; set; }

        public string TypeName { get; set; }

        public string Size { get; set; }

        public bool Required { get; set; }

        public bool Indexed { get; set; }

        public bool ViewInList { get; set; }

        public bool MapAsProperty { get; set; }

        public string TypeIcon { get; set; }

        public int TypeCode { get; set; }

        public int? LinkId { get; set; }

        public string FieldTypeName
        {
            get
            {
                Func<int, string> getValue = (id => Translator.Translate(FieldType.AllFieldTypes.Single(f => f.Id == id).Name));
                IDictionary<int, string> tn = new Dictionary<int, string>()
                {
                    {C.FieldTypeCodes.Boolean, getValue(C.FieldTypeCodes.Boolean)},
                    {C.FieldTypeCodes.Date, getValue(C.FieldTypeCodes.Date)},
                    {C.FieldTypeCodes.DateTime, getValue(C.FieldTypeCodes.DateTime)},
                    {C.FieldTypeCodes.DynamicImage, getValue(C.FieldTypeCodes.DynamicImage)},
                    {C.FieldTypeCodes.File, getValue(C.FieldTypeCodes.File)},
                    {C.FieldTypeCodes.Image, getValue(C.FieldTypeCodes.Image)},
                    {C.FieldTypeCodes.Numeric, getValue(C.FieldTypeCodes.Numeric)},
                    {C.FieldTypeCodes.M2ORelation, getValue(C.FieldTypeCodes.M2ORelation)},
                    {C.FieldTypeCodes.String, getValue(C.FieldTypeCodes.String)},
                    {C.FieldTypeCodes.Textbox, getValue(C.FieldTypeCodes.Textbox)},
                    {C.FieldTypeCodes.Time, getValue(C.FieldTypeCodes.Time)},
                    {C.FieldTypeCodes.VisualEdit, getValue(C.FieldTypeCodes.VisualEdit)}
                };
                if (tn.ContainsKey(TypeCode))
                {
                    return tn[TypeCode];
                }

                if (TypeCode == C.FieldTypeCodes.Relation)
                {
                    return LinkId.HasValue ? FieldStrings.M2MRelation : FieldStrings.O2MRelation;
                }

                return string.Empty;
            }
        }
    }
}
