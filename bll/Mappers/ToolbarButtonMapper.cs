using AutoMapper;
using Quantumart.QP8.DAL;
using Quantumart.QP8.DAL.Entities;

namespace Quantumart.QP8.BLL.Mappers
{
    internal class ToolbarButtonMapper : GenericMapper<ToolbarButton, ToolbarButtonDAL>
    {
        public override void CreateDalMapper(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<ToolbarButton, ToolbarButtonDAL>(MemberList.Destination)
                .ForMember(data => data.Action, opt => opt.Ignore())
                .ForMember(data => data.ParentAction, opt => opt.Ignore());
        }
    }
}
