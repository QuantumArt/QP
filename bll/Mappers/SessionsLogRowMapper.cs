using System;
using System.Data;
using AutoMapper;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.BLL.Mappers
{	
	internal class SessionsLogRowMapper : GenericMapper<SessionsLog, DataRow>
	{
		public override void CreateBizMapper()
		{
			Mapper.CreateMap<DataRow, SessionsLog>()
				.ForMember(biz => biz.SessionId, opt => opt.MapFrom(row => Converter.ToInt32(row.Field<decimal>("SessionId"))))
				.ForMember(biz => biz.AutoLogged, opt => opt.MapFrom(row => row.Field<int>("AutoLogged")))
				.ForMember(biz => biz.Browser, opt => opt.MapFrom(row => row.Field<string>("Browser")))				
				.ForMember(biz => biz.IP, opt => opt.MapFrom(row => row.Field<string>("IP")))
				.ForMember(biz => biz.IsQP7, opt => opt.MapFrom(row => row.Field<bool>("IsQP7")))
				.ForMember(biz => biz.Login, opt => opt.MapFrom(row => row.Field<string>("Login")))
				.ForMember(biz => biz.UserId, opt => opt.MapFrom(row => Converter.ToNullableInt32(row.Field<decimal?>("UserId"))))
				.ForMember(biz => biz.ServerName, opt => opt.MapFrom(row => row.Field<string>("ServerName")))				
				.ForMember(biz => biz.Sid, opt => opt.MapFrom(row => row.Field<string>("Sid")))
				.ForMember(biz => biz.StartTime, opt => opt.MapFrom(row => row.Field<DateTime>("StartTime")))
				.ForMember(biz => biz.EndTime, opt => opt.MapFrom(row => row.Field<DateTime?>("EndTime")));
				
		}
	}
}
