using System.Collections.Generic;
using System.Linq;

namespace Quantumart.QP8.BLL.Services.DTO
{
    public class EntityTreeItem
	{
		public EntityTreeItem()
		{
			Enabled = true;
		}

		public string Id { get; set; }

        public int? ParentId { get; set; }

        public string Name { get; set; }

		public string Alias { get; set; }

		public string LockedByToolTip { get; set; }

		public bool LockedByAnyone { get; set; }

		public bool LockedByYou { get; set; }

		public bool HasChildren { get; set; }

		public bool Enabled { get; set; }

		public bool Checked { get; set; }

		public IEnumerable<EntityTreeItem> Children { get; set; }

		public string IconUrl { get; set; }

		public string IconTitle { get; set; }

        public bool IsHighlighted { get; set; }

		public bool Expanded => Children != null && Children.Any();
	}
}
