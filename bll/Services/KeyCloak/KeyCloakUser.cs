using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Quantumart.QP8.BLL.Services.KeyCloak;

public class KeyCloakUser
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("username")]
    public string UserName { get; set; }

    [JsonPropertyName("firstName")]
    public string FirstName { get; set; }

    [JsonPropertyName("lastName")]
    public string LastName { get; set; }

    [JsonPropertyName("email")]
    public string Email { get; set; }

    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; }

    public List<string> Groups { get; set; } = new();
}
