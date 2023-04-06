namespace Quantumart.QP8.BLL.Services.ExternalWorkflow.Models;

public class UserTaskData
{
    public int Id { get; set; }
    public int ParentId { get; set; }
    public string ItemName { get; set; }
    public string TaskId { get; set; }
    public string ProcessId { get; set; }
    public string TaskName { get; set; }
    public string ContentName { get; set; }
    public string SiteName { get; set; }
}
