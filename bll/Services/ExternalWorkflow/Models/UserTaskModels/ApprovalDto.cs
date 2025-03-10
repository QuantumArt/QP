namespace Quantumart.QP8.BLL.Services.ExternalWorkflow.Models.UserTaskModels;

public class ApprovalDto : UserTaskBase
{
    public bool Approved { get; set; }
    public string Message { get; set; } = "false";
}
