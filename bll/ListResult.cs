using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quantumart.QP8.BLL
{
    public class ListResult<T>
    {
        public List<T> Data { get; set; }
        public int TotalRecords { get; set; }
    }
}
