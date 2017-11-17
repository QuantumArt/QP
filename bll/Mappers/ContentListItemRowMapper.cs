using System;
using System.Data;
using AutoMapper;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.BLL.Mappers
{
    internal class ContentListItemRowMapper : GenericMapper<ContentListItem, DataRow>
    {
        public override void CreateBizMapper()
        {
            Mapper.CreateMap<DataRow, ContentListItem>()
                .ForMember(biz => biz.Id, opt => opt.MapFrom(row => Converter.ToInt32(row.Field<decimal>("Id"))))
                .ForMember(biz => biz.Name, opt => opt.MapFrom(row => row.Field<string>("Name")))
                .ForMember(biz => biz.GroupName, opt => opt.MapFrom(row => Converter.ToString(row.Field<string>("GroupName"), ContentStrings.DefaultContentGroup)))
                .ForMember(biz => biz.SiteName, opt => opt.MapFrom(row => row.Field<string>("SiteName")))
                .ForMember(biz => biz.Description, opt => opt.MapFrom(row => Converter.ToString(row.Field<string>("Description"), string.Empty)))
                .ForMember(biz => biz.VirtualType, opt => opt.MapFrom(row => Content.GetVirtualTypeString(Converter.ToInt32(row.Field<decimal>("VirtualType")))))
                .ForMember(biz => biz.Created, opt => opt.MapFrom(row => row.Field<DateTime>("Created")))
                .ForMember(biz => biz.Modified, opt => opt.MapFrom(row => row.Field<DateTime>("Modified")))
                .ForMember(biz => biz.LastModifiedByUser, opt => opt.MapFrom(row => row.Field<string>("LastModifiedByUser")))
                .AfterMap(SetBizProperties);
        }

        public void SetBizProperties(DataRow dal, ContentListItem biz)
        {
            if (biz.GroupName == ContentGroup.DefaultName)
            {
                biz.GroupName = ContentGroup.TranslatedDefaultName;
            }
        }
    }
}
