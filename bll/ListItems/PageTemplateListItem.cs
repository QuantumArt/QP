using System;

namespace Quantumart.QP8.BLL.ListItems
{
    public class PageTemplateListItem
    {
        public int Id { get; set; }

        public bool IsSystem { get; set; }

        public string Name { get; set; }

        public string Folder { get; set; }

        public string Description { get; set; }

        public DateTime Created { get; set; }

        public DateTime Modified { get; set; }

        public int LastModifiedBy { get; set; }

        public string LastModifiedByLogin { get; set; }

        public int LockedBy { get; set; }

        public string LockedByFullName { get; set; }

        public string LockedByIcon { get; set; }

        public string LockedByToolTip { get; set; }
    }

    public class PageTemplateSearchListItem : PageTemplateListItem
    {
        public int ParentId { get; set; }

        public string ParentName { get; set; }
    }
}
