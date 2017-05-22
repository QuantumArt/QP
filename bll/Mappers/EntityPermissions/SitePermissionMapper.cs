using AutoMapper;
using Quantumart.QP8.BLL.Facades;
using Quantumart.QP8.DAL;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.BLL.Mappers.EntityPermissions
{
    internal class SitePermissionMapper : GenericMapper<EntityPermission, SitePermissionDAL>
    {
        public override void CreateBizMapper()
        {
            Mapper.CreateMap<SitePermissionDAL, EntityPermission>()
                .ForMember(biz => biz.Parent, opt => opt.MapFrom(data => MapperFacade.SiteMapper.GetBizObject(data.Site)))
                .ForMember(biz => biz.ParentEntityId, opt => opt.MapFrom(data => Converter.ToInt32(data.SiteId)));
        }

        public override void CreateDalMapper()
        {
            Mapper.CreateMap<EntityPermission, SitePermissionDAL>()
                .ForMember(data => data.Site, opt => opt.Ignore())
                .ForMember(data => data.SiteId, opt => opt.MapFrom(biz => Converter.ToDecimal(biz.ParentEntityId)))
                .ForMember(data => data.LastModifiedByUser, opt => opt.Ignore())
                .ForMember(data => data.Group, opt => opt.Ignore())
                .ForMember(data => data.PermissionLevel, opt => opt.Ignore())
                .ForMember(data => data.User, opt => opt.Ignore());
        }
    }
}
