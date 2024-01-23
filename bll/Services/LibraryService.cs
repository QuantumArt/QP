using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Quantumart.QP8.Configuration;

namespace Quantumart.QP8.BLL.Services;

public class LibraryService : ILibraryService
{
    private readonly IHttpClientFactory _factory;
    private readonly string _reduceSettingsEndpoint = "_reduce_settings";
    private readonly QPublishingOptions _options;

    public LibraryService(IHttpClientFactory factory, QPublishingOptions options)
    {
        _factory = factory;
        _options = options;
    }

    public async Task<StorageReduceSettings> GetSettingsFromStorage(string url)
    {
        var client = _factory.CreateClient();
        var resultUrl = _options.ForceHttpForImageResizing ? url.Replace("https:", "http:") : url;
        var result = await client.GetAsync($"{resultUrl}/{_reduceSettingsEndpoint}");
        return result.IsSuccessStatusCode ? await result.Content.ReadFromJsonAsync<StorageReduceSettings>() : null;
    }
}
