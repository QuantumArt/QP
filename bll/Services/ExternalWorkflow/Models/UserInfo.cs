using System.Collections.Generic;

namespace Quantumart.QP8.BLL.Services.ExternalWorkflow.Models;

public class UserInfo
{
    public string Login { get; set; }
    public List<string> Roles { get; set; } = new();
}
