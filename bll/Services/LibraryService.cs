using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Quantumart.QP8.BLL.Services;

public class LibraryService : ILibraryService
{
    private readonly IHttpClientFactory _factory;
    private const string _reduceSettingdEndpoint = "_reduce_settings";

    public LibraryService(IHttpClientFactory factory)
    {
        _factory = factory;
    }

    public async Task<StorageReduceSettings> GetSettingsFromStorage(string url)
    {
        var client = _factory.CreateClient();
        var result = await client.GetAsync($"{url}/{_reduceSettingdEndpoint}");
        if (result.IsSuccessStatusCode)
        {
            var content = await result.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<StorageReduceSettings>(content);
        }

        return null;
    }
}
