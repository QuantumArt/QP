using AutoMapper;
using Quantumart.QP8.BLL.Facades;
using Quantumart.QP8.DAL;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.BLL.Mappers.EntityPermissions
{
    internal class ArticlePermissionMapper : GenericMapper<EntityPermission, ArticlePermissionDAL>
    {
        public override void CreateBizMapper(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<ArticlePermissionDAL, EntityPermission>()
                .ForMember(biz => biz.Parent, opt => opt.MapFrom(data => MapperFacade.ArticleMapper.GetBizObject(data.Article)))
                .ForMember(biz => biz.ParentEntityId, opt => opt.MapFrom(data => Converter.ToInt32(data.ArticleId)));
        }

        public override void CreateDalMapper(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<EntityPermission, ArticlePermissionDAL>()
                .ForMember(data => data.Article, opt => opt.Ignore())
                .ForMember(data => data.ArticleId, opt => opt.MapFrom(biz => Converter.ToDecimal(biz.ParentEntityId)))
                .ForMember(data => data.LastModifiedByUser, opt => opt.Ignore())
                .ForMember(data => data.Group, opt => opt.Ignore())
                .ForMember(data => data.PermissionLevel, opt => opt.Ignore())
                .ForMember(data => data.User, opt => opt.Ignore());
        }
    }
}
