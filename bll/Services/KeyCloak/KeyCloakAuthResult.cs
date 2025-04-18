using System.Text.Json;

namespace Quantumart.QP8.BLL.Services.KeyCloak;

public class KeyCloakAuthResult
{
    public bool IsSuccess { get; set; }
    public JsonElement Response { get; set; }
}
