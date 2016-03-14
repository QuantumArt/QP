using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using AutoMapper;

namespace Quantumart.QP8.BLL.Mappers
{
    internal class ArticleListItemRowMapper : GenericMapper<ArticleListItem, DataRow>
    {
        public override void CreateBizMapper()
        {
            Mapper.CreateMap<DataRow, ArticleListItem>()
                .ForMember(biz => biz.Id, opt => opt.MapFrom(row => row.Field<decimal>("ID")))
				.ForMember(biz => biz.ParentId, opt => opt.MapFrom(row => row.Field<decimal>("ParentId")))
                .ForMember(biz => biz.StatusName, opt => opt.MapFrom(row => row.Field<string>("StatusName")))
                .ForMember(biz => biz.SiteName, opt => opt.MapFrom(row => row.Field<string>("SiteName")))
                .ForMember(biz => biz.ContentName, opt => opt.MapFrom(row => row.Field<string>("ContentName")))
                .ForMember(biz => biz.LastModifiedByUser, opt => opt.MapFrom(row => row.Field<string>("LastModifiedByUser")))
                .ForMember(biz => biz.Created, opt => opt.MapFrom(row => row.Field<DateTime>("Created")))
                .ForMember(biz => biz.Modified, opt => opt.MapFrom(row => row.Field<DateTime>("Modified")))
                .ForMember(biz => biz.Title, opt => opt.MapFrom(row => row.Field<string>("Title")))
                .ForMember(biz => biz.IsPermanentLock, opt => opt.MapFrom(row => row.Field<bool>("IsPermanentLock")));

        }
    }
}
