using System.Data;
using AutoMapper;
using Quantumart.QP8.BLL.Repository.Results;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.BLL.Mappers
{
    internal class VirtualFieldsRelationMapper : GenericMapper<VirtualFieldsRelation, DataRow>
    {
        public override void CreateBizMapper(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<DataRow, VirtualFieldsRelation>()
                .ForMember(biz => biz.BaseFieldId, opt => opt.MapFrom(r => Converter.ToInt32(r.Field<decimal>("BASE_ATTR_ID"))))
                .ForMember(biz => biz.BaseFieldContentId, opt => opt.MapFrom(r => Converter.ToInt32(r.Field<decimal>("BASE_CNT_ID"))))
                .ForMember(biz => biz.VirtualFieldId, opt => opt.MapFrom(r => Converter.ToInt32(r.Field<decimal>("VIRTUAL_ATTR_ID"))))
                .ForMember(biz => biz.VirtualFieldContentId, opt => opt.MapFrom(r => Converter.ToInt32(r.Field<decimal>("VIRTUAL_CNT_ID"))));
        }
    }
}
