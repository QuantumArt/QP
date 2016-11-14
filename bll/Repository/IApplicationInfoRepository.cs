namespace Quantumart.QP8.BLL.Repository
{
    public interface IApplicationInfoRepository
    {
        string GetCurrentDbVersion();

        bool RecordActions();
    }
}
