using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Quantumart.QP8.WebMvc.Extensions.Helpers
{
    public class QPSelectListItem : SelectListItem
    {
        public bool HasDependentItems { get; set; }

        public string[] DependentItemIDs { get; set; }

        public static string GetPanelHash(string id, IList<QPSelectListItem> list)
        {
            var dependentList = list
                .Where(n => n.HasDependentItems && n.DependentItemIDs != null && n.DependentItemIDs.Length > 0)
                .Select(n => GetHashItem(n.Value, GetDependentPanelHtmlIDs(id, n.Value, n.DependentItemIDs)))
                .ToArray();

            return string.Join(",", list
                .Where(n => n.HasDependentItems && (n.DependentItemIDs == null || n.DependentItemIDs.Length == 0))
                .Select(n => GetHashItem(n.Value, GetDependentPanelHtmlId(id, n.Value)))
                .Concat(dependentList));
        }

        public static string GetDependentPanelId(string id, string value)
        {
            return $"{id}_{value}_Panel";
        }

        private static string GetDependentPanelHtmlId(string id, string value)
        {
            return $"#{id}_{value}_Panel";
        }

        private static string GetDependentPanelHtmlIDs(string id, string value, string[] dependentItemIds)
        {
            var dims = dependentItemIds.Select(di => string.Concat("#", di));
            return string.Join(",", GetDependentPanelHtmlId(id, value), string.Join(",", dims));
        }

        public static string GetHashItem(string id, string value)
        {
            return $@"""{id}"" : ""{value}""";
        }

        public string TextWithId => $"(#{Value}) - {Text}";

        public QPSelectListItem CopyWithIdInText()
        {
            var result = new QPSelectListItem
            {
                DependentItemIDs = DependentItemIDs,
                HasDependentItems = HasDependentItems,
                Selected = Selected,
                Value = Value,
                Text = TextWithId
            };

            return result;
        }
    }
}
