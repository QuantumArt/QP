using AutoMapper;
using Quantumart.QP8.DAL;

namespace Quantumart.QP8.BLL.Mappers
{
    internal class ContentFolderMapper : GenericMapper<ContentFolder, ContentFolderDAL>
    {
        public override void CreateBizMapper(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<ContentFolderDAL, ContentFolder>(MemberList.Source)
                .ForMember(biz => biz.StoredPath, opt => opt.MapFrom(data => data.Path));
        }

        public override void CreateDalMapper(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<ContentFolder, ContentFolderDAL>(MemberList.Destination)
                .ForMember(data => data.Content, opt => opt.Ignore())
                .ForMember(data => data.LastModifiedByUser, opt => opt.Ignore());
        }
    }
}
