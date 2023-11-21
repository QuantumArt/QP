using System.Threading.Tasks;

namespace Quantumart.QP8.BLL.Services;

public interface ILibraryService
{
    Task<StorageReduceSettings> GetSettingsFromStorage(string url);
}
