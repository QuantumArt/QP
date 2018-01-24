using System;

namespace Quantumart.QP8.BLL.ListItems
{
    public class ObjectListItem
    {
        public int Id { get; set; }

        public DateTime Created { get; set; }

        public DateTime Modified { get; set; }

        public int LastModifiedBy { get; set; }

        public string LastModifiedByLogin { get; set; }

        public string Name { get; set; }

        public string TypeName { get; set; }

        public string Description { get; set; }

        public string Icon { get; set; }

        public int LockedBy { get; set; }

        public string LockedByFullName { get; set; }

        public string LockedByIcon { get; set; }

        public string LockedByToolTip { get; set; }

        public int? ParentId { get; set; }

        public bool Overriden { get; set; }
    }

    public class ObjectSearchListItem : ObjectListItem
    {
        public string TemplateName { get; set; }

        public string PageName { get; set; }

        public string ParentName { get; set; }

        public string ActionCode => string.IsNullOrEmpty(PageName) ? Constants.ActionCode.TemplateObjectProperties : Constants.ActionCode.PageObjectProperties;

        public string EntityTypeCode => string.IsNullOrEmpty(PageName) ? Constants.EntityTypeCode.TemplateObject : Constants.EntityTypeCode.PageObject;
    }
}
