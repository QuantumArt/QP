using AutoMapper;
using Quantumart.QP8.BLL.Services.VisualEditor;
using Quantumart.QP8.DAL;

namespace Quantumart.QP8.BLL.Mappers.VisualEditor
{
    internal class VisualEditorCommandMapper : GenericMapper<VisualEditorCommand, VeCommandDAL>
    {
        public override void CreateDalMapper()
        {
            Mapper.CreateMap<VisualEditorCommand, VeCommandDAL>().ForMember(data => data.LastModifiedByUser, opt => opt.Ignore()).ForMember(data => data.VePlugin, opt => opt.Ignore());
        }

        public override void CreateBizMapper()
        {
            Mapper.CreateMap<VeCommandDAL, VisualEditorCommand>().ForMember(biz => biz.Alias, opt => opt.MapFrom(data => Translator.Translate(data.Alias)));
        }
    }
}
