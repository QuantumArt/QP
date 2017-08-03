using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Quantumart.QP8.CdcDataImport.Tarantool.Infrastructure.Data
{
    internal class CdcDataTableDto
    {
        [JsonProperty("customerCode")]
        public string CustomerCode { get; set; }

        [JsonProperty("lsn")]
        public string TransactionLsn { get; set; }

        [JsonProperty("transactionDate")]
        public DateTime TransactionDate { get; set; }

        [JsonProperty("changes")]
        public List<CdcChangeDto> Changes { get; set; }
    }
}
