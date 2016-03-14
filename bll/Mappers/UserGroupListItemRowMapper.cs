using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.BLL.Services.DTO;
using System.Data;
using Quantumart.QP8.Utils;
using AutoMapper;

namespace Quantumart.QP8.BLL.Mappers
{
	class UserGroupListItemRowMapper : GenericMapper<UserGroupListItem, DataRow>
	{
		public override void CreateBizMapper()
		{
			Mapper.CreateMap<DataRow, UserGroupListItem>()
				.ForMember(biz => biz.Id, opt => opt.MapFrom(row => Converter.ToInt32(row.Field<decimal>("ID"))))
				.ForMember(biz => biz.Name, opt => opt.MapFrom(row => row.Field<string>("Name")))
				.ForMember(biz => biz.Description, opt => opt.MapFrom(row => row.Field<string>("Description")))
				.ForMember(biz => biz.SharedArticles, opt => opt.MapFrom(row => Converter.ToBoolean(row.Field<decimal>("SharedArticles"))))
				.ForMember(biz => biz.Created, opt => opt.MapFrom(row => row.Field<DateTime>("CREATED")))
				.ForMember(biz => biz.Modified, opt => opt.MapFrom(row => row.Field<DateTime>("MODIFIED")))
				.ForMember(biz => biz.LastModifiedByUserId, opt => opt.MapFrom(row => Converter.ToInt32(row.Field<decimal>("LastModifiedByUserId"))))
				.ForMember(biz => biz.LastModifiedByUser, opt => opt.MapFrom(row => row.Field<string>("LastModifiedByUser")));
		}
	}
}
