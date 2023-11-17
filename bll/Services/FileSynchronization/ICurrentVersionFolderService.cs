namespace Quantumart.QP8.BLL.Services.FileSynchronization;

public interface ICurrentVersionFolderService
{
    void SyncFolders(string customerName, int numFiles);
}
