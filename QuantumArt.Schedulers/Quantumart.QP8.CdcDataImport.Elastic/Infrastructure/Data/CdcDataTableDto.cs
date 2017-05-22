using System;
using System.Collections.Generic;

namespace Quantumart.QP8.CdcDataImport.Elastic.Infrastructure.Data
{
    internal class CdcDataTableDto
    {
        public string CustomerCode { get; set; }

        public string TransactionLsn { get; set; }

        public DateTime TransactionDate { get; set; }

        public List<CdcChangeDto> Changes { get; set; }
    }
}
