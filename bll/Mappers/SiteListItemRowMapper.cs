using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.BLL.ListItems;
using System.Data;
using Quantumart.QP8.Utils;
using AutoMapper;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Constants;

namespace Quantumart.QP8.BLL.Mappers
{
    internal class SiteListItemRowMapper : GenericMapper<SiteListItem, DataRow>
    {
        public override void CreateBizMapper()
        {
            Mapper.CreateMap<DataRow, SiteListItem>()
                .ForMember(biz => biz.Id, opt => opt.MapFrom(row => Converter.ToInt32(row.Field<decimal>("Id"))))
                .ForMember(biz => biz.Name, opt => opt.MapFrom(row => row.Field<string>("Name")))
                .ForMember(biz => biz.Description, opt => opt.MapFrom(row => Converter.ToString(row.Field<string>("Description"), String.Empty)))
                .ForMember(biz => biz.Created, opt => opt.MapFrom(row => row.Field<DateTime>("Created")))
                .ForMember(biz => biz.Modified, opt => opt.MapFrom(row => row.Field<DateTime>("Modified")))
                .ForMember(biz => biz.LastModifiedByUser, opt => opt.MapFrom(row => row.Field<string>("LastModifiedByUser")))
                .ForMember(biz => biz.LockedBy, opt => opt.MapFrom(row => Converter.ToInt32(row.Field<decimal?>("LockedBy"), 0)))
                .ForMember(biz => biz.LockedByFullName, opt => opt.MapFrom(row => Converter.ToString(row.Field<string>("LockedByFullName"), String.Empty)))

                .ForMember(biz => biz.IsLive, opt => opt.MapFrom(row => Converter.ToInt32(row.Field<string>("IsLive"))))
                .ForMember(biz => biz.Dns, opt => opt.MapFrom(row => row.Field<string>("Dns")))
                .ForMember(biz => biz.UploadUrl, opt => opt.MapFrom(row => row.Field<string>("UploadUrl")))
                .AfterMap(SetBizProperties);
        }


        private static void SetBizProperties(DataRow dataObject, SiteListItem bizObject)
        {
            bizObject.LockedByIcon = Site.GetLockedByIcon(bizObject.LockedBy);
            bizObject.LockedByToolTip = Site.GetLockedByToolTip(bizObject.LockedBy, bizObject.LockedByFullName);
        }
    }
}
