using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Quantumart.QP8.BLL.Services;

public class LibraryService : ILibraryService
{
    private readonly IHttpClientFactory _factory;
    private readonly string _reduceSettingsEndpoint = "_reduce_settings";

    public LibraryService(IHttpClientFactory factory)
    {
        _factory = factory;
    }

    public async Task<StorageReduceSettings> GetSettingsFromStorage(string url)
    {
        var client = _factory.CreateClient();
        var result = await client.GetAsync($"{url}/{_reduceSettingsEndpoint}");
        return result.IsSuccessStatusCode ? await result.Content.ReadFromJsonAsync<StorageReduceSettings>() : null;
    }
}
