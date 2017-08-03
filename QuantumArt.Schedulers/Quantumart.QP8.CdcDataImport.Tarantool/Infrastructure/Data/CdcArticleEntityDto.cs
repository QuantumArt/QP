using Newtonsoft.Json;

namespace Quantumart.QP8.CdcDataImport.Tarantool.Infrastructure.Data
{
    internal class CdcArticleEntityDto : CdcEntityDto
    {
        [JsonProperty("isSplitted")]
        public bool IsSplitted { get; set; }
    }
}
