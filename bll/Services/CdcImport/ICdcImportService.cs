using System.Collections.Generic;
using Quantumart.QP8.BLL.Models.NotificationSender;

namespace Quantumart.QP8.BLL.Services.CdcImport
{
    public interface ICdcImportService
    {
        /// <summary>
        /// Get saved before upper bound lsn, that was checked at previous run
        /// </summary>
        /// <returns>Cdc last executed lsn</returns>
        string GetLastExecutedLsn();

        /// <summary>
        /// Get max available lsn for current database
        /// </summary>
        /// <returns>Cdc max lsn</returns>
        string GetMaxLsn();

        /// <summary>
        /// Save cdc log information for provider
        /// </summary>
        /// <param name="providerName">Provider name</param>
        /// <param name="providerUrl">Provider url endpoint</param>
        /// <param name="lastPushedLsn">Last transaction lsn that was successfuly pushed for current provider</param>
        /// <param name="lastExecutedLsn">Upper bound lsn used for search</param>
        /// <returns>Primary key of the table</returns>
        int PostLastExecutedLsn(string providerName, string providerUrl, string lastPushedLsn, string lastExecutedLsn);

        /// <summary>
        /// Import single data table from cdc, and customize based on
        /// <param name="captureInstance" />
        /// value
        /// </summary>
        /// <param name="captureInstance">Using to customize import and processing cdc data from db</param>
        /// <param name="fromLsn">Lower bound lsn, if null - min available for each table will be used</param>
        /// <param name="toLsn">Upper bound lsn, if null - max lsn for database will be used</param>
        /// <returns>Get cdc table data</returns>
        List<CdcTableTypeModel> ImportData(string captureInstance, string fromLsn = null, string toLsn = null);
    }
}
