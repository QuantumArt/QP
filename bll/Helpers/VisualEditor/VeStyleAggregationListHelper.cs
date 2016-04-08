using System.Collections.Generic;
using System.Linq;

namespace Quantumart.QP8.BLL.Helpers.VisualEditor
{
    public static class VeStyleAggregationListHelper
    {
        private const string Separator = ";";
        private const string ItemSeparator = ":";

        public static string Serialize(IEnumerable<VeStyleAggregationListItem> items)
        {
            return string.Join(Separator, items.Select(x => $"{x.Name}:{x.ItemValue}"));
        }

        public static IEnumerable<VeStyleAggregationListItem> Deserialize(string str)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return Enumerable.Empty<VeStyleAggregationListItem>();
            }

            return str.Split(Separator.ToCharArray()).Select(x =>
            {
                var strings = x.Split(ItemSeparator.ToCharArray());
                return new VeStyleAggregationListItem { Name = strings[0], ItemValue = strings[1] };
            });
        }
    }
}
