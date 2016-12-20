using System.Threading;
using System.Threading.Tasks;

namespace Quantumart.QP8.Scheduler.API
{
    public interface IProcessor
    {
        Task Run(CancellationToken token);
    }
}
