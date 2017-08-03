using System.Collections.Generic;
using Quantumart.QP8.BLL.Models.NotificationSender;

namespace Quantumart.QP8.CdcDataImport.Tarantool.Infrastructure.Services
{
    public interface ICdcDataImportProcessor
    {
        List<CdcTableTypeModel> GetCdcDataFromTables(string fromLsn = null, string toLsn = null);
    }
}
