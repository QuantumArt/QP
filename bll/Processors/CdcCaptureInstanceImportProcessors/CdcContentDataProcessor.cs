using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Quantumart.QP8.BLL.Models.NotificationSender;
using Quantumart.QP8.Constants.Cdc;
using static Quantumart.QP8.Constants.DbColumns.ContentDataColumnName;

namespace Quantumart.QP8.BLL.Processors.CdcCaptureInstanceImportProcessors
{
    public class CdcContentDataProcessor : CdcImportProcessor
    {
        public CdcContentDataProcessor(string captureInstanseName)
            : base(captureInstanseName)
        {
        }

        public override List<CdcTableTypeModel> ImportCdcData(string fromLsn = null, string toLsn = null)
        {
            return GetCdcDataTable(fromLsn, toLsn).AsEnumerable().Select(row =>
            {
                var data = row[Data] as string;
                var blobData = row[BlobData] as string;
                return new CdcTableTypeModel
                {
                    ChangeType = CdcActionType.Data,
                    Action = row[TarantoolCommonConstants.Operation] as string,
                    TransactionDate = (DateTime)row[TarantoolCommonConstants.TransactionDate],
                    TransactionLsn = row[TarantoolCommonConstants.TransactionLsn] as string,
                    SequenceLsn = row[TarantoolCommonConstants.SequenceLsn] as string,
                    FromLsn = row[TarantoolCommonConstants.FromLsn] as string,
                    ToLsn = row[TarantoolCommonConstants.ToLsn] as string,
                    Entity = new CdcEntityModel
                    {
                        EntityType = TableName,
                        InvariantName = TableName.ToUpper(),
                        Columns = new Dictionary<string, object>
                        {
                            { AttributeId.ToUpper(), (decimal)row[AttributeId] },
                            { ContentItemId.ToUpper(), (decimal)row[ContentItemId] },
                            { ContentDataId.ToUpper(), (decimal)row[ContentDataId] },
                            { Data.ToUpper(), blobData ?? data },
                            { Created.ToUpper(), (DateTime)row[Created] },
                            { Modified.ToUpper(), (DateTime)row[Modified] }
                        }
                    }
                };
            }).OrderBy(cdc => cdc.TransactionLsn).ToList();
        }
    }
}
