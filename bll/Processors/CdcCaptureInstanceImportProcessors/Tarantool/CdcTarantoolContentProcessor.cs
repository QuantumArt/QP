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
    public sealed class CdcTarantoolContentProcessor : BaseCdcTarantoolImportProcessor
    {
        public CdcTarantoolContentProcessor(string captureInstanseName)
            : base(captureInstanseName)
        {
        }

        public override List<CdcTableTypeModel> ImportCdcData(string fromLsn = null, string toLsn = null)
        {
            return GetCdcDataTable(fromLsn, toLsn).AsEnumerable().Select(row =>
            {
                try
                {
                    const bool isAsync = false;
                    var contentId = Convert.ToDecimal(row[ContentColumnName.ContentId]);
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
                            EntityType = ContentColumnName.TableName,
                            InvariantName = ContentColumnName.TableName.ToUpper(),
                            Columns = new Dictionary<string, object>
                            {
                                { TarantoolContentModel.IsAsync, isAsync },
                                { TarantoolContentModel.UseSplitting, !isAsync },
                                { TarantoolContentModel.ContentId, contentId },
                                { TarantoolContentModel.InvariantName, TarantoolContentModel.GetInvariantName(contentId, isAsync) },
                                { TarantoolContentModel.InvariantNameAsync, TarantoolContentModel.GetInvariantName(contentId, !isAsync) },
                                { TarantoolContentModel.ContentName, row[ContentColumnName.ContentName] as string },
                                { TarantoolContentModel.NetContentName, row[ContentColumnName.NetContentName] as string },
                                { TarantoolContentModel.IsForReplication, true },
                                { TarantoolContentModel.DefaultFieldsName, TarantoolContentModel.DefaultFields }
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
