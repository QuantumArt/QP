using System;
using System.Data;
using AutoMapper;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.BLL.Mappers
{
    internal class NotificationListItemRowMapper : GenericMapper<NotificationListItem, DataRow>
    {
        public override void CreateBizMapper(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<DataRow, NotificationListItem>(MemberList.Source)
                .ForMember(biz => biz.Created, opt => opt.MapFrom(row => row.Field<DateTime>("Created")))
                .ForMember(biz => biz.ForCreate, opt => opt.MapFrom(row => row.Field<bool>("ForCreate")))
                .ForMember(biz => biz.ForFrontend, opt => opt.MapFrom(row => row.Field<bool>("ForFrontend")))
                .ForMember(biz => biz.ForModify, opt => opt.MapFrom(row => row.Field<bool>("ForModify")))
                .ForMember(biz => biz.ForRemove, opt => opt.MapFrom(row => row.Field<bool>("ForRemove")))
                .ForMember(biz => biz.ForStatusChanged, opt => opt.MapFrom(row => row.Field<bool>("ForStatusChanged")))
                .ForMember(biz => biz.ForStatusPartiallyChanged, opt => opt.MapFrom(row => row.Field<bool>("ForStatusPartiallyChanged")))
                .ForMember(biz => biz.Id, opt => opt.MapFrom(row => Converter.ToInt32(row.Field<decimal>("Id"))))
                .ForMember(biz => biz.LastModifiedBy, opt => opt.MapFrom(row => Converter.ToInt32(row.Field<decimal>("LastModifiedBy"))))
                .ForMember(biz => biz.LastModifiedByLogin, opt => opt.MapFrom(row => row.Field<string>("LastModifiedByLogin")))
                .ForMember(biz => biz.Modified, opt => opt.MapFrom(row => row.Field<DateTime>("Modified")))
                .ForMember(biz => biz.Name, opt => opt.MapFrom(row => row.Field<string>("Name")))
                .ForMember(biz => biz.NoEmail, opt => opt.MapFrom(row => Converter.ToBoolean(row.Field<decimal>("no_email"))))
                .ForMember(biz => biz.FieldId, opt => opt.MapFrom(row => Converter.ToNullableInt32(row.Field<decimal?>("email_attribute_id"))))
                .ForMember(biz => biz.Receiver, opt => opt.MapFrom(row => row.Field<string>("Receiver")))
                .ForMember(biz => biz.IsExternal, opt => opt.MapFrom(row => Converter.ToBoolean(row.Field<bool>("IsExternal"))))
                .ForMember(biz => biz.ForDelayedPublication, opt => opt.MapFrom(row => row.Field<bool>("ForDelayedPublication")))
                .AfterMap(SetBizProperties)
                ;
        }

        private static void SetBizProperties(DataRow dataObject, NotificationListItem bizObject)
        {
            if (!bizObject.IsExternal)
            {
                if (bizObject.NoEmail)
                {
                    bizObject.Receiver = NotificationStrings.RadioNone;
                }
                else if (string.IsNullOrEmpty(bizObject.Receiver))
                {
                    bizObject.Receiver = NotificationStrings.RadioEveryone;
                }
                if (bizObject.FieldId.HasValue)
                {
                    bizObject.Receiver = NotificationStrings.Field + " : " + bizObject.Receiver;
                }
            }
            else
            {
                bizObject.Receiver = string.Empty;
            }
        }
    }
}
