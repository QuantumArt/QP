using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Quantumart.QP8.BLL.Models.NotificationSender;
using Quantumart.QP8.Constants.Cdc;
using Quantumart.QP8.Constants.Cdc.Enums;
using Quantumart.QP8.Constants.DbColumns;
using static Quantumart.QP8.Constants.Cdc.TarantoolContentArticleModel;

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
                var contentId = (decimal)row[ContentItemColumnName.ContentId];
                return new CdcTableTypeModel
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
                        EntityType = TarantoolContentArticleModel.EntityType,
                        InvariantName = GetInvariantName(contentId),
                        Columns = new Dictionary<string, object>
                        {
                            { ContentItemId, (decimal)row[ContentItemColumnName.ContentItemId] },
                            { StatusTypeId, (decimal)row[ContentItemColumnName.StatusTypeId] },
                            { Visible, (decimal)row[ContentItemColumnName.Visible] == 1 },
                            { Archive, (decimal)row[ContentItemColumnName.Archive] == 1 },
                            { Created, (DateTime)row[ContentItemColumnName.Created] },
                            { Modified, (DateTime)row[ContentItemColumnName.Modified] },
                            { LastModifiedBy, (decimal)row[ContentItemColumnName.LastModifiedBy] }
                        },
                        MetaData = new Dictionary<string, object>
                        {
                            { IsSplitted, Equals(true, row[ContentItemColumnName.Splitted]) }
                        }
                    }
                };
            }).OrderBy(cdc => cdc.TransactionLsn).ToList();
        }
    }
}
