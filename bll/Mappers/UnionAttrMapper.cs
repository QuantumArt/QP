using AutoMapper;
using Quantumart.QP8.BLL.Facades;
using Quantumart.QP8.DAL;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.BLL.Mappers
{
    internal class UnionAttrMapper : GenericMapper<UnionAttr, UnionAttrDAL>
    {
        public override void CreateBizMapper(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<UnionAttrDAL, UnionAttr>(MemberList.Source)
                .ForMember(biz => biz.BaseFieldId, opt => opt.MapFrom(r => Converter.ToInt32(r.UnionFieldId)))
                .ForMember(biz => biz.VirtualFieldId, opt => opt.MapFrom(r => Converter.ToInt32(r.VirtualFieldId)))
                .ForMember(biz => biz.VirtualField, opt => opt.MapFrom(r => MapperFacade.FieldMapper.GetBizObject(r.VirtualField)))
                .ForMember(biz => biz.BaseField, opt => opt.MapFrom(r => MapperFacade.FieldMapper.GetBizObject(r.UnionField)));
        }

        public override void CreateDalMapper(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<UnionAttr, UnionAttrDAL>(MemberList.Destination)
                .ForMember(data => data.UnionFieldId, opt => opt.MapFrom(biz => Converter.ToDecimal(biz.BaseFieldId)))
                .ForMember(data => data.VirtualFieldId, opt => opt.MapFrom(biz => Converter.ToDecimal(biz.VirtualFieldId)))
                .ForMember(data => data.UnionField, opt => opt.Ignore())
                .ForMember(data => data.VirtualField, opt => opt.Ignore());
        }
    }
}
