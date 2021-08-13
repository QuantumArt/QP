using System;
using AutoMapper;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using QP8.Plugins.Contract;
using Quantumart.QP8.BLL.Enums;
using Quantumart.QP8.DAL.Entities;

namespace Quantumart.QP8.BLL.Mappers.VisualEditor
{
    internal class QpPluginFieldMapper : GenericMapper<QpPluginField, PluginFieldDAL>
    {
        public T Read<T>(string data)
        {
            var reader = new JTokenReader(new JValue(data));
            reader.Read();
            var result = (T)new StringEnumConverter().ReadJson(reader, typeof(T), null, null);
            return result;
        }

        public string Write(object data)
        {
            var arr = new JArray();
            var writer = new JTokenWriter(arr);
            new StringEnumConverter().WriteJson(writer, data, null);
            var result = arr.First?.ToString();
            return result;
        }

        public override void CreateDalMapper(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<QpPluginField, PluginFieldDAL>()
                .ForMember(data => data.RelationType, opt => opt.MapFrom(biz => Write(biz.RelationType)))
                .ForMember(data => data.ValueType, opt => opt.MapFrom(biz => Write(biz.ValueType)))
                ;
        }

        public override void CreateBizMapper(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<PluginFieldDAL, QpPluginField>()
                .ForMember(biz => biz.RelationType, opt => opt.MapFrom(data => Read<QpPluginRelationType>(data.RelationType)))
                .ForMember(biz => biz.ValueType, opt => opt.MapFrom(data => Read<QpPluginValueType>(data.ValueType)))
                ;
        }
    }
}
