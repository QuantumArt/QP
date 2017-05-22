using System.Collections.Generic;
using System.Data;
using Quantumart.QP8.BLL.Models.NotificationSender;
using Quantumart.QP8.DAL.NotificationSender;

namespace Quantumart.QP8.BLL.Processors.CdcCaptureInstanceImportProcessors
{
    public abstract class CdcImportProcessor
    {
        private readonly string _captureInstanceName;

        protected CdcImportProcessor(string captureInstanceName)
        {
            _captureInstanceName = captureInstanceName;
        }

        public abstract List<CdcTableTypeModel> ImportCdcData(string fromLsn = null, string toLsn = null);

        public virtual DataTable GetCdcDataTable(string fromLsn = null, string toLsn = null) =>
            CdcImportDal.GetCdcTableData(QPConnectionScope.Current.DbConnection, _captureInstanceName, fromLsn, toLsn);
    }
}
