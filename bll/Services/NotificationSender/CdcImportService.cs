using System.Collections.Generic;
using QP8.Infrastructure.Logging.Extensions;
using Quantumart.QP8.BLL.Factories;
using Quantumart.QP8.BLL.Models.NotificationSender;
using Quantumart.QP8.DAL.NotificationSender;

namespace Quantumart.QP8.BLL.Services.NotificationSender
{
    public class CdcImportService : ICdcImportService
    {
        public string GetMaxLsn() => CdcImportDal.GetMaxLsn(QPConnectionScope.Current.DbConnection);

        public string GetLastExecutedLsn() => CdcImportDal.GetLastExecutedLsn(QPConnectionScope.Current.DbConnection);

        public void PostLastExecutedLsn(string providerUrl, string lastPushedLsn, string lastExecutedLsn)
        {
            CdcImportDal.PostLastExecutedLsn(QPConnectionScope.Current.DbConnection, providerUrl, lastPushedLsn, lastExecutedLsn);
        }

        public List<CdcTableTypeModel> ImportData(string captureInstance, string fromLsn = null, string toLsn = null) =>
            CdcCaptureInstanceImportFactory.Create(captureInstance).ImportCdcData(fromLsn, toLsn).LogTraceFormat($"Imported cdc table ({captureInstance}) data object: {{0}}");
    }
}
