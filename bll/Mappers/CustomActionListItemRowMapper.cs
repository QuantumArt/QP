using System;
using System.Collections.Generic;
using System.Data;
using AutoMapper;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.BLL.Mappers
{
    internal class CustomActionListItemRowMapper : GenericMapper<CustomActionListItem, DataRow>
    {
        public override void CreateBizMapper()
        {
            Mapper.CreateMap<DataRow, CustomActionListItem>()
                .ForMember(biz => biz.Id, opt => opt.MapFrom(row => Converter.ToInt32(row.Field<decimal>("ID"))))
                .ForMember(biz => biz.Name, opt => opt.MapFrom(row => row.Field<string>("NAME")))
                .ForMember(biz => biz.Url, opt => opt.MapFrom(row => row.Field<string>("URL")))
                .ForMember(biz => biz.Order, opt => opt.MapFrom(row => row.Field<int>("ORDER")))

                .ForMember(biz => biz.ActionTypeName, opt => opt.MapFrom(row => Translator.Translate(row.Field<string>("ACTION_TYPE_NAME"))))
                .ForMember(biz => biz.EntityTypeName, opt => opt.MapFrom(row => Translator.Translate(row.Field<string>("ENTITY_TYPE_NAME"))))

                .ForMember(biz => biz.Created, opt => opt.MapFrom(row => row.Field<DateTime>(FieldName.Created)))
                .ForMember(biz => biz.Modified, opt => opt.MapFrom(row => row.Field<DateTime>(FieldName.Modified)))
                .ForMember(biz => biz.LastModifiedByUserId, opt => opt.MapFrom(row => Converter.ToInt32(row.Field<decimal>("USER_ID"))))
                .ForMember(biz => biz.LastModifiedByUser, opt => opt.MapFrom(row => row.Field<string>("LOGIN")));
        }

        public string TranslateSortExpression(string sortExpression)
        {
            if (string.IsNullOrEmpty(sortExpression))
            {
                sortExpression = "[ORDER] ASC";
            }

            var replaces = new Dictionary<string, string>
            {
                {"LastModifiedByUser", "LOGIN"},
                {"ActionTypeName", "ACTION_TYPE_NAME"},
                {"EntityTypeName", "ENTITY_TYPE_NAME"}
            };

            return TranslateHelper.TranslateSortExpression(sortExpression, replaces);
        }
    }
}
