using System;
using System.Data;
using AutoMapper;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.BLL.Mappers
{
    internal class PageRowMapper : GenericMapper<PageListItem, DataRow>
    {
        public override void CreateBizMapper(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<DataRow, PageListItem>(MemberList.Source)
                .ForMember(biz => biz.Id, opt => opt.MapFrom(row => Converter.ToInt32(row.Field<decimal>("Id"))))
                .ForMember(biz => biz.Name, opt => opt.MapFrom(row => row.Field<string>("Name")))
                .ForMember(biz => biz.Folder, opt => opt.MapFrom(row => "\\" + row.Field<string>("Folder")))
                .ForMember(biz => biz.Description, opt => opt.MapFrom(row => row.Field<string>("Description")))
                .ForMember(biz => biz.Created, opt => opt.MapFrom(row => row.Field<DateTime>("Created")))
                .ForMember(biz => biz.Modified, opt => opt.MapFrom(row => row.Field<DateTime>("Modified")))
                .ForMember(biz => biz.LastModifiedBy, opt => opt.MapFrom(row => Converter.ToInt32(row.Field<decimal>("LastModifiedBy"))))
                .ForMember(biz => biz.LastModifiedByLogin, opt => opt.MapFrom(row => row.Field<string>("LastModifiedByLogin")))
                .ForMember(biz => biz.Assembled, opt => opt.MapFrom(row => row.Field<DateTime>("Assembled")))
                .ForMember(biz => biz.AssembledBy, opt => opt.MapFrom(row => row.Field<decimal>("LastAssembledBy")))
                .ForMember(biz => biz.AssembledByLogin, opt => opt.MapFrom(row => row.Field<string>("LastAssembledByLogin")))
                .ForMember(biz => biz.FileName, opt => opt.MapFrom(row => row.Field<string>("FileName")))
                .ForMember(biz => biz.Reassemble, opt => opt.MapFrom(row => Converter.ToBoolean(row.Field<decimal>("Reassemble"))))
                .ForMember(biz => biz.GenerateTrace, opt => opt.MapFrom(row => row.Field<bool>("GenerateTrace")))
                .ForMember(biz => biz.LockedBy, opt => opt.MapFrom(row => Converter.ToInt32(row.Field<decimal?>("LockedBy"), 0)))
                .ForMember(biz => biz.LockedByFullName, opt => opt.MapFrom(row => Converter.ToString(row.Field<string>("LockedByFullName"), string.Empty)))
                .ForMember(biz => biz.TemplateName, opt => opt.MapFrom(row => Converter.ToString(row.Field<string>("TemplateName"), string.Empty)))
                .AfterMap(SetBizProperties);
        }

        private static void SetBizProperties(DataRow dataObject, PageListItem bizObject)
        {
            bizObject.LockedByIcon = LockableEntityObject.GetLockedByIcon(bizObject.LockedBy);
            bizObject.LockedByToolTip = LockableEntityObject.GetLockedByToolTip(bizObject.LockedBy, bizObject.LockedByFullName);
        }
    }
}
