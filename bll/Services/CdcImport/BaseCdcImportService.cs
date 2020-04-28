using System.Collections.Generic;
using Quantumart.QP8.BLL.Factories;
using Quantumart.QP8.BLL.Models.NotificationSender;
using Quantumart.QP8.DAL.CdcImport;

namespace Quantumart.QP8.BLL.Services.CdcImport
{
    public abstract class BaseCdcImportService : ICdcImportService
    {
        private readonly ICdcImportFactory _factory;
        public BaseCdcImportService(ICdcImportFactory factory)
        {
            _factory = factory;
        }
        public string GetMaxLsn() =>
            CdcImportDal.GetMaxLsn(QPConnectionScope.Current.DbConnection);

        public string GetLastExecutedLsn(string providerUrl) =>
            CdcImportDal.GetLastExecutedLsn(QPConnectionScope.Current.DbConnection, providerUrl);

        public int PostLastExecutedLsn(string providerName, string providerUrl, string lastPushedLsn, string lastExecutedLsn) =>
            CdcImportDal.PostLastExecutedLsn(QPConnectionScope.Current.DbConnection, providerName, providerUrl, lastPushedLsn, lastExecutedLsn);

        public List<CdcTableTypeModel> ImportData(string captureInstance, string fromLsn = null, string toLsn = null) =>
            _factory.Create(captureInstance).ImportCdcData(fromLsn, toLsn);

        public abstract List<CdcTableTypeModel> GetCdcDataFromTables(string fromLsn = null, string toLsn = null);
    }
}
