using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
