using AutoMapper;
using Quantumart.QP8.DAL;

namespace Quantumart.QP8.BLL.Mappers
{
    internal class ContentLinkMapper : GenericMapper<ContentLink, ContentToContentDAL>
    {
        public override void CreateBizMapper()
        {
            Mapper.CreateMap<ContentToContentDAL, ContentLink>()
                .ForMember(biz => biz.Content, opt => opt.Ignore())
                ;
        }

        public override void CreateDalMapper()
        {
            Mapper.CreateMap<ContentLink, ContentToContentDAL>()
                .ForMember(data => data.Content, opt => opt.Ignore())
                ;
        }
    }
}
