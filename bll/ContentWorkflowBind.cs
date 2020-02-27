using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

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

        [BindNever]
        [ValidateNever]
        public Content Content { get; set; }

        public int ContentId { get; set; }
    }
}
