using AutoMapper;
using Quantumart.QP8.BLL.Services.VisualEditor;
using Quantumart.QP8.DAL;
using Quantumart.QP8.DAL.Entities;

namespace Quantumart.QP8.BLL.Mappers.VisualEditor
{
    internal class VisualEditorCommandMapper : GenericMapper<VisualEditorCommand, VeCommandDAL>
    {
        public override void CreateDalMapper(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<VisualEditorCommand, VeCommandDAL>().ForMember(data => data.LastModifiedByUser, opt => opt.Ignore()).ForMember(data => data.VePlugin, opt => opt.Ignore());
        }

        public override void CreateBizMapper(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<VeCommandDAL, VisualEditorCommand>().ForMember(biz => biz.Alias, opt => opt.MapFrom(data => Translator.Translate(data.Alias)));
        }
    }
}
