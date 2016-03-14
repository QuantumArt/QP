using Quantumart.QP8.BLL.Mappers;
using Quantumart.QP8.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quantumart.QP8.BLL.Repository
{
	class ObjectTypeRepository
	{
		public static ObjectType GetByName(string name)
		{
			QP8Entities entities = QPContext.EFContext;
			return MappersRepository.ObjectTypeMapper.GetBizObject(entities.ObjectTypeSet.SingleOrDefault(x => x.Name == name));
		}
	}
}
