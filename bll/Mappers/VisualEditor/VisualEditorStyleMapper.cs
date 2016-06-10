using AutoMapper;
using Quantumart.QP8.BLL.Services.VisualEditor;
using Quantumart.QP8.DAL;

namespace Quantumart.QP8.BLL.Mappers.VisualEditor
{
    internal class VisualEditorStyleMapper : GenericMapper<VisualEditorStyle, VeStyleDAL>
    {
        public override void CreateDalMapper()
        {
            Mapper.CreateMap<VisualEditorStyle, VeStyleDAL>().ForMember(data => data.LastModifiedByUser, opt => opt.Ignore());
        }

        public override void CreateBizMapper()
        {
            Mapper.CreateMap<VeStyleDAL, VisualEditorStyle>().AfterMap(SetBizProperties);
        }

        private static void SetBizProperties(VeStyleDAL dataObject, VisualEditorStyle bizObject)
        {
            bizObject.Init();
        }
    }
}
