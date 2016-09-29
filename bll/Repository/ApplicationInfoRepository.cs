using Quantumart.QP8.DAL;

namespace Quantumart.QP8.BLL.Repository
{
    internal static class ApplicationInfoRepository
    {
        public static string GetCurrentDbVersion()
        {
            using (var scope = new QPConnectionScope())
            {
                return Common.GetCurrentDbVersion(scope.DbConnection);
            }
        }

        public static bool RecordActions()
        {
            using (new QPConnectionScope())
            {
                return DbRepository.Get().RecordActions;
            }
        }
    }
}
