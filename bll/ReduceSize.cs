using System.Text.Json.Serialization;

namespace Quantumart.QP8.BLL;

public class ReduceSize
{
    [JsonPropertyName("postfix")]
    public string Postfix { get; set; }

    [JsonPropertyName("reduceRatio")]
    public decimal ReduceRatio { get; set; }
}
