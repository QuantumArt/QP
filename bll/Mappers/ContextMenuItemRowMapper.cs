using System.Data;
using AutoMapper;

namespace Quantumart.QP8.BLL.Mappers
{
	internal class ContextMenuItemRowMapper : GenericMapper<ContextMenuItem, DataRow>
	{		
		public override void CreateBizMapper()
		{
			Mapper.CreateMap<DataRow, ContextMenuItem>()
				.ForMember(biz => biz.ActionCode, opt => opt.MapFrom(row => row.Field<string>("ACTION_CODE")))
				.ForMember(biz => biz.ActionTypeCode, opt => opt.MapFrom(row => row.Field<string>("ACTION_TYPE_CODE")))
				.ForMember(biz => biz.Name, opt => opt.MapFrom(row => row.Field<string>("NAME")))
				.ForMember(biz => biz.Icon, opt => opt.MapFrom(row => row.Field<string>("ICON")))
				.ForMember(biz => biz.BottomSeparator, opt => opt.MapFrom(row => row.Field<bool>("BOTTOM_SEPARATOR")));
		}
	}
}
