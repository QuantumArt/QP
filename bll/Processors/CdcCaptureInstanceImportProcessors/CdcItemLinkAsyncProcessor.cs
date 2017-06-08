﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using QP8.Infrastructure;
using Quantumart.QP8.BLL.Models.NotificationSender;
using Quantumart.QP8.Constants.Cdc;
using Quantumart.QP8.Constants.Cdc.Enums;
using Quantumart.QP8.Constants.DbColumns;
using static Quantumart.QP8.Constants.Cdc.TarantoolItemLinkAsyncModel;

namespace Quantumart.QP8.BLL.Processors.CdcCaptureInstanceImportProcessors
{
    public class CdcItemLinkAsyncProcessor : CdcImportProcessor
    {
        public CdcItemLinkAsyncProcessor(string captureInstanseName)
            : base(captureInstanseName)
        {
        }

        public override List<CdcTableTypeModel> ImportCdcData(string fromLsn = null, string toLsn = null)
        {
            return GetCdcDataTable(fromLsn, toLsn).AsEnumerable().Select(row =>
            {
                var isRev = Equals(true, row[ItemLinkAsyncColumnName.IsRev]);
                var isSelf = Equals(true, row[ItemLinkAsyncColumnName.IsSelf]);
                Ensure.Not(isRev && isSelf, "IsRev and IsSelf flags could not be both flagged as true");

                var linkId = (int)row[ItemLinkAsyncColumnName.LinkId];
                var leftId = (int)row[ItemLinkAsyncColumnName.ItemId];
                var rightId = (int)row[ItemLinkAsyncColumnName.LinkedItemId];
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
                        EntityType = TarantoolItemLinkAsyncModel.EntityType,
                        InvariantName = GetInvariantName(linkId, isRev),
                        Columns = new Dictionary<string, object>
                        {
                            { Id, leftId },
                            { LinkedId, rightId }
                        },
                        MetaData = new Dictionary<string, object>
                        {
                            { LinkId, linkId },
                            { IsRev, isRev },
                            { IsSelf, isSelf }
                        }
                    }
                };
            }).OrderBy(cdc => cdc.TransactionLsn).ToList();
        }
    }
}
