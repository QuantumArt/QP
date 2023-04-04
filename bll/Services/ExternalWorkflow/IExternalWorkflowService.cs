using System.Threading;
using System.Threading.Tasks;

namespace Quantumart.QP8.BLL.Services.ExternalWorkflow;

public interface IExternalWorkflowService
{
    Task<bool> PublishWorkflow(string customerCode, int contentItemId, int siteId, CancellationToken token);
}
