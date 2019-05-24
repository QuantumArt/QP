using AutoMapper;
using Quantumart.QP8.DAL.Entities;

namespace Quantumart.QP8.BLL.Mappers
{
    internal class ContextMenuItemMapper : GenericMapper<ContextMenuItem, ContextMenuItemDAL>
    {
        public override void CreateBizMapper(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<ContextMenuItemDAL, ContextMenuItem>(MemberList.Source)
                .ForMember(biz => biz.ActionCode, opt => opt.MapFrom(x => x.Action != null ? x.Action.Code : null))
                .ForMember(biz => biz.ActionTypeCode, opt => opt.MapFrom(x => x.Action != null && x.Action.ActionType != null ? x.Action.ActionType.Code : null));


        }
    }
}