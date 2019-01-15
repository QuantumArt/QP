using System;
using System.Data;
using AutoMapper;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.BLL.Mappers.EntityPermissions
{
    internal class PermissionListItemRowMapper : GenericMapper<EntityPermissionListItem, DataRow>
    {
        public override void CreateBizMapper(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<DataRow, EntityPermissionListItem>()
                .ForMember(biz => biz.Id, opt => opt.MapFrom(row => Converter.ToInt32(row.Field<decimal>("ID"))))
                .ForMember(biz => biz.UserLogin, opt => opt.MapFrom(row => row.Field<string>("UserLogin")))
                .ForMember(biz => biz.GroupName, opt => opt.MapFrom(row => row.Field<string>("GroupName")))
                .ForMember(biz => biz.LevelName, opt => opt.MapFrom(row => Translator.Translate(row.Field<string>("LevelName"))))
                .ForMember(biz => biz.PropagateToItems, opt => opt.MapFrom(row => Converter.ToBoolean(row.Field<decimal>("PropagateToItems"))))
                .ForMember(biz => biz.Hide, opt => opt.MapFrom(row => row.Field<bool>("Hide")))
                .ForMember(biz => biz.Created, opt => opt.MapFrom(row => row.Field<DateTime>(FieldName.Created)))
                .ForMember(biz => biz.Modified, opt => opt.MapFrom(row => row.Field<DateTime>(FieldName.Modified)))
                .ForMember(biz => biz.LastModifiedByUserId, opt => opt.MapFrom(row => Converter.ToInt32(row.Field<decimal>("LastModifiedByUserId"))))
                .ForMember(biz => biz.LastModifiedByUser, opt => opt.MapFrom(row => row.Field<string>("LastModifiedByUser")));
        }
    }
}
