using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using AutoMapper;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.BLL.Mappers
{
    internal class BackendActionLogRowMapper : GenericMapper<BackendActionLog, DataRow>
    {
        public override void CreateBizMapper()
        {
            Mapper.CreateMap<DataRow, BackendActionLog>()
                .ForMember(biz => biz.Id, opt => opt.MapFrom(row => row.Field<int>("Id")))
                .ForMember(biz => biz.ExecutionTime, opt => opt.MapFrom(row => row.Field<DateTime>("ExecutionTime")))
                .ForMember(biz => biz.ActionName, opt => opt.MapFrom(row => row.Field<string>("ActionName")))
                .ForMember(biz => biz.ActionTypeCode, opt => opt.MapFrom(row => row.Field<string>("ActionTypeCode")))
                .ForMember(biz => biz.ActionTypeName, opt => opt.MapFrom(row => row.Field<string>("ActionTypeName")))
                .ForMember(biz => biz.EntityTypeCode, opt => opt.MapFrom(row => row.Field<string>("EntityTypeCode")))
                .ForMember(biz => biz.EntityTypeName, opt => opt.MapFrom(row => row.Field<string>("EntityTypeName")))
                .ForMember(biz => biz.EntityStringId, opt => opt.MapFrom(row => row.Field<string>("EntityStringId")))
                .ForMember(biz => biz.ParentEntityId, opt => opt.MapFrom(row => Converter.ToNullableInt32(row.Field<decimal?>("ParentEntityId"))))
                .ForMember(biz => biz.EntityTitle, opt => opt.MapFrom(row => row.Field<string>("EntityTitle")))
                .ForMember(biz => biz.UserId, opt => opt.MapFrom(row => Converter.ToNullableInt32(row.Field<decimal?>("UserId"))))
                .ForMember(biz => biz.IsApi, opt => opt.MapFrom(row => row.Field<bool>("IsApi")))
                .ForMember(biz => biz.UserLogin, opt => opt.MapFrom(row => row.Field<string>("UserLogin")));
        }
    }
}
