using AutoMapper;
using QP8.Infrastructure.Extensions;
using QP8.Infrastructure.Web.Responses;
using Quantumart.QP8.BLL.Models.NotificationSender;
using Quantumart.QP8.CdcDataImport.Tarantool.Infrastructure.Data;
using Quantumart.QP8.CdcDataImport.Tarantool.Properties;
using Quantumart.QP8.Constants.Cdc.Tarantool;

namespace Quantumart.QP8.CdcDataImport.Tarantool.Infrastructure
{
    public class TarantoolMapperProfile : Profile
    {
        protected override void Configure()
        {
            CreateMap<CdcEntityModel, CdcEntityModel>();
            CreateMap<CdcTableTypeModel, CdcTableTypeModel>();

            CreateMap<CdcEntityModel, CdcEntityDto>();
            CreateMap<CdcEntityModel, CdcArticleEntityDto>()
                .ForMember(dest => dest.IsSplitted, config => config.MapFrom(src => (bool)src.MetaData[TarantoolContentArticleModel.IsSplitted]));

            CreateMap<CdcDataTableDto, SystemNotificationModel>()
                .ForMember(dest => dest.Id, config => config.Ignore())
                .ForMember(dest => dest.CdcLastExecutedLsnId, config => config.Ignore())
                .ForMember(dest => dest.Created, config => config.Ignore())
                .ForMember(dest => dest.Modified, config => config.Ignore())
                .ForMember(dest => dest.Tries, config => config.Ignore())
                .ForMember(dest => dest.Sent, config => config.Ignore())
                .ForMember(dest => dest.TransactionLsn, config => config.MapFrom(src => src.TransactionLsn))
                .ForMember(dest => dest.TransactionDate, config => config.MapFrom(src => src.TransactionDate))
                .ForMember(dest => dest.Url, config => config.MapFrom(src => Settings.Default.HttpEndpoint))
                .ForMember(dest => dest.Json, config => config.MapFrom(src => new NginxRequest(src).ToJsonLog(null)));
        }
    }
}
