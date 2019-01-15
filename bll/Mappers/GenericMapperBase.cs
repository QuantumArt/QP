using AutoMapper;

namespace Quantumart.QP8.BLL.Mappers
{
    internal abstract class GenericMapper
    {

        public abstract void CreateBizMapper(IMapperConfigurationExpression cfg);

        public abstract void CreateDalMapper(IMapperConfigurationExpression cfg);
    }
}
