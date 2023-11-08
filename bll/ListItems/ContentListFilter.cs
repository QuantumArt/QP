using Quantumart.QP8.Constants;
using System.Collections.Generic;

namespace Quantumart.QP8.BLL.ListItems
{
    public sealed class ContentListFilter
    {
        public ContentListFilter()
        {
            IsVirtual = false;
            Mode = ContentSelectMode.Normal;
        }

        public int? GroupId { get; set; }

        public int? SiteId { get; set; }

        public string ContentName { get; set; }

        public bool IsVirtual { get; set; }

        public ContentSelectMode Mode { get; set; }

        public Dictionary<string, object[]> CustomFilter { get; set; }

        public static ContentListFilter Empty => new ContentListFilter();
    }
}
