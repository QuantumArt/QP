using System.Data;
using Quantumart.QP8.BLL.ListItems;
using System;
using AutoMapper;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.BLL.Mappers
{
	class VisualEditorStyleListItemRowMapper : GenericMapper<VisualEditorStyleListItem, DataRow>
	{
		public override void CreateBizMapper()
		{
			Mapper.CreateMap<DataRow, VisualEditorStyleListItem>()
				.ForMember(biz => biz.Id, opt => opt.MapFrom(row => Converter.ToInt32(row.Field<decimal>("Id"))))
				.ForMember(biz => biz.Name, opt => opt.MapFrom(row => row.Field<string>("Name")))
				.ForMember(biz => biz.Description, opt => opt.MapFrom(row => row.Field<string>("Description")))
				.ForMember(biz => biz.Tag, opt => opt.MapFrom(row => row.Field<string>("Tag")))
				.ForMember(biz => biz.Order, opt => opt.MapFrom(row => Converter.ToInt32(row.Field<int>("Order"))))
				.ForMember(biz => biz.IsFormat, opt => opt.MapFrom(row => row.Field<bool>("IsFormat")))
				.ForMember(biz => biz.IsSystem, opt => opt.MapFrom(row => row.Field<bool>("IsSystem")))				
				.ForMember(biz => biz.Created, opt => opt.MapFrom(row => row.Field<DateTime>("Created")))
				.ForMember(biz => biz.Modified, opt => opt.MapFrom(row => row.Field<DateTime>("Modified")))				
				.ForMember(biz => biz.LastModifiedByLogin, opt => opt.MapFrom(row => row.Field<string>("LastModifiedByLogin")))
				;
		}
	}
}
