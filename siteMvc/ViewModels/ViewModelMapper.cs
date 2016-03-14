using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AutoMapper;
using Quantumart.QP8.BLL.ListItems;
using B = Quantumart.QP8.BLL;
using Quantumart.QP8.Utils;
using Quantumart.QP8.WebMvc.ViewModels.Field;


namespace Quantumart.QP8.WebMvc.ViewModels
{
    public class ViewModelMapper
    {
        public static void CreateAllMappings()
        {
            Mapper.CreateMap<DateTime, string>().ConvertUsing(src => src.ValueToDisplay());
            Mapper.CreateMap<B.User, string>().ConvertUsing(src => (src == null) ? "" : src.LogOn);
            Mapper.CreateMap<B.ArticleVersion, ArticleVersionListItem>();
        }

    }
}
