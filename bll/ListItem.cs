using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Constants;


namespace Quantumart.QP8.BLL
{
    public class ListItem : SimpleListItem
    {
        public ListItem() : base()
		{
		}
		
		public ListItem(string value, string text) : base(value, text) {}		

		public ListItem(string value, string text, bool hasDependentItem)
			: this(value, text)
		{
			HasDependentItems = hasDependentItem;
		}

		public ListItem(string value, string text, string[] dependentIds)
			: this(value, text)
		{
			HasDependentItems = true;
			DependentItemIDs = dependentIds;
		}

		public ListItem(string value, string text, string dependentId) : this(value, text, (!String.IsNullOrWhiteSpace(dependentId)? new[] { dependentId } : null))
		{
		}

		public ListItem(int value, string text, string dependentId)
			: this(value.ToString(), text, (!String.IsNullOrWhiteSpace(dependentId) ? new[] { dependentId } : null))
		{
		}

		
        public bool Selected { get; set; }
        public bool HasDependentItems { get; set; }
		public string[] DependentItemIDs { get; set; }
    }
}
