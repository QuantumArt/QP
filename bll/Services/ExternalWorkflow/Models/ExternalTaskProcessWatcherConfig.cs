using System;
using QA.Workflow.Integration.QP.Models;

namespace Quantumart.QP8.BLL.Services.ExternalWorkflow.Models;

public class ExternalTaskProcessWatcherConfig : ExtendedCamundaSettings
{
    public TimeSpan ProcessCheckInterval { get; set; }
}
