using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Quantumart.QP8.BLL.Models.NotificationSender;
using Quantumart.QP8.Constants.Cdc;
using static Quantumart.QP8.Constants.DbColumns.ContentItemColumnName;

namespace Quantumart.QP8.BLL.Processors.CdcCaptureInstanceImportProcessors
{
    public class CdcContentItemProcessor : CdcImportProcessor
    {
        public CdcContentItemProcessor(string captureInstanseName)
            : base(captureInstanseName)
        {
        }

        public override List<CdcTableTypeModel> ImportCdcData(string fromLsn = null, string toLsn = null)
        {
            return GetCdcDataTable(fromLsn, toLsn).AsEnumerable().Select(row =>
            {
                var contentId = (decimal)row[ContentId];
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
                        EntityType = "article",
                        InvariantName = $"content_{contentId}",
                        Columns = new Dictionary<string, object>
                        {
                            { ContentItemId.ToUpper(), (decimal)row[ContentItemId] },
                            { StatusTypeId.ToUpper(), (decimal)row[StatusTypeId] },
                            { Visible.ToUpper(), (decimal)row[Visible] == 1 },
                            { Archive.ToUpper(), (decimal)row[Archive] == 1 },
                            { Created.ToUpper(), (DateTime)row[Created] },
                            { Modified.ToUpper(), (DateTime)row[Modified] },
                            { LastModifiedBy.ToUpper(), (decimal)row[LastModifiedBy] }
                        }
                    }
                };
            }).OrderBy(cdc => cdc.TransactionLsn).ToList();
        }
    }
}
