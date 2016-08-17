using System.Diagnostics.CodeAnalysis;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.BLL
{
	public class FieldTypeInfo
	{
		public FieldExactTypes ExactType { get; set; }
		public int? RelationId { get; set; }

		public FieldTypeInfo(FieldExactTypes fti, int? relationId)
		{
			ExactType = fti;
			RelationId = relationId;
		}

	    [SuppressMessage("ReSharper", "PossibleInvalidOperationException")]
	    public string FormatFieldValue(string value)
		{
			switch (ExactType)
			{
				case FieldExactTypes.M2ORelation:
				case FieldExactTypes.M2MRelation:
					return RelationId.Value.ToString();
				case FieldExactTypes.Numeric:
					return Converter.ToDbNumericString(value);
				case FieldExactTypes.Date:
				case FieldExactTypes.DateTime:
				case FieldExactTypes.Time:
					return Converter.ToDbDateTimeString(value);
				default:
					return value;
			}
		}
	}
}
