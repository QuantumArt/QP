using System.Collections.Generic;
using System.Linq;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.Constants;

namespace Quantumart.QP8.BLL.Helpers
{
    public class ReplayHelper
    {
        public static Dictionary<int, Field> GetRelations(int contentId)
        {
            return ContentRepository
                .GetById(contentId)
                .Fields
                .Where(n => new[] { FieldExactTypes.O2MRelation, FieldExactTypes.M2MRelation, FieldExactTypes.M2ORelation }
                    .Contains(n.ExactType)
                ).ToDictionary(n => n.Id, n => n);
        }

        public static Dictionary<int, Field> GetClasifiers(int contentId)
        {
            return ContentRepository
                .GetById(contentId)
                .Fields
                .Where(n => new[] { FieldExactTypes.Classifier }
                    .Contains(n.ExactType)
                ).ToDictionary(n => n.Id, n => n);
        }


        public static Field GetSingleField(int contentId)
        {
            return FieldRepository.GetFullList(contentId).First();
        }
    }
}
