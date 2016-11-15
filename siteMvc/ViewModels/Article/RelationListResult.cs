using System.Collections.Generic;
using Quantumart.QP8.BLL;

namespace Quantumart.QP8.WebMvc.ViewModels.Article
{
    public class RelationListResult
    {
        public RelationListResult()
        {
            Items = new List<ListItem>();
        }

        public IEnumerable<ListItem> Items { get; set; }

        public bool IsListOverflow { get; set; }
    }
}