using System.ComponentModel.DataAnnotations;

namespace Quantumart.QP8.BLL.Services.ExternalWorkflow.Models.UserTaskModels;

public class FillArticleDto : UserTaskBase
{
    public string Message { get; set; }
}
