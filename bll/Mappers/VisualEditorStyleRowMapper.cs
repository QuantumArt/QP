using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Quantumart.QP8.Utils;
using AutoMapper;

namespace Quantumart.QP8.BLL.Mappers
{
	class VisualEditorStyleRowMapper : GenericMapper<VisualEditorStyle, DataRow>
	{
		public override void CreateBizMapper()
		{
			Mapper.CreateMap<DataRow, VisualEditorStyle>()
				.ForMember(biz => biz.Id, opt => opt.MapFrom(row => Converter.ToInt32(row.Field<decimal>("ID"))))
				.ForMember(biz => biz.Name, opt => opt.MapFrom(row => row.Field<string>("NAME")))
				.ForMember(biz => biz.Description, opt => opt.MapFrom(row => row.Field<string>("DESCRIPTION")))
				.ForMember(biz => biz.Tag, opt => opt.MapFrom(row => row.Field<string>("TAG")))				
				.ForMember(biz => biz.Order, opt => opt.MapFrom(row => row.Field<int>("ORDER")))
				.ForMember(biz => biz.OverridesTag, opt => opt.MapFrom(row => row.Field<string>("OVERRIDES_TAG")))
				.ForMember(biz => biz.IsFormat, opt => opt.MapFrom(row => row.Field<bool>("IS_FORMAT")))
				.ForMember(biz => biz.IsSystem, opt => opt.MapFrom(row => row.Field<bool>("IS_SYSTEM")))
				.ForMember(biz => biz.Attributes, opt => opt.MapFrom(row => row.Field<string>("ATTRIBUTES")))
				.ForMember(biz => biz.Styles, opt => opt.MapFrom(row => row.Field<string>("STYLES")))
				.ForMember(biz => biz.Created, opt => opt.MapFrom(row => row.Field<DateTime>("CREATED")))
				.ForMember(biz => biz.Modified, opt => opt.MapFrom(row => row.Field<DateTime>("MODIFIED")))
				.ForMember(biz => biz.On, opt => opt.MapFrom(row => row.Field<bool>("ON")))
				;
		}
	}
}
