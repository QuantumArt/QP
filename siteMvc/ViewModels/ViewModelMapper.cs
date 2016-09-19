﻿using System;
using AutoMapper;
using Quantumart.QP8.BLL;
using Quantumart.QP8.Utils;
using Quantumart.QP8.WebMvc.ViewModels.ArticleVersion;

namespace Quantumart.QP8.WebMvc.ViewModels
{
    public class ViewModelMapper
    {
        public static void CreateAllMappings()
        {
            Mapper.CreateMap<DateTime, string>().ConvertUsing(src => src.ValueToDisplay());
            Mapper.CreateMap<User, string>().ConvertUsing(src => src == null ? "" : src.LogOn);
            Mapper.CreateMap<BLL.ArticleVersion, ArticleVersionListItem>();
        }
    }
}
