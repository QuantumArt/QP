using System.Collections.Generic;
using Quantumart.QP8.DAL;

namespace Quantumart.QP8.BLL.Repository
{
    public class ApplicationInfoRepository : IApplicationInfoRepository
    {
        public string GetCurrentDbVersion()
        {
            using (var scope = new QPConnectionScope())
            {
                return Common.GetCurrentDbVersion(scope.DbConnection);
            }
        }

        public bool RecordActions()
        {
            using (new QPConnectionScope())
            {
                return DbRepository.Get().RecordActions;
            }
        }

        public void PostReplay(HashSet<string> insertIdentityOptions)
        {
            DefaultRepository.PostReplay(insertIdentityOptions);
        }
    }
}
