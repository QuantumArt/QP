using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quantumart.QP8.BLL.ListItems
{
    public class PageListItem
    {
        public int Id { get; set; }

        public DateTime Created { get; set; }

        public DateTime Modified { get; set; }

        public int LastModifiedBy { get; set; }

        public string LastModifiedByLogin { get; set; }

        public bool GenerateTrace { get; set; }

        public bool Reassemble { get; set; }

        public string Name { get; set; }

        public string Folder { get; set; }

        public string Description { get; set; }

        public string FileName { get; set; }

        public DateTime Assembled { get; set; }

        public int AssembledBy { get; set; }

        public string AssembledByLogin { get; set; }

		public int LockedBy { get; set; }

		public string LockedByFullName { get; set; }

		public string LockedByIcon { get; set; }

		public string LockedByToolTip { get; set; }

		public string TemplateName { get; set; }
    }
}
