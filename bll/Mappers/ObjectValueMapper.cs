using AutoMapper;
using Quantumart.QP8.DAL;

namespace Quantumart.QP8.BLL.Mappers
{
    internal class ObjectValueMapper : GenericMapper<ObjectValue, ObjectValuesDAL>
    {
        public override void CreateDalMapper(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<ObjectValue, ObjectValuesDAL>(MemberList.Destination)
                .ForMember(data => data.Object, opt => opt.Ignore())
                ;
        }
    }
}
