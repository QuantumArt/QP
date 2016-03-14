using AutoMapper;
using Quantumart.QP8.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Quantumart.QP8.BLL.Mappers
{
	class StatusTypeRowMapper : GenericMapper<StatusType, DataRow>
	{
		public override void CreateBizMapper()
		{
			Mapper.CreateMap<DataRow, StatusType>()
				.ForMember(biz => biz.Id, opt => opt.MapFrom(row => Converter.ToInt32(row.Field<decimal>("STATUS_TYPE_ID"))))
				.ForMember(biz => biz.Weight, opt => opt.MapFrom(row => Converter.ToInt32(row.Field<decimal>("WEIGHT"))))
				.ForMember(biz => biz.SiteId, opt => opt.MapFrom(row => Converter.ToInt32(row.Field<decimal>("SITE_ID"))))
				.ForMember(biz => biz.Name, opt => opt.MapFrom(row => row.Field<string>("STATUS_TYPE_NAME")))
				.ForMember(biz => biz.Description, opt => opt.MapFrom(row => row.Field<string>("DESCRIPTION")))
				.ForMember(biz => biz.Created, opt => opt.MapFrom(row => row.Field<DateTime>("Created")))
				.ForMember(biz => biz.Modified, opt => opt.MapFrom(row => row.Field<DateTime>("Modified")))
				.ForMember(biz => biz.LastModifiedBy, opt => opt.MapFrom(row => Converter.ToInt32(row.Field<decimal>("LAST_MODIFIED_BY"))))
				.ForMember(biz => biz.BuiltIn, opt => opt.MapFrom(row => Converter.ToBoolean(row.Field<bool>("BUILT_IN"))));				
		}		
	}
}
