using System.Collections.Generic;
using System.Linq;

namespace Quantumart.QP8.BLL
{
    public static class QpPluginFieldValueExtensions
    {

        public static IEnumerable<QpPluginFieldValueGroup> ToFieldGroups(this IEnumerable<QpPluginFieldValue> list)
        {
            return list
                .GroupBy(g => g.Plugin.Name)
                .Select(group => new QpPluginFieldValueGroup
                {
                    Name = group.Key,
                    Fields = group.ToList()
                }).ToList();
        }
    }
}
