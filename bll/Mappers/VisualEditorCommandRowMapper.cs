using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using AutoMapper;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.BLL.Mappers
{
	class VisualEditorCommandRowMapper : GenericMapper<VisualEditorCommand, DataRow>
	{
		public override void CreateBizMapper()
		{
			Mapper.CreateMap<DataRow, VisualEditorCommand>()
				.ForMember(biz => biz.Id, opt => opt.MapFrom(row => Converter.ToInt32(row.Field<decimal>("ID"))))
				.ForMember(biz => biz.Name, opt => opt.MapFrom(row => row.Field<string>("NAME")))
				.ForMember(biz => biz.Alias, opt => opt.MapFrom(row => Translator.Translate(row.Field<string>("ALIAS"))))
				.ForMember(biz => biz.RowOrder, opt => opt.MapFrom(row => row.Field<int>("ROW_ORDER")))
				.ForMember(biz => biz.ToolbarInRowOrder, opt => opt.MapFrom(row => row.Field<int>("TOOLBAR_IN_ROW_ORDER")))
				.ForMember(biz => biz.GroupInToolbarOrder, opt => opt.MapFrom(row => row.Field<int>("GROUP_IN_TOOLBAR_ORDER")))
				.ForMember(biz => biz.CommandInGroupOrder, opt => opt.MapFrom(row => row.Field<int>("COMMAND_IN_GROUP_ORDER")))
				.ForMember(biz => biz.On, opt => opt.MapFrom(row => row.Field<bool>("ON")))
				.ForMember(biz => biz.Created, opt => opt.MapFrom(row => row.Field<DateTime>("CREATED")))
				.ForMember(biz => biz.Modified, opt => opt.MapFrom(row => row.Field<DateTime>("MODIFIED")))
				.ForMember(biz => biz.PluginId, opt => opt.MapFrom(row => Converter.ToNullableInt32(row.Field<decimal?>("PLUGIN_ID"))))
				;
		}
	}
}
