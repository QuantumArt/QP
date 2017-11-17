namespace Quantumart.QP8.BLL.ListItems
{
    public class ChildEntityPermissionListItem
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string LevelName { get; set; }
        public bool IsExplicit { get; set; }
        public bool Hide { get; set; }
        public bool PropagateToItems { get; set; }
    }
}
