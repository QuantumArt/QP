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
    public sealed class CdcTarantoolContentToContentAsyncRevProcessor : BaseCdcTarantoolImportProcessor
    {
        public CdcTarantoolContentToContentAsyncRevProcessor(string captureInstanseName)
            : base(captureInstanseName)
        {
        }

        public override List<CdcTableTypeModel> ImportCdcData(string fromLsn = null, string toLsn = null)
        {
            return GetCdcDataTable(fromLsn, toLsn).AsEnumerable().Select(row =>
            {
                try
                {
                    var linkId = Convert.ToInt32(row[ContentToContentColumnName.LinkId]);
                    return new CdcTableTypeModel
                    {
                        ChangeType = CdcActionType.Schema,
                        Action = (CdcOperationType)Enum.Parse(typeof(CdcOperationType), Convert.ToString(row[CdcCommonConstants.Operation])),
                        TransactionDate = (DateTime)row[CdcCommonConstants.TransactionDate],
                        TransactionLsn = row[CdcCommonConstants.TransactionLsn] as string,
                        SequenceLsn = row[CdcCommonConstants.SequenceLsn] as string,
                        FromLsn = row[CdcCommonConstants.FromLsn] as string,
                        ToLsn = row[CdcCommonConstants.ToLsn] as string,
                        Entity = new CdcEntityModel
                        {
                            EntityType = ContentToContentColumnName.TableName,
                            InvariantName = ContentToContentColumnName.TableName.ToUpper(),
                            Columns = new Dictionary<string, object>
                            {
                                { TarantoolContentToContentModel.LinkId, linkId },
                                { TarantoolContentToContentModel.InvariantName, TarantoolContentToContentModel.GetInvariantAsyncName(linkId, true) },
                                { TarantoolContentToContentModel.LeftContentId, Convert.ToInt32(row[ContentToContentColumnName.RContentId]) },
                                { TarantoolContentToContentModel.RightContentId, Convert.ToInt32(row[ContentToContentColumnName.LContentId]) },
                                { TarantoolContentToContentModel.IsSymmetric, (bool)row[ContentToContentColumnName.IsSymmetric] },
                                { TarantoolContentToContentModel.IsReverse, true },
                                { TarantoolContentToContentModel.DefaultFieldsName, TarantoolContentToContentModel.DefaultFields }
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
