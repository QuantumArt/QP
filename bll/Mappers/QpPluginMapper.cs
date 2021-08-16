using AutoMapper;
using Quantumart.QP8.BLL.Services.VisualEditor;
using Quantumart.QP8.DAL;
using Quantumart.QP8.DAL.Entities;

namespace Quantumart.QP8.BLL.Mappers.VisualEditor
{
    internal class QpPluginMapper : GenericMapper<QpPlugin, PluginDAL>
    {
        public override void CreateDalMapper(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<QpPlugin, PluginDAL>()
                .ForMember(data => data.LastModifiedByUser, opt => opt.Ignore())
                ;
        }

        public override void CreateBizMapper(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<PluginDAL, QpPlugin>()
                .ForMember(biz => biz.OldContract, opt => opt.MapFrom(n => n.Contract))
                ;
        }
    }
}
