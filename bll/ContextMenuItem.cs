using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8;
using Quantumart.QP8.DAL;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Mappers;

namespace Quantumart.QP8.BLL
{
    public class ContextMenuItem
    {
		public int ContextMenuId { get; set; }

		public int ActionId { get; set; }

		
		public string Name
        {
            get;
            set;
        }
        
        public string Icon
        {
            get;
            set;
        }

		public string IconDisabled { get; set; }
        
        public bool BottomSeparator
        {
            get;
            set;
        }

		public int Order { get; set; }

        
        public string ActionCode
        {
            get;
            set;
        }
        
        public string ActionTypeCode
        {
            get;
            set;
        }


    }
}
