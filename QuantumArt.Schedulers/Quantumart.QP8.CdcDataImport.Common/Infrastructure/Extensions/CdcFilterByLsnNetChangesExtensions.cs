﻿using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Quantumart.QP8.BLL.Models.NotificationSender;
using Quantumart.QP8.Constants.Cdc.Enums;

namespace Quantumart.QP8.CdcDataImport.Common.Infrastructure.Extensions
{
    public static class CdcFilterByLsnNetChangesExtensions
    {
        public static List<CdcTableTypeModel> GetCdcDataFilteredByLsnNetChanges(this IEnumerable<CdcTableTypeModel> data, Func<CdcTableTypeModel, object> groupkey)
        {
            return data.GetDataFilteredByLsnNetChanges(groupkey, (prev, curr) =>
                string.CompareOrdinal(curr.SequenceLsn, prev.SequenceLsn) > 0
                    ? HandleSimpleTableTypeDataModifier(prev, curr)
                    : HandleSimpleTableTypeDataModifier(curr, prev));
        }

        public static List<CdcTableTypeModel> GetCdcDataFilteredByLsnNetChangesWithColumnsCopy(this IEnumerable<CdcTableTypeModel> data, Func<CdcTableTypeModel, object> groupkey)
        {
            return data.GetDataFilteredByLsnNetChanges(groupkey, (prev, curr) =>
                string.CompareOrdinal(curr.SequenceLsn, prev.SequenceLsn) > 0
                    ? HandleContentDataTableTypeDataModifier(prev, curr)
                    : HandleContentDataTableTypeDataModifier(curr, prev));
        }

        public static List<T> GetDataFilteredByLsnNetChanges<T>(this IEnumerable<T> data, Func<T, object> groupkey, Func<T, T, T> maxPredicate)
        {
            return data.GroupBy(groupkey).AsParallel().Select(d => d.Aggregate(maxPredicate)).ToList();
        }

        private static CdcTableTypeModel HandleSimpleTableTypeDataModifier(CdcTableTypeModel prevInSortOrder, CdcTableTypeModel nextInSortOrder)
        {
            var result = Mapper.Map<CdcTableTypeModel, CdcTableTypeModel>(nextInSortOrder);
            if (result.Action == CdcOperationType.Update && prevInSortOrder.Action == CdcOperationType.Insert)
            {
                result.Action = CdcOperationType.Insert;
            }

            return result;
        }

        private static CdcTableTypeModel HandleContentDataTableTypeDataModifier(CdcTableTypeModel prevInSortOrder, CdcTableTypeModel nextInSortOrder)
        {
            var result = Mapper.Map<CdcTableTypeModel, CdcTableTypeModel>(nextInSortOrder);
            if (result.Action == CdcOperationType.Update)
            {
                if (prevInSortOrder.Action == CdcOperationType.Insert)
                {
                    result.Action = CdcOperationType.Insert;
                }

                if (prevInSortOrder.Action == CdcOperationType.Insert || prevInSortOrder.Action == CdcOperationType.Update)
                {
                    foreach (var col in prevInSortOrder.Entity.Columns)
                    {
                        if (!result.Entity.Columns.ContainsKey(col.Key))
                        {
                            result.Entity.Columns.Add(col);
                        }
                    }
                }
            }

            return result;
        }
    }
}
