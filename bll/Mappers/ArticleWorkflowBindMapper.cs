using AutoMapper;
using Quantumart.QP8.DAL;

namespace Quantumart.QP8.BLL.Mappers
{
    internal class ArticleWorkflowBindMapper : GenericMapper<ArticleWorkflowBind, ArticleWorkflowBindDAL>
    {
        public override void CreateBizMapper(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<ArticleWorkflowBindDAL, ArticleWorkflowBind>(MemberList.Source)
                .ForMember(biz => biz.Article, opt => opt.Ignore())
                ;
        }

        public override void CreateDalMapper(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<ArticleWorkflowBind, ArticleWorkflowBindDAL>(MemberList.Destination)
                .ForMember(data => data.Article, opt => opt.Ignore())
                ;
        }
    }
}
