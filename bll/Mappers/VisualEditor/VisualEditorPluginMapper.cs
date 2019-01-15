using AutoMapper;
using Quantumart.QP8.BLL.Services.VisualEditor;
using Quantumart.QP8.DAL;

namespace Quantumart.QP8.BLL.Mappers.VisualEditor
{
    internal class VisualEditorPluginMapper : GenericMapper<VisualEditorPlugin, VePluginDAL>
    {
        public override void CreateDalMapper(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<VisualEditorPlugin, VePluginDAL>()
                .ForMember(data => data.LastModifiedByUser, opt => opt.Ignore())
                .ForMember(data => data.VeCommands, opt => opt.Ignore());
        }

        public override void CreateBizMapper(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<VePluginDAL, VisualEditorPlugin>();
        }
    }
}
