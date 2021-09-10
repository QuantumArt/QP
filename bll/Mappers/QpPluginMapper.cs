using AutoMapper;
using Quantumart.QP8.DAL.Entities;

namespace Quantumart.QP8.BLL.Mappers
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
                .ForMember(biz => biz.OldLastModifiedBy, opt => opt.MapFrom(n => n.LastModifiedBy))
                .ForMember(biz => biz.OldModified, opt => opt.MapFrom(n => n.Modified))
                ;
        }
    }
}
