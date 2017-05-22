using Quantumart.QP8.BLL.Models.NotificationSender;
using System;

namespace Quantumart.QP8.BLL.Models.NotificationSender
{
    public class CdcTableTypeModel
    {
        public string Action { get; set; }

        public CdcActionType ChangeType { get; set; }

        public DateTime TransactionDate { get; set; }

        public string TransactionLsn { get; set; }

        public string SequenceLsn { get; set; }

        public string FromLsn { get; set; }

        public string ToLsn { get; set; }

        public CdcEntityModel Entity { get; set; }
    }
}
