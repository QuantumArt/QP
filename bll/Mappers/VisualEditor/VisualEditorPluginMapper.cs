using AutoMapper;
using Quantumart.QP8.BLL.Services.VisualEditor;
using Quantumart.QP8.DAL;

namespace Quantumart.QP8.BLL.Mappers.VisualEditor
{
    internal class VisualEditorPluginMapper : GenericMapper<VisualEditorPlugin, VePluginDAL>
    {
        public override void CreateDalMapper()
        {
            Mapper.CreateMap<VisualEditorPlugin, VePluginDAL>()
                .ForMember(data => data.LastModifiedByUser, opt => opt.Ignore())
                .ForMember(data => data.VeCommands, opt => opt.Ignore());
        }

        public override void CreateBizMapper()
        {
            Mapper.CreateMap<VePluginDAL, VisualEditorPlugin>();
        }
    }
}
