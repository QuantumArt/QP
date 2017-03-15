namespace Quantumart.QP8.BLL.Services.UserSynchronization
{
    public interface IUserSynchronizationService
    {
        bool NeedSynchronization();

        void Synchronize();
    }
}
