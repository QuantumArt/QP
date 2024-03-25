using Quantumart.QP8.Resources;
using Quantumart.QP8.WebMvc.ViewModels.Abstract;

namespace Quantumart.QP8.WebMvc.ViewModels.ExternalWorkflowUserTask;

public class FillArticleViewModel : UserTaskBaseViewModel
{
    public override int Width => 300;

    public override int Height => 200;

    public string Message { get; } = ExternalWorkflowStrings.FillArticleMessage;
}
