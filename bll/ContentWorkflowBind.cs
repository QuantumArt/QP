namespace Quantumart.QP8.BLL
{
    public class ContentWorkflowBind : WorkflowBind
    {
        public ContentWorkflowBind()
        {
        }

        public static ContentWorkflowBind Create(Content content)
        {
            ContentWorkflowBind result = new ContentWorkflowBind();
            result.SetContent(content);
            return result;
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
