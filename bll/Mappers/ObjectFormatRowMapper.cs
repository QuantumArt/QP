using System;
using System.Data;
using AutoMapper;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.BLL.Mappers
{
    internal class ObjectFormatRowMapper : GenericMapper<ObjectFormatListItem, DataRow>
    {
        public override void CreateBizMapper()
        {
            Mapper.CreateMap<DataRow, ObjectFormatListItem>()
                .ForMember(biz => biz.Id, opt => opt.MapFrom(row => Converter.ToInt32(row.Field<decimal>("Id"))))
                .ForMember(biz => biz.Name, opt => opt.MapFrom(row => row.Field<string>("Name")))
                .ForMember(biz => biz.Description, opt => opt.MapFrom(row => row.Field<string>("Description")))
                .ForMember(biz => biz.Created, opt => opt.MapFrom(row => row.Field<DateTime>("Created")))
                .ForMember(biz => biz.Modified, opt => opt.MapFrom(row => row.Field<DateTime>("Modified")))
                .ForMember(biz => biz.LastModifiedBy, opt => opt.MapFrom(row => Converter.ToInt32(row.Field<decimal>("LastModifiedBy"))))
                .ForMember(biz => biz.LastModifiedByLogin, opt => opt.MapFrom(row => row.Field<string>("LastModifiedByLogin")))
                .ForMember(biz => biz.LockedBy, opt => opt.MapFrom(row => Converter.ToInt32(row.Field<decimal?>("LockedBy"), 0)))
                .ForMember(biz => biz.LockedByFullName, opt => opt.MapFrom(row => Converter.ToString(row.Field<string>("LockedByFullName"), string.Empty)))
                .AfterMap(SetBizProperties);
        }

        private static void SetBizProperties(DataRow dataObject, ObjectFormatListItem bizObject)
        {
            bizObject.IsDefault = Converter.ToInt32(dataObject.Field<decimal?>("FormatIdToCheck"), 0) == bizObject.Id;
            bizObject.LockedByIcon = LockableEntityObject.GetLockedByIcon(bizObject.LockedBy);
            bizObject.LockedByToolTip = LockableEntityObject.GetLockedByToolTip(bizObject.LockedBy, bizObject.LockedByFullName);
        }
    }
}
