using AutoMapper;
using Quantumart.QP8.BLL.Facades;
using Quantumart.QP8.DAL;

namespace Quantumart.QP8.BLL.Mappers.EntityPermissions
{
    internal class EntityTypePermissionMapper : GenericMapper<EntityPermission, EntityTypePermissionDAL>
    {
        public override void CreateBizMapper()
        {
            Mapper.CreateMap<EntityTypePermissionDAL, EntityPermission>()
                .ForMember(biz => biz.Parent, opt => opt.MapFrom(data => MapperFacade.EntityTypeMapper.GetBizObject(data.EntityType)))
                .ForMember(biz => biz.ParentEntityId, opt => opt.MapFrom(data => data.EntityTypeId));
        }

        public override void CreateDalMapper()
        {
            Mapper.CreateMap<EntityPermission, EntityTypePermissionDAL>()
                .ForMember(data => data.EntityType, opt => opt.Ignore())
                .ForMember(data => data.EntityTypeId, opt => opt.MapFrom(biz => biz.ParentEntityId))
                .ForMember(data => data.LastModifiedByUser, opt => opt.Ignore())
                .ForMember(data => data.Group, opt => opt.Ignore())
                .ForMember(data => data.PermissionLevel, opt => opt.Ignore())
                .ForMember(data => data.User, opt => opt.Ignore());
        }
    }
}
