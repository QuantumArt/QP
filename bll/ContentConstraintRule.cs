using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.BLL
{
	public class ContentConstraintRule : BizObject
	{
		public ContentConstraintRule()
		{
            field = new InitPropertyValue<Field>(() => FieldRepository.GetById(FieldId));
		}

		public int ConstraintId
		{
			get;
			set;
		}

		public int FieldId
		{
			get;
			set;
		}

		InitPropertyValue<Field> field = null;
		public Field Field
		{
			get
			{
				return field.Value;
			}
		}		
	}
}
