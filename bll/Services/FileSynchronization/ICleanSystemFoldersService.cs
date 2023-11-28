namespace Quantumart.QP8.BLL.Services.FileSynchronization;

public interface ICleanSystemFoldersService
{
    void CleanSystemFolders(string customerName, int numFiles);
}
