using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoMapper;
using Quantumart.QP8.DAL;

namespace Quantumart.QP8.BLL.Mappers
{
    internal class ViewTypeMapper : GenericMapper<ViewType, ViewTypeDAL>
    {
        public override void CreateBizMapper()
        {
            Mapper.CreateMap<ViewTypeDAL, ViewType>()
                .ForMember(biz => biz.Name, opt => opt.MapFrom(data => Translator.Translate(data.Name)));
        }
    }

}
