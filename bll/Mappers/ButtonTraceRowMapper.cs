using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using AutoMapper;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.BLL.Mappers
{
	internal class ButtonTraceRowMapper : GenericMapper<ButtonTrace, DataRow>
	{
		public override void CreateBizMapper()
		{
			Mapper.CreateMap<DataRow, ButtonTrace>()
				.ForMember(biz => biz.ButtonName, opt => opt.MapFrom(row => Converter.ToString(row.Field<String>("ButtonName"), String.Empty)))
                .ForMember(biz => biz.TabName, opt => opt.MapFrom(row => Converter.ToString(row.Field<String>("TabName"), String.Empty)))
				.ForMember(biz => biz.ActivatedTime, opt => opt.MapFrom(row => row.Field<DateTime>("ActivatedTime")))				
				.ForMember(biz => biz.UserId, opt => opt.MapFrom(row => Converter.ToInt32(row.Field<decimal>("UserId"))))
				.ForMember(biz => biz.UserLogin, opt => opt.MapFrom(row => row.Field<string>("UserLogin")));
		}
	}
}
