using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Constants;

namespace Quantumart.QP8.BLL.Services
{
	public static  class VirtualFieldService
	{
		public static Field Update(Field item)
		{
			if (item == null)
				throw new ArgumentNullException("item");
			if (!FieldRepository.Exists(item.Id))
				throw new ApplicationException(String.Format(FieldStrings.FieldNotFound, item.Id));
			if (item.Content.VirtualType == VirtualType.None)
				throw new ApplicationException(String.Format("Content {0} of field {1} is not virtual.", item.Content.Id, item.Id));

			using (VirtualFieldRepository.LoadVirtualFieldsRelationsToMemory(item.ContentId))
			{
				Field newField = FieldRepository.Update(item);
				var helper = new VirtualContentHelper();

				helper.DropContentViews(newField.Content);
				helper.CreateContentViews(newField.Content);

				// Обновить дочерние виртуальные контенты
				helper.UpdateVirtualFields(newField);

				return newField;
			}
			
		}
	}
}
