using AutoMapper;
using Flurl;
using QP8.Infrastructure.Extensions;
using Quantumart.QP8.BLL.Models.NotificationSender;
using Quantumart.QP8.CdcDataImport.Elastic.Infrastructure.Data;
using Quantumart.QP8.CdcDataImport.Elastic.Properties;

namespace Quantumart.QP8.CdcDataImport.Elastic.Infrastructure
{
    public class ElasticMapperProfile : Profile
    {
        public ElasticMapperProfile()
        {
            CreateMap<CdcEntityModel, CdcEntityModel>();
            CreateMap<CdcTableTypeModel, CdcTableTypeModel>();

            CreateMap<CdcEntityModel, CdcEntityDto>();
            CreateMap<CdcTableTypeModel, CdcDataTableDto>()
                .ForMember(dest => dest.CustomerCode, config => config.Ignore());

            CreateMap<CdcDataTableDto, SystemNotificationModel>()
                .ForMember(dest => dest.Id, config => config.Ignore())
                .ForMember(dest => dest.CdcLastExecutedLsnId, config => config.Ignore())
                .ForMember(dest => dest.Created, config => config.Ignore())
                .ForMember(dest => dest.Modified, config => config.Ignore())
                .ForMember(dest => dest.Tries, config => config.Ignore())
                .ForMember(dest => dest.Sent, config => config.Ignore())
                .ForMember(dest => dest.TransactionLsn, config => config.MapFrom(src => src.TransactionLsn))
                .ForMember(dest => dest.TransactionDate, config => config.MapFrom(src => src.TransactionDate))
                .ForMember(dest => dest.Url, config => config.MapFrom(src => Url.Combine(Settings.Default.HttpEndpoint, src.CustomerCode, "contentData")))
                .ForMember(dest => dest.Json, config => config.MapFrom(src => src.Entity.Columns.ToJsonLog(null)));
        }
    }
}
