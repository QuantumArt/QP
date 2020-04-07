using System;
using System.Data;
using AutoMapper;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.BLL.Mappers
{
    internal class ContentFolderRowMapper : GenericMapper<ContentFolder, DataRow>
    {
        public override void CreateBizMapper(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<DataRow, ContentFolder>(MemberList.Source)
                .ForMember(biz => biz.Id, opt => opt.MapFrom(row => Converter.ToInt32(row.Field<decimal>("FOLDER_ID"))))
                .ForMember(biz => biz.Name, opt => opt.MapFrom(row => row.Field<string>("NAME")))
                .ForMember(biz => biz.LastModifiedBy, opt => opt.MapFrom(row => Converter.ToInt32(row.Field<decimal>(FieldName.LastModifiedBy))))
                .ForMember(biz => biz.Created, opt => opt.MapFrom(row => row.Field<DateTime>(FieldName.Created)))
                .ForMember(biz => biz.Modified, opt => opt.MapFrom(row => row.Field<DateTime>(FieldName.Modified)))
                .ForMember(biz => biz.HasChildren, opt => opt.MapFrom(row => row.Field<bool>("HAS_CHILDREN")))
                .ForMember(biz => biz.LastModifiedByUser, opt => opt.MapFrom(row => SiteFolderRowMapper.GetModifierUserFromRow(row)));
        }
    }
}
