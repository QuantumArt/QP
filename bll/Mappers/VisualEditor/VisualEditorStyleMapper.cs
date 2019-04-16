using AutoMapper;
using Quantumart.QP8.BLL.Services.VisualEditor;
using Quantumart.QP8.DAL;
using Quantumart.QP8.DAL.Entities;

namespace Quantumart.QP8.BLL.Mappers.VisualEditor
{
    internal class VisualEditorStyleMapper : GenericMapper<VisualEditorStyle, VeStyleDAL>
    {
        public override void CreateDalMapper(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<VisualEditorStyle, VeStyleDAL>().ForMember(data => data.LastModifiedByUser, opt => opt.Ignore());
        }

        public override void CreateBizMapper(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<VeStyleDAL, VisualEditorStyle>().AfterMap(SetBizProperties);
        }

        private static void SetBizProperties(VeStyleDAL dataObject, VisualEditorStyle bizObject)
        {
            bizObject.Init();
        }
    }
}
