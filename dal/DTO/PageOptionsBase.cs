using System.Collections.Generic;

namespace Quantumart.QP8.DAL.DTO
{
    public abstract class PageOptionsBase
    {
        public string SortExpression { get; set; }

        public int StartRecord { get; set; }

        public int PageSize { get; set; }

        public IEnumerable<int> SelectedIDs { get; set; }
    }
}
