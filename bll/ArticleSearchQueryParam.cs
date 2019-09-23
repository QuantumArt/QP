using Newtonsoft.Json;

namespace Quantumart.QP8.BLL
{
    public class ArticleSearchQueryParam
    {
        public ArticleFieldSearchType SearchType { get; set; }

        [JsonProperty("FieldID")]
        public string FieldId { get; set; }

        [JsonProperty("ReferenceFieldID")]
        public string ReferenceFieldId { get; set; }

        [JsonProperty("ContentID")]
        public string ContentId { get; set; }

        public string FieldColumn { get; set; }

        public object[] QueryParams { get; set; }
    }
}
