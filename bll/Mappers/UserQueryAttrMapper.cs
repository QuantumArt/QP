using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.DAL;
using AutoMapper;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.BLL.Mappers
{
	internal class UserQueryAttrMapper : GenericMapper<UserQueryAttr, UserQueryAttrsDAL>
	{
		public override void CreateBizMapper()
		{
			Mapper.CreateMap<UserQueryAttrsDAL, UserQueryAttr>()
				.ForMember(biz => biz.BaseFieldId, opt => opt.MapFrom(r => Converter.ToInt32(r.UserQueryAttrId)))
				.ForMember(biz => biz.UserQueryContentId, opt => opt.MapFrom(r => Converter.ToInt32(r.VirtualContentId)));
		}

		public override void CreateDalMapper()
		{
			Mapper.CreateMap<UserQueryAttr, UserQueryAttrsDAL>()
				.ForMember(data => data.UserQueryAttrId, opt => opt.MapFrom(biz => Converter.ToDecimal(biz.BaseFieldId)))
				.ForMember(data => data.VirtualContentId, opt => opt.MapFrom(biz => Converter.ToDecimal(biz.UserQueryContentId)));
		}
	}
}
