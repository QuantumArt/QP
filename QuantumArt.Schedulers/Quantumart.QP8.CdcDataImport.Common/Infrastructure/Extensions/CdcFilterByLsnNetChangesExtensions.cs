using System;
using System.Collections.Generic;
using System.Linq;
using Quantumart.QP8.BLL.Models.NotificationSender;

namespace Quantumart.QP8.CdcDataImport.Common.Infrastructure.Extensions
{
    public static class CdcFilterByLsnNetChangesExtensions
    {
        public static List<CdcTableTypeModel> FilterCdcTableTypeByLsnNetChanges(this IEnumerable<CdcTableTypeModel> data, Func<CdcTableTypeModel, object> groupkey)
        {
            return data.FilterByLsnNetChanges(groupkey, (prev, curr) => string.Compare(curr.SequenceLsn, prev.SequenceLsn, StringComparison.InvariantCulture) > 0  ? curr : prev);
        }

        public static List<T> FilterByLsnNetChanges<T>(this IEnumerable<T> data, Func<T, object> groupkey, Func<T, T, T> maxPredicate)
        {
            return data.GroupBy(groupkey).Select(d => d.Aggregate(maxPredicate)).ToList();
        }
    }
}
