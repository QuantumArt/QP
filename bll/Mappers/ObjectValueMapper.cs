using AutoMapper;
using Quantumart.QP8.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quantumart.QP8.BLL.Mappers
{
	internal class ObjectValueMapper : GenericMapper<ObjectValue, ObjectValuesDAL>
	{		
		public override void CreateDalMapper()
		{
			Mapper.CreateMap<ObjectValue, ObjectValuesDAL>()
				.ForMember(data => data.Object, opt => opt.Ignore())
			;
		}
	}
}
