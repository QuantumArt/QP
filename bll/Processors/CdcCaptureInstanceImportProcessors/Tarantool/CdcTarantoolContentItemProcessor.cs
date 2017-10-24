using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using QP8.Infrastructure.Extensions;
using Quantumart.QP8.BLL.Models.NotificationSender;
using Quantumart.QP8.Constants.Cdc;
using Quantumart.QP8.Constants.Cdc.Enums;
using Quantumart.QP8.Constants.Cdc.Tarantool;
using Quantumart.QP8.Constants.DbColumns;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.BLL.Processors.CdcCaptureInstanceImportProcessors.Tarantool
{
    public sealed class CdcTarantoolContentItemProcessor : BaseCdcTarantoolImportProcessor
    {
        public CdcTarantoolContentItemProcessor(string captureInstanseName)
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
                        Action = (CdcOperationType)Enum.Parse(typeof(CdcOperationType), Convert.ToString(row[CdcCommonConstants.Operation])),
                        TransactionDate = (DateTime)row[CdcCommonConstants.TransactionDate],
                        TransactionLsn = row[CdcCommonConstants.TransactionLsn] as string,
                        SequenceLsn = row[CdcCommonConstants.SequenceLsn] as string,
                        FromLsn = row[CdcCommonConstants.FromLsn] as string,
                        ToLsn = row[CdcCommonConstants.ToLsn] as string,
                        Entity = new CdcEntityModel
                        {
                            EntityType = TarantoolContentArticleModel.EntityType,
                            InvariantName = TarantoolContentArticleModel.GetInvariantName(contentId),
                            Columns = new Dictionary<string, object>
                            {
                                { TarantoolContentArticleModel.ContentItemId, Convert.ToInt32(row[ContentItemColumnName.ContentItemId]) },
                                { TarantoolContentArticleModel.StatusTypeId, Convert.ToInt32(row[ContentItemColumnName.StatusTypeId]) },
                                { TarantoolContentArticleModel.Visible, Convert.ToInt32(row[ContentItemColumnName.Visible]) == 1 },
                                { TarantoolContentArticleModel.Archive, Convert.ToInt32(row[ContentItemColumnName.Archive]) == 1 },
                                { TarantoolContentArticleModel.Created, (DateTime)row[ContentItemColumnName.Created] },
                                { TarantoolContentArticleModel.Modified, (DateTime)row[ContentItemColumnName.Modified] },
                                { TarantoolContentArticleModel.LastModifiedBy, Convert.ToInt32(row[ContentItemColumnName.LastModifiedBy]) }
                            },
                            MetaData = new Dictionary<string, object>
                            {
                                { TarantoolContentArticleModel.IsSplitted, Equals(true, row[ContentItemColumnName.Splitted]) }
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
