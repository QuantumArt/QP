using AutoMapper;
using Quantumart.QP8.DAL;
using Quantumart.QP8.DAL.Entities;

namespace Quantumart.QP8.BLL.Mappers
{
    internal class CustomActionMapper : GenericMapper<CustomAction, CustomActionDAL>
    {
        public override void CreateDalMapper(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<CustomAction, CustomActionDAL>(MemberList.Destination)
                .ForMember(data => data.Action, opt => opt.Ignore())
                .ForMember(data => data.LastModifiedByUser, opt => opt.Ignore())
                .ForMember(data => data.Sites, opt => opt.Ignore())
                .ForMember(data => data.Contents, opt => opt.Ignore());
        }
    }
}
