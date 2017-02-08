using System;
using System.Data;
using AutoMapper;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.BLL.Mappers
{
    internal class UserListItemRowMapper : GenericMapper<UserListItem, DataRow>
    {
        public override void CreateBizMapper()
        {
            Mapper.CreateMap<DataRow, UserListItem>()
                .ForMember(biz => biz.Id, opt => opt.MapFrom(row => Converter.ToInt32(row.Field<decimal>("ID"))))
                .ForMember(biz => biz.Login, opt => opt.MapFrom(row => row.Field<string>("LOGIN")))
                .ForMember(biz => biz.FirstName, opt => opt.MapFrom(row => row.Field<string>("FIRST_NAME")))
                .ForMember(biz => biz.LastName, opt => opt.MapFrom(row => row.Field<string>("LAST_NAME")))
                .ForMember(biz => biz.Email, opt => opt.MapFrom(row => row.Field<string>("EMAIL")))
                .ForMember(biz => biz.LanguageId, opt => opt.MapFrom(row => Converter.ToInt32(row.Field<decimal>("LANGUAGE_ID"))))
                .ForMember(biz => biz.Language, opt => opt.MapFrom(row => Translator.Translate(row.Field<string>("LANGUAGE_NAME"))))
                .ForMember(biz => biz.LastLogOn, opt => opt.MapFrom(row => row.Field<DateTime?>("LAST_LOGIN")))
                .ForMember(biz => biz.Disabled, opt => opt.MapFrom(row => Converter.ToBoolean(row.Field<decimal>("DISABLED"))))
                .ForMember(biz => biz.Created, opt => opt.MapFrom(row => row.Field<DateTime>(FieldName.Created)))
                .ForMember(biz => biz.Modified, opt => opt.MapFrom(row => row.Field<DateTime>(FieldName.Modified)))
                .ForMember(biz => biz.LastModifiedByUserId, opt => opt.MapFrom(row => Converter.ToInt32(row.Field<decimal>(FieldName.LastModifiedBy))))
                .ForMember(biz => biz.LastModifiedByUser, opt => opt.MapFrom(row => row.Field<string>("LAST_MODIFIED_BY_LOGIN")));
        }
    }
}
