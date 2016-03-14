using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using AutoMapper;

namespace Quantumart.QP8.BLL.Mappers
{
	internal class ArticleRowMapper : GenericMapper<Article, DataRow>
	{
		public override void CreateBizMapper()
		{
			Mapper.CreateMap<DataRow, Article>()
				.ForMember(biz => biz.Id, opt => opt.MapFrom(row => row.Field<decimal>("CONTENT_ITEM_ID")))
				.ForMember(biz => biz.StatusTypeId, opt => opt.MapFrom(row => row.Field<decimal>("STATUS_TYPE_ID")))
				.ForMember(biz => biz.Visible, opt => opt.MapFrom(row => row.Field<decimal>("VISIBLE")))
				.ForMember(biz => biz.Archived, opt => opt.MapFrom(row => row.Field<decimal>("ARCHIVE")))
				.ForMember(biz => biz.LastModifiedBy, opt => opt.MapFrom(row => row.Field<decimal>("LAST_MODIFIED_BY")))
				.ForMember(biz => biz.Created, opt => opt.MapFrom(row => row.Field<DateTime>("CREATED")))
				.ForMember(biz => biz.Modified, opt => opt.MapFrom(row => row.Field<DateTime>("MODIFIED")));
		
		}
	}
}
