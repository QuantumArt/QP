using System;

namespace Quantumart.QP8.BLL.Services.UserSynchronization
{
    public class UserSynchronisationServiceDummy : IUserSynchronizationService
    {
        public bool NeedSynchronization()
        {
            return false;
        }

        public void Synchronize()
        {
            throw new NotImplementedException();
        }
    }
}
