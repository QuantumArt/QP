using AutoMapper;
using Quantumart.QP8.DAL;
using Quantumart.QP8.DAL.Entities;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.BLL.Mappers
{
    internal class UserQueryAttrMapper : GenericMapper<UserQueryAttr, UserQueryAttrsDAL>
    {
        public override void CreateBizMapper(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<UserQueryAttrsDAL, UserQueryAttr>(MemberList.Source)
                .ForMember(biz => biz.BaseFieldId, opt => opt.MapFrom(r => Converter.ToInt32(r.UserQueryAttrId)))
                .ForMember(biz => biz.UserQueryContentId, opt => opt.MapFrom(r => Converter.ToInt32(r.VirtualContentId)));
        }

        public override void CreateDalMapper(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<UserQueryAttr, UserQueryAttrsDAL>(MemberList.Destination)
                .ForMember(data => data.UserQueryAttrId, opt => opt.MapFrom(biz => Converter.ToDecimal(biz.BaseFieldId)))
                .ForMember(data => data.VirtualContentId, opt => opt.MapFrom(biz => Converter.ToDecimal(biz.UserQueryContentId)));
        }
    }
}
