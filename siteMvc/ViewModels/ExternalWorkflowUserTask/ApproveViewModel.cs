using System.ComponentModel.DataAnnotations;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.WebMvc.ViewModels.ExternalWorkflowUserTask;

public class ApproveViewModel : UserTaskBaseViewModel
{
    [Display(Name = "ApprovedRequest", ResourceType = typeof(ExternalWorkflowStrings))]
    public bool Approved { get; set; }

    [Display(Name = "ApproveComment", ResourceType = typeof(ExternalWorkflowStrings))]
    public string Message { get; set; }
}
