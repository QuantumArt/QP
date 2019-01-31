using System;
using AutoMapper;
using Quantumart.QP8.Utils;
using Quantumart.QP8.WebMvc.ViewModels.ArticleVersion;

namespace Quantumart.QP8.WebMvc.ViewModels
{
    public class ViewModelMapper
    {
        public static void CreateAllMappings(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<DateTime, string>().ConvertUsing(src => src.ValueToDisplay());
            cfg.CreateMap<BLL.User, string>().ConvertUsing(src => src == null ? string.Empty : src.LogOn);
            cfg.CreateMap<BLL.ArticleVersion, ArticleVersionListItem>();
        }
    }
}
