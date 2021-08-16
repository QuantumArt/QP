using AutoMapper;
using Quantumart.QP8.BLL.Services.VisualEditor;
using Quantumart.QP8.DAL;
using Quantumart.QP8.DAL.Entities;

namespace Quantumart.QP8.BLL.Mappers.VisualEditor
{
    internal class QpPluginVersionMapper : GenericMapper<QpPluginVersion, PluginVersionDAL>
    {
        public override void CreateDalMapper(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<QpPluginVersion, PluginVersionDAL>()
                .ForMember(data => data.LastModifiedByUser, opt => opt.Ignore())
                .ForMember(data => data.Plugin, opt => opt.Ignore())
                ;
        }

        public override void CreateBizMapper(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<PluginVersionDAL, QpPluginVersion>();
        }
    }
}
