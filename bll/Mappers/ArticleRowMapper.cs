using System;
using System.Data;
using AutoMapper;
using Quantumart.QP8.Constants;

namespace Quantumart.QP8.BLL.Mappers
{
    internal class ArticleRowMapper : GenericMapper<Article, DataRow>
    {
        public override void CreateBizMapper(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<DataRow, Article>(MemberList.Source)
                .ForMember(biz => biz.Id, opt => opt.MapFrom(row => row.Field<decimal>(FieldName.ContentItemId)))
                .ForMember(biz => biz.StatusTypeId, opt => opt.MapFrom(row => row.Field<decimal>(FieldName.StatusTypeId)))
                .ForMember(biz => biz.Visible, opt => opt.MapFrom(row => row.Field<decimal>(FieldName.Visible)))
                .ForMember(biz => biz.Archived, opt => opt.MapFrom(row => row.Field<decimal>(FieldName.Archive)))
                .ForMember(biz => biz.LastModifiedBy, opt => opt.MapFrom(row => row.Field<decimal>(FieldName.LastModifiedBy)))
                .ForMember(biz => biz.Created, opt => opt.MapFrom(row => row.Field<DateTime>(FieldName.Created)))
                .ForMember(biz => biz.Modified, opt => opt.MapFrom(row => row.Field<DateTime>(FieldName.Modified)));
        }
    }
}
