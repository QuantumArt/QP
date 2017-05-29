using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Quantumart.QP8.BLL.Models.NotificationSender;
using Quantumart.QP8.Constants.Cdc;
using Quantumart.QP8.Constants.DbColumns;

namespace Quantumart.QP8.BLL.Processors.CdcCaptureInstanceImportProcessors
{
    public class CdcItemToItemProcessor : CdcImportProcessor
    {
        public CdcItemToItemProcessor(string captureInstanseName)
            : base(captureInstanseName)
        {
        }

        public override List<CdcTableTypeModel> ImportCdcData(string fromLsn = null, string toLsn = null)
        {
            return GetCdcDataTable(fromLsn, toLsn).AsEnumerable().Select(row =>
            {
#pragma warning disable 162

                // ReSharper disable UnreachableCode
                // ReSharper disable ConditionIsAlwaysTrueOrFalse

                const bool isRev = false;
                var linkId = (decimal)row[ItemToItemColumnName.LinkId];
                var leftId = (decimal)row[ItemToItemColumnName.LItemId];
                var rightId = (decimal)row[ItemToItemColumnName.RItemId];
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
                        EntityType = TarantoolItemToItemModel.EntityType,
                        InvariantName = $"{TarantoolItemToItemModel.EntityType}_{linkId}" + (isRev ? "_rev" : string.Empty),
                        Columns = new Dictionary<string, object>
                        {
                            { TarantoolItemToItemModel.Id, isRev ? rightId : leftId },
                            { TarantoolItemToItemModel.LinkedId, isRev ? leftId : rightId }
                        }
                    }
                };

                // ReSharper restore ConditionIsAlwaysTrueOrFalse
                // ReSharper restore UnreachableCode

#pragma warning restore 162
            }).OrderBy(cdc => cdc.TransactionLsn).ToList();
        }
    }
}
