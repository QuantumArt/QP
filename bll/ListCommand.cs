using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quantumart.QP8.BLL
{
    public class ListCommand
    {
        public string SortExpression { get; set; }
        public string FilterExpression { get; set; }
        public int StartPage { get; set; }
        public int PageSize { get; set; }

        public int StartRecord 
        {
            get
            {
                return (StartPage - 1) * PageSize + 1;
            }
        
        }
        
        
    }

    public class FolderCommand : ListCommand { }
}
