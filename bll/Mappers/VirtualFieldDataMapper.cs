using System.Data;
using AutoMapper;
using Quantumart.QP8.BLL.Repository.Results;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.BLL.Mappers
{
    internal class VirtualFieldDataMapper : GenericMapper<VirtualFieldData, DataRow>
    {
        public override void CreateBizMapper(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<DataRow, VirtualFieldData>(MemberList.Source)
                .ForMember(biz => biz.Id, opt => opt.MapFrom(r => Converter.ToInt32(r.Field<decimal>("Id"))))
                .ForMember(biz => biz.JoinId, opt => opt.MapFrom(r => Converter.ToNullableInt32(r.Field<decimal?>("JoinId"))))
                .ForMember(biz => biz.PersistentContentId, opt => opt.MapFrom(r => Converter.ToInt32(r.Field<decimal>("PersistentContentId"))))
                .ForMember(biz => biz.PersistentId, opt => opt.MapFrom(r => Converter.ToInt32(r.Field<decimal>("PersistentId"))))
                .ForMember(biz => biz.RelateToPersistentContentId, opt => opt.MapFrom(r => Converter.ToNullableInt32(r.Field<decimal?>("RelateToPersistentContentId"))))
                .ForMember(biz => biz.Type, opt => opt.MapFrom(r => Converter.ToInt32(r.Field<decimal>("Type"))))
                .ForMember(biz => biz.Name, opt => opt.MapFrom(r => r.Field<string>("Name")))
                .ForMember(biz => biz.PersistentName, opt => opt.MapFrom(r => r.Field<string>("PersistentName")));
        }
    }
}
