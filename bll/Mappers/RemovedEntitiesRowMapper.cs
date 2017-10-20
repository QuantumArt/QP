using System;
using System.Data;
using AutoMapper;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.BLL.Mappers
{
	internal class RemovedEntitiesRowMapper : GenericMapper<RemovedEntity, DataRow>
	{
		public override void CreateBizMapper()
		{
			Mapper.CreateMap<DataRow, RemovedEntity>()
				.ForMember(biz => biz.DeletedTime, opt => opt.MapFrom(row => row.Field<DateTime>("DeletedTime")))
				.ForMember(biz => biz.EntityId, opt => opt.MapFrom(row => Converter.ToInt32(row.Field<decimal>("EntityId"))))
				.ForMember(biz => biz.ParentEntityId, opt => opt.MapFrom(row => Converter.ToInt32(row.Field<decimal>("ParentEntityId"))))
				.ForMember(biz => biz.EntityTypeCode, opt => opt.MapFrom(row => row.Field<string>("EntityTypeCode")))								
				.ForMember(biz => biz.EntityTitle, opt => opt.MapFrom(row => row.Field<string>("EntityTitle")))
				.ForMember(biz => biz.UserId, opt => opt.MapFrom(row => Converter.ToInt32(row.Field<decimal>("UserId"))))
				.ForMember(biz => biz.UserLogin, opt => opt.MapFrom(row => row.Field<string>("UserLogin")));
		}
	}
}
