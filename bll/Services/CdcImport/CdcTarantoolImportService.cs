using System.Collections.Generic;
using Quantumart.QP8.BLL.Factories;
using Quantumart.QP8.BLL.Models.NotificationSender;

namespace Quantumart.QP8.BLL.Services.CdcImport
{
    public sealed class CdcTarantoolImportService : BaseCdcImportService
    {
        public override List<CdcTableTypeModel> ImportData(string captureInstance, string fromLsn = null, string toLsn = null) =>
            CdcTarantoolImportFactory.Create(captureInstance).ImportCdcData(fromLsn, toLsn);
    }
}
