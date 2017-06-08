using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using QP8.Infrastructure.Extensions;
using Quantumart.QP8.BLL.Models.NotificationSender;
using Quantumart.QP8.Constants.Cdc;
using Quantumart.QP8.Constants.Cdc.Enums;
using Quantumart.QP8.Constants.DbColumns;
using Quantumart.QP8.Utils;
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
                try
                {
                    var contentId = Convert.ToInt32(row[ContentItemColumnName.ContentId]);
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
                                { ContentItemId, Convert.ToInt32(row[ContentItemColumnName.ContentItemId]) },
                                { StatusTypeId, Convert.ToInt32(row[ContentItemColumnName.StatusTypeId]) },
                                { Visible, Convert.ToInt32(row[ContentItemColumnName.Visible]) == 1 },
                                { Archive, Convert.ToInt32(row[ContentItemColumnName.Archive]) == 1 },
                                { Created, (DateTime)row[ContentItemColumnName.Created] },
                                { Modified, (DateTime)row[ContentItemColumnName.Modified] },
                                { LastModifiedBy, Convert.ToInt32(row[ContentItemColumnName.LastModifiedBy]) }
                            },
                            MetaData = new Dictionary<string, object>
                            {
                                { IsSplitted, Equals(true, row[ContentItemColumnName.Splitted]) }
                            }
                        }
                    };
                }
                catch (Exception ex)
                {
                    throw new Exception($"There was an exception for parsing \"{CaptureInstanceName}\" row: {row.ToSimpleDataRow().ToJsonLog()}", ex);
                }
            }).OrderBy(cdc => cdc.TransactionLsn).ToList();
        }
    }
}
