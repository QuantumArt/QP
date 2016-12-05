namespace Quantumart.QPublishing.Info
{
    public class Content
    {
        public int Id { get; set; }

        public int SiteId { get; set; }

        public string Name { get; set; }

        public string LinqName { get; set; }

        public int VirtualType { get; set; }

        public int MaxVersionNumber { get; set; }

        public int? WorkflowId { get; set; }

        public bool UseVersionControl => MaxVersionNumber != 0;

        public bool IsWorkflowAssigned => WorkflowId.HasValue;
    }
}
