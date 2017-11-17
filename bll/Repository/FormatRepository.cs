using System.Collections.Generic;
using System.Linq;
using Quantumart.QP8.BLL.Facades;
using Quantumart.QP8.DAL;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.BLL.Repository
{
    internal class FormatRepository
    {
        internal static ObjectFormat SaveObjectFormatProperties(ObjectFormat objectFormat) => DefaultRepository.Save<ObjectFormat, ObjectFormatDAL>(objectFormat);

        internal static ObjectFormat UpdateObjectFormatProperties(ObjectFormat objectFormat) => DefaultRepository.Update<ObjectFormat, ObjectFormatDAL>(objectFormat);

        internal static IEnumerable<EntityObject> GetList(IEnumerable<int> IDs, bool pageOrTemplate)
        {
            IEnumerable<decimal> decIDs = Converter.ToDecimalCollection(IDs).Distinct().ToArray();
            var result = MapperFacade.ObjectFormatMapper
                .GetBizList(QPContext.EFContext.ObjectFormatSet
                    .Where(f => decIDs.Contains(f.Id))
                    .ToList()
                );

            foreach (var item in result)
            {
                item.PageOrTemplate = pageOrTemplate;
            }

            return result;
        }
    }
}
