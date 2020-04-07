using System.Data;
using System.Linq;
using AutoMapper;
using Quantumart.QP8.BLL.Facades;
using Quantumart.QP8.DAL.Entities;

namespace Quantumart.QP8.BLL.Mappers
{
    internal class ContextMenuRowMapper : GenericMapper<ContextMenu, DataRow>
    {
        public override void CreateBizMapper(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<DataRow, ContextMenu>(MemberList.Source)
                .ForMember(biz => biz.Id, opt => opt.MapFrom(row => row.Field<int>("ID")))
                .ForMember(biz => biz.Code, opt => opt.MapFrom(row => row.Field<string>("CODE")))
                .ForMember(biz => biz.Items, opt => opt.MapFrom(row => MapperFacade.ContextMenuItemRowMapper.GetBizList(row.GetChildRows("Menu2Item").ToList())));
        }


    }

    internal class ContextMenuMapper : GenericMapper<ContextMenu, ContextMenuDAL>
    {
        public override void CreateBizMapper(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<ContextMenuDAL, ContextMenu>(MemberList.Source)
                .ForMember(biz => biz.Items, opt => opt.MapFrom(x => MapperFacade.ContextMenuItemMapper.GetBizList(x.Items.ToList())));
        }
    }
}
