using System.Threading;

namespace Quantumart.QP8.BLL.Services.UserSynchronization
{
    public interface ICommonUserService
    {
        void DisableUsers(string customerName, int diff, CancellationToken token);
        void EnableUsers(string customerName, string[] excludedUsers, CancellationToken token);
    }
}
