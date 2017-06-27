using System.Collections.Generic;
using Quantumart.QP8.BLL.Models.NotificationSender;
using Quantumart.QP8.DAL.CdcImport;

namespace Quantumart.QP8.BLL.Services.CdcImport
{
    public abstract class BaseCdcImportService : ICdcImportService
    {
        public string GetMaxLsn() =>
            CdcImportDal.GetMaxLsn(QPConnectionScope.Current.DbConnection);

        public string GetLastExecutedLsn() =>
            CdcImportDal.GetLastExecutedLsn(QPConnectionScope.Current.DbConnection);

        public int PostLastExecutedLsn(string providerUrl, string lastPushedLsn, string lastExecutedLsn) =>
            CdcImportDal.PostLastExecutedLsn(QPConnectionScope.Current.DbConnection, providerUrl, lastPushedLsn, lastExecutedLsn);

        public abstract List<CdcTableTypeModel> ImportData(string captureInstance, string fromLsn = null, string toLsn = null);
    }
}
