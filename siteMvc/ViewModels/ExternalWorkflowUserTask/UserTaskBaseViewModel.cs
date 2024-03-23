using Quantumart.QP8.WebMvc.ViewModels.Abstract;

namespace Quantumart.QP8.WebMvc.ViewModels.ExternalWorkflowUserTask;

public class UserTaskBaseViewModel : EntityViewModel
{
    public bool ForceFormSubmit { get; } = true;

    public override string EntityTypeCode => Constants.EntityTypeCode.ArticleExternalWorkflow;

    public override string ActionCode => Constants.ActionCode.GetExternalWorkflowUserTask;
}
