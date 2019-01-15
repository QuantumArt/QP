using AutoMapper;
using Quantumart.QP8.BLL.Facades;
using Quantumart.QP8.DAL;

namespace Quantumart.QP8.BLL.Mappers.EntityPermissions
{
    internal class BackendActionPermissionMapper : GenericMapper<EntityPermission, BackendActionPermissionDAL>
    {
        public override void CreateBizMapper(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<BackendActionPermissionDAL, EntityPermission>()
                .ForMember(biz => biz.Parent, opt => opt.MapFrom(data => MapperFacade.BackendActionMapper.GetBizObject(data.Action)))
                .ForMember(biz => biz.ParentEntityId, opt => opt.MapFrom(data => data.ActionId));
        }

        public override void CreateDalMapper(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<EntityPermission, BackendActionPermissionDAL>()
                .ForMember(data => data.Action, opt => opt.Ignore())
                .ForMember(data => data.ActionId, opt => opt.MapFrom(biz => biz.ParentEntityId))
                .ForMember(data => data.LastModifiedByUser, opt => opt.Ignore())
                .ForMember(data => data.Group, opt => opt.Ignore())
                .ForMember(data => data.PermissionLevel, opt => opt.Ignore())
                .ForMember(data => data.User, opt => opt.Ignore());
        }
    }
}
