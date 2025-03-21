using Microsoft.Extensions.Options;
using NLog;
using Quantumart.QP8.BLL.Services.API;

namespace Quantumart.QP8.BLL.Services.UserSynchronization;

public abstract class UserSynchronizationServiceBase : IUserSynchronizationService
{
    protected const string DefaultMail = "undefined@domain.com";
    protected const string DefaultValue = "undefined";

    protected readonly ILogger Logger;
    protected readonly CommonSchedulerProperties Settings;

    protected UserSynchronizationServiceBase(IOptions<CommonSchedulerProperties> options, ILogger logger)
    {
        Settings = options.Value;
        Logger = logger;
    }

    public abstract bool NeedSynchronization();

    public abstract void Synchronize();
}
