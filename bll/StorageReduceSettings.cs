﻿using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace Quantumart.QP8.BLL;

public class StorageReduceSettings
{
    [JsonPropertyName("extensionsAllowedToResize")]
    public string[] ExtensionsAllowedToResize { get; set; }

    [JsonPropertyName("resizedImageTemplate")]
    public string ResizedImageTemplate { get; set; }

    [JsonPropertyName("reduceSizes")]
    public ReduceSize[] ReduceSizes { get; set; }
}

public class ReduceSize
{
    [JsonPropertyName("postfix")]
    public string Postfix { get; set; }

    [JsonPropertyName("reduceRatio")]
    public decimal ReduceRatio { get; set; }
}
