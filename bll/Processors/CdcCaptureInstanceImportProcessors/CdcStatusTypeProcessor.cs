using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Quantumart.QP8.BLL.Models.NotificationSender;
using Quantumart.QP8.Constants.Cdc;
using Quantumart.QP8.Constants.Cdc.Enums;
using Quantumart.QP8.Constants.DbColumns;
using static Quantumart.QP8.Constants.Cdc.TarantoolStatusTypeModel;

namespace Quantumart.QP8.BLL.Processors.CdcCaptureInstanceImportProcessors
{
    public class CdcStatusTypeProcessor : CdcImportProcessor
    {
        public CdcStatusTypeProcessor(string captureInstanseName)
            : base(captureInstanseName)
        {
        }

        public override List<CdcTableTypeModel> ImportCdcData(string fromLsn = null, string toLsn = null)
        {
            return GetCdcDataTable(fromLsn, toLsn).AsEnumerable().Select(row => new CdcTableTypeModel
            {
                ChangeType = CdcActionType.Data,
                Action = (CdcOperationType)Enum.Parse(typeof(CdcOperationType), row[TarantoolCommonConstants.Operation] as string),
                TransactionDate = (DateTime)row[TarantoolCommonConstants.TransactionDate],
                TransactionLsn = row[TarantoolCommonConstants.TransactionLsn] as string,
                SequenceLsn = row[TarantoolCommonConstants.SequenceLsn] as string,
                FromLsn = row[TarantoolCommonConstants.FromLsn] as string,
                ToLsn = row[TarantoolCommonConstants.ToLsn] as string,
                Entity = new CdcEntityModel
                {
                    EntityType = StatusTypeColumnName.TableName,
                    InvariantName = StatusTypeColumnName.TableName.ToUpper(),
                    Columns = new Dictionary<string, object>
                    {
                        { StatusTypeId, (decimal)row[StatusTypeColumnName.StatusTypeId] },
                        { StatusTypeName, row[StatusTypeColumnName.StatusTypeName] as string }
                    }
                }
            }).OrderBy(cdc => cdc.TransactionLsn).ToList();
        }
    }
}
