using AutoMapper;
using Quantumart.QP8.DAL;
using SiteFolderDAL = Quantumart.QP8.DAL.Entities.SiteFolderDAL;

namespace Quantumart.QP8.BLL.Mappers
{
    internal class SiteFolderMapper : GenericMapper<SiteFolder, SiteFolderDAL>
    {
        public override void CreateBizMapper(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<SiteFolderDAL, SiteFolder>(MemberList.Source)
                .ForMember(biz => biz.StoredPath, opt => opt.MapFrom(data => data.Path));
        }

        public override void CreateDalMapper(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<SiteFolder, SiteFolderDAL>(MemberList.Destination)
                .ForMember(data => data.Site, opt => opt.Ignore())
                .ForMember(data => data.LastModifiedByUser, opt => opt.Ignore());
        }
    }
}
