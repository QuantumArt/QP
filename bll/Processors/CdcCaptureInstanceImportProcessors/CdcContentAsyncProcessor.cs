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
using static Quantumart.QP8.Constants.Cdc.TarantoolContentModel;

namespace Quantumart.QP8.BLL.Processors.CdcCaptureInstanceImportProcessors
{
    public class CdcContentAsyncProcessor : CdcImportProcessor
    {
        public CdcContentAsyncProcessor(string captureInstanseName)
            : base(captureInstanseName)
        {
        }

        public override List<CdcTableTypeModel> ImportCdcData(string fromLsn = null, string toLsn = null)
        {
            return GetCdcDataTable(fromLsn, toLsn).AsEnumerable().Select(row =>
            {
                try
                {
                    var isAsync = true;
                    var contentId = Convert.ToDecimal(row[ContentColumnName.ContentId]);
                    return new CdcTableTypeModel
                    {
                        ChangeType = CdcActionType.Schema,
                        Action = (CdcOperationType)Enum.Parse(typeof(CdcOperationType), row[TarantoolCommonConstants.Operation] as string),
                        TransactionDate = (DateTime)row[TarantoolCommonConstants.TransactionDate],
                        TransactionLsn = row[TarantoolCommonConstants.TransactionLsn] as string,
                        SequenceLsn = row[TarantoolCommonConstants.SequenceLsn] as string,
                        FromLsn = row[TarantoolCommonConstants.FromLsn] as string,
                        ToLsn = row[TarantoolCommonConstants.ToLsn] as string,
                        Entity = new CdcEntityModel
                        {
                            EntityType = ContentColumnName.TableName,
                            InvariantName = ContentColumnName.TableName.ToUpper(),
                            Columns = new Dictionary<string, object>
                            {
                                { IsAsync, isAsync },
                                { UseSplitting, !isAsync },
                                { ContentId, contentId },
                                { InvariantName, GetInvariantName(contentId, isAsync) },
                                { InvariantNameSync, GetInvariantName(contentId, !isAsync) },
                                { ContentName, row[ContentColumnName.ContentName] as string },
                                { NetContentName, row[ContentColumnName.NetContentName] as string },
                                { IsForReplication, true },
                                { DefaultFieldsName, DefaultFields }
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
