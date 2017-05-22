using AutoMapper;
using QP8.Infrastructure.Extensions;
using Quantumart.QP8.BLL.Models.NotificationSender;
using Quantumart.QP8.CdcDataImport.Tarantool.Infrastructure.Data;
using Quantumart.QP8.CdcDataImport.Tarantool.Properties;

namespace Quantumart.QP8.CdcDataImport.Tarantool.Infrastructure
{
    public class TarantoolMapperProfile : Profile
    {
        protected override void Configure()
        {
            CreateMap<CdcEntityModel, CdcEntityDto>();
            CreateMap<CdcDataTableDto, SystemNotificationModel>()
                .ForMember(dest => dest.TransactionLsn, config => config.MapFrom(src => src.TransactionLsn))
                .ForMember(dest => dest.TransactionDate, config => config.MapFrom(src => src.TransactionDate))
                .ForMember(dest => dest.Url, config => config.MapFrom(src => Settings.Default.HttpEndpoint))
                .ForMember(dest => dest.Json, config => config.MapFrom(src => src.ToJsonLog(null)));
        }
    }
}
