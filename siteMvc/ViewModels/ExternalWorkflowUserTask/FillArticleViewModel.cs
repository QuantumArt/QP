using Quantumart.QP8.Resources;
using Quantumart.QP8.WebMvc.ViewModels.Abstract;

namespace Quantumart.QP8.WebMvc.ViewModels.ExternalWorkflowUserTask;

public class FillArticleViewModel : UserTaskBaseViewModel
{
    public override int Width => 370;

    public override int Height => 50;

    public string Message { get; } = ExternalWorkflowStrings.FillArticleMessage;
}
