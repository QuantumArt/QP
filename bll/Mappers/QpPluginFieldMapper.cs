using System;
using AutoMapper;
using Quantumart.QP8.BLL.Enums;
using Quantumart.QP8.BLL.Services.VisualEditor;
using Quantumart.QP8.DAL;
using Quantumart.QP8.DAL.Entities;

namespace Quantumart.QP8.BLL.Mappers.VisualEditor
{
    internal class QpPluginFieldMapper : GenericMapper<QpPluginField, PluginFieldDAL>
    {
        public override void CreateDalMapper(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<QpPluginField, PluginFieldDAL>()
                .ForMember(data => data.RelationType, opt => opt.MapFrom(biz => biz.RelationType.ToString()))
                .ForMember(data => data.ValueType, opt => opt.MapFrom(biz => biz.ValueType.ToString()))
                ;
        }

        public override void CreateBizMapper(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<PluginFieldDAL, QpPluginField>()
                .ForMember(biz => biz.RelationType, opt => opt.MapFrom(data => Enum.Parse(typeof(QpPluginRelationType), data.RelationType)))
                .ForMember(biz => biz.ValueType, opt => opt.MapFrom(data => Enum.Parse(typeof(QpPluginValueType), data.ValueType)))
                ;
        }
    }
}
