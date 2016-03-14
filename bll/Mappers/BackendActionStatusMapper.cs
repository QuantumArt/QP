using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using AutoMapper;

namespace Quantumart.QP8.BLL.Mappers
{
	internal class BackendActionStatusMapper : GenericMapper<BackendActionStatus, DataRow>
	{
		public override void CreateBizMapper()
		{
			Mapper.CreateMap<DataRow, BackendActionStatus>()
				.ForMember(biz => biz.Code, opt => opt.MapFrom(row => row.Field<string>("CODE")))
				.ForMember(biz => biz.Visible, opt => opt.MapFrom(row => row.Field<bool>("VISIBLE")));
		}
	}
}
