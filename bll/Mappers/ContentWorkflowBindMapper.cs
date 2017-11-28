using AutoMapper;
using Quantumart.QP8.DAL;

namespace Quantumart.QP8.BLL.Mappers
{
    internal class ContentWorkflowBindMapper : GenericMapper<ContentWorkflowBind, ContentWorkflowBindDAL>
    {
        public override void CreateBizMapper()
        {
            Mapper.CreateMap<ContentWorkflowBindDAL, ContentWorkflowBind>()
                .ForMember(biz => biz.Content, opt => opt.Ignore())
                ;
        }

        public override void CreateDalMapper()
        {
            Mapper.CreateMap<ContentWorkflowBind, ContentWorkflowBindDAL>()
                .ForMember(data => data.Content, opt => opt.Ignore())
                ;
        }
    }
}
