using System;
using System.Text.Json.Serialization;

namespace Quantumart.QP8.BLL.Services.KeyCloak;

public class KeyCloakGroup
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("subGroupCount")]
    public int SubGroupCount { get; set; }

    [JsonPropertyName("subGroups")]
    public KeyCloakGroup[] SubGroups { get; set; }
}
