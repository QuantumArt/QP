using System.Collections.Generic;
using System.IO;
using Quantumart.QP8.BLL.Exceptions;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.BLL.Helpers
{
	public class MutateHelper
	{

		public static List<FieldValue> MutateFieldValues(List<FieldValue> fieldValues, int mutationStep)
		{
			var result = new List<FieldValue>();
			foreach (var fieldValue in fieldValues)
			{
				if (fieldValue.Field.ExactType == FieldExactTypes.O2MRelation && fieldValue.Field.Aggregated)
				{
				    continue;
				}
			    if (fieldValue.Field.ExactType == FieldExactTypes.M2ORelation && fieldValue.Field.BackRelation != null && fieldValue.Field.BackRelation.Aggregated)
			    {
			        continue;
			    }

			    string value;
				if (fieldValue.Field.Type.Name == FieldTypeName.String)
				{
				    value = MutateString(fieldValue.Value, mutationStep);
				}
				else if (fieldValue.Field.Type.Name == FieldTypeName.Numeric)
				{
				    value = MutateInt(Converter.ToInt32(fieldValue.Value, 0), mutationStep).ToString();
				}
				else
				{
				    throw new UnsupportedConstraintException();
				}

			    result.Add(new FieldValue { Article = fieldValue.Article, Field = fieldValue.Field, Value = value });
			}
			return result;
		}

		public static string MutateString(string value, int mutationStep) => string.Format("{0} {1}", value, mutationStep);

	    public static string MutateNetName(string value, int mutationStep) => string.Format("{0}{1}", value, mutationStep);

	    public static string MutateUserLogin(string value, int mutationStep) => string.Format("{0}{1}", value, mutationStep);

	    public static int MutateInt(int value, int mutationStep) => value + mutationStep;

	    public static string MutateTitle(string title, int mutationStep)
	    {
	        if (mutationStep == 1)
			{
			    return string.Format("Copy of {0}", title);
			}

	        return string.Format("Copy of {0} {1}", title, mutationStep);
	    }

		public static string MutateFileName(string fileName, int mutationStep)
		{
			var shortName = Path.GetFileNameWithoutExtension(fileName);
			var extension = Path.GetExtension(fileName);
			return string.Format("{0}[{1}]{2}", shortName, mutationStep, extension);
		}

		public static string MutatePageFileName(string fileName, int mutationStep)
		{
			var shortName = Path.GetFileNameWithoutExtension(fileName);
			var extension = Path.GetExtension(fileName);
			return string.Format("{0}_{1}{2}", shortName, mutationStep, extension);
		}
	}
}
