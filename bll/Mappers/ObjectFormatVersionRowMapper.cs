using System;
using System.Data;
using AutoMapper;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.BLL.Mappers
{	
	class ObjectFormatVersionRowMapper : GenericMapper<ObjectFormatVersionListItem, DataRow>
	{
		public override void CreateBizMapper()
		{
			Mapper.CreateMap<DataRow, ObjectFormatVersionListItem>()
				.ForMember(biz => biz.Id, opt => opt.MapFrom(row => Converter.ToInt32(row.Field<decimal>("Id"))))				
				.ForMember(biz => biz.Description, opt => opt.MapFrom(row => row.Field<string>("Description")))
				.ForMember(biz => biz.Modified, opt => opt.MapFrom(row => row.Field<DateTime>("Modified")))
				.ForMember(biz => biz.ModifiedByLogin, opt => opt.MapFrom(row => row.Field<string>("LastModifiedByLogin")))
				;
		}		
	}
}
