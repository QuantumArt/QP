using System.Collections.Generic;

namespace Quantumart.QP8.BLL
{
    public class ListResult<T>
    {
        public List<T> Data { get; set; }
        public int TotalRecords { get; set; }
    }
}
