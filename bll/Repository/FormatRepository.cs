using Quantumart.QP8.BLL.Mappers;
using Quantumart.QP8.DAL;
using Quantumart.QP8.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.BLL.Facades;

namespace Quantumart.QP8.BLL.Repository
{
	class FormatRepository
	{
		internal static ObjectFormat SaveObjectFormatProperties(ObjectFormat objectFormat)
		{
			return DefaultRepository.Save<ObjectFormat, ObjectFormatDAL>(objectFormat);
		}

		internal static ObjectFormat UpdateObjectFormatProperties(ObjectFormat objectFormat)
		{
			return DefaultRepository.Update<ObjectFormat, ObjectFormatDAL>(objectFormat);
		}

		internal static IEnumerable<EntityObject> GetList(IEnumerable<int> IDs, bool pageOrTemplate)
		{
			IEnumerable<decimal> decIDs = Converter.ToDecimalCollection(IDs).Distinct().ToArray();
			var result = MapperFacade.ObjectFormatMapper
				.GetBizList(QPContext.EFContext.ObjectFormatSet
					.Where(f => decIDs.Contains(f.Id))
					.ToList()
				);

			foreach (var item in result)
			{
				item.PageOrTemplate = pageOrTemplate;
			}
			return result;
		}
	}
}
