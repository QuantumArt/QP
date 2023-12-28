namespace Quantumart.QP8.BLL.Services.FileSynchronization;

public interface ICleanSystemFoldersService
{
    int CleanSystemFolders(string customerName, int numFiles);
}
