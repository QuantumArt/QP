using System.Collections.Generic;
using System.Linq;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.Constants;

namespace Quantumart.QP8.BLL.Helpers
{
    // TODO: Move relations to unity
    public class ReplayHelper
    {
        private static IEnumerable<int> GetRelations(int contentId)
        {
            var content = ContentRepository
                .GetById(contentId);
            var fields = content.AggregatedContents.Any()
                ? content.Fields.Union(content.AggregatedContents.SelectMany(s => s.Fields))
                : content.Fields;
            return fields
                .Where(f => new[] { FieldExactTypes.O2MRelation, FieldExactTypes.M2MRelation, FieldExactTypes.M2ORelation }.Contains(f.ExactType))
                .Select(f => f.Id);
        }

        private static IEnumerable<int> GetClassifiers(int contentId)
        {
            return ContentRepository
                .GetById(contentId)
                .Fields
                .Where(n => new[] { FieldExactTypes.Classifier }.Contains(n.ExactType))
                .Select(f => f.Id);
        }

        public static bool IsRelation(int contentId, int fieldId)
        {
            return GetRelations(contentId).Contains(fieldId);
        }

        public static bool IsClassifier(int contentId, int fieldId)
        {
            return GetClassifiers(contentId).Contains(fieldId);
        }
    }
}
