using Quantumart.QP8.WebMvc.ViewModels.Abstract;

namespace Quantumart.QP8.WebMvc.ViewModels.ExternalWorkflowUserTask;

public class FillArticleViewModel : EntityViewModel
{
    public string Message { get; set; }

    public bool ForceFormSubmit { get; } = true;

    public override string EntityTypeCode => Constants.EntityTypeCode.Content;

    public override string ActionCode => Constants.ActionCode.GetExternalWorkflowUserTasks;
}
