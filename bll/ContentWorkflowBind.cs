namespace Quantumart.QP8.BLL
{
    public class ContentWorkflowBind : WorkflowBind
    {
        public ContentWorkflowBind()
        {
        }

        public ContentWorkflowBind(Content content)
        {
            SetContent(content);
        }

        public void SetContent(Content content)
        {
            Content = content;
            ContentId = content.Id;
        }

        public Content Content { get; set; }

        public int ContentId { get; set; }
    }
}
