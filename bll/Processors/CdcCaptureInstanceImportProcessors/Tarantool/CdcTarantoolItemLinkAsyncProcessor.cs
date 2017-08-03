using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using QP8.Infrastructure;
using QP8.Infrastructure.Extensions;
using Quantumart.QP8.BLL.Models.NotificationSender;
using Quantumart.QP8.Constants.Cdc;
using Quantumart.QP8.Constants.Cdc.Enums;
using Quantumart.QP8.Constants.Cdc.Tarantool;
using Quantumart.QP8.Constants.DbColumns;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.BLL.Processors.CdcCaptureInstanceImportProcessors.Tarantool
{
    public sealed class CdcTarantoolItemLinkAsyncProcessor : BaseCdcTarantoolImportProcessor
    {
        public CdcTarantoolItemLinkAsyncProcessor(string captureInstanseName)
            : base(captureInstanseName)
        {
        }

        public override List<CdcTableTypeModel> ImportCdcData(string fromLsn = null, string toLsn = null)
        {
            return GetCdcDataTable(fromLsn, toLsn).AsEnumerable().Select(row =>
            {
                try
                {
                    var isRev = Equals(true, row[ItemLinkAsyncColumnName.IsRev]);
                    var isSelf = Equals(true, row[ItemLinkAsyncColumnName.IsSelf]);
                    Ensure.Not(isRev && isSelf, "IsRev and IsSelf flags could not be both flagged as true");

                    var linkId = Convert.ToInt32(row[ItemLinkAsyncColumnName.LinkId]);
                    var leftId = Convert.ToInt32(row[ItemLinkAsyncColumnName.ItemId]);
                    var rightId = Convert.ToInt32(row[ItemLinkAsyncColumnName.LinkedItemId]);
                    return new CdcTableTypeModel
                    {
                        ChangeType = CdcActionType.Data,
                        Action = (CdcOperationType)Enum.Parse(typeof(CdcOperationType), row[CdcCommonConstants.Operation] as string),
                        TransactionDate = (DateTime)row[CdcCommonConstants.TransactionDate],
                        TransactionLsn = row[CdcCommonConstants.TransactionLsn] as string,
                        SequenceLsn = row[CdcCommonConstants.SequenceLsn] as string,
                        FromLsn = row[CdcCommonConstants.FromLsn] as string,
                        ToLsn = row[CdcCommonConstants.ToLsn] as string,
                        Entity = new CdcEntityModel
                        {
                            EntityType = TarantoolItemLinkAsyncModel.EntityType,
                            InvariantName = TarantoolItemLinkAsyncModel.GetInvariantName(linkId, isRev),
                            Columns = new Dictionary<string, object>
                            {
                                { TarantoolItemLinkAsyncModel.Id, leftId },
                                { TarantoolItemLinkAsyncModel.LinkedId, rightId }
                            },
                            MetaData = new Dictionary<string, object>
                            {
                                { TarantoolItemLinkAsyncModel.LinkId, linkId },
                                { TarantoolItemLinkAsyncModel.IsRev, isRev },
                                { TarantoolItemLinkAsyncModel.IsSelf, isSelf }
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
