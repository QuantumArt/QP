using System.Linq;
using Quantumart.QP8.BLL.Facades;

namespace Quantumart.QP8.BLL.Repository
{
    internal class ObjectTypeRepository
    {
        public static ObjectType GetByName(string name)
        {
            var entities = QPContext.EFContext;
            return MapperFacade.ObjectTypeMapper.GetBizObject(entities.ObjectTypeSet.SingleOrDefault(x => x.Name == name));
        }
    }
}
