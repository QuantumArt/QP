using System.Data;
using System.Linq;
using AutoMapper;
using Quantumart.QP8.BLL.Facades;

namespace Quantumart.QP8.BLL.Mappers
{
    internal class ContextMenuRowMapper : GenericMapper<ContextMenu, DataRow>
    {
        public override void CreateBizMapper()
        {
            Mapper.CreateMap<DataRow, ContextMenu>()
                .ForMember(biz => biz.Id, opt => opt.MapFrom(row => row.Field<int>("ID")))
                .ForMember(biz => biz.Code, opt => opt.MapFrom(row => row.Field<string>("CODE")))
                .ForMember(biz => biz.Items, opt => opt.MapFrom(row => MapperFacade.ContextMenuItemRowMapper.GetBizList(row.GetChildRows("Menu2Item").ToList())));
        }
    }
}
