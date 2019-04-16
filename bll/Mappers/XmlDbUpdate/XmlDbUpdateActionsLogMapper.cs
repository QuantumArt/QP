using AutoMapper;
using Quantumart.QP8.BLL.Models.XmlDbUpdate;
using Quantumart.QP8.DAL;
using Quantumart.QP8.DAL.Entities;

namespace Quantumart.QP8.BLL.Mappers.XmlDbUpdate
{
    internal sealed class XmlDbUpdateActionsLogMapper : GenericMapper<XmlDbUpdateActionsLogModel, XmlDbUpdateActionsLogEntity>
    {
        public XmlDbUpdateActionsLogMapper(IMapperConfigurationExpression cfg)
        {
            CreateDalMapper(cfg);
        }
    }
}
