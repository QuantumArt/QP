using Quantumart.QP8.BLL.Services.DTO;

namespace Quantumart.QP8.BLL.Services;

public interface ISiteService
{
    LibraryResult Library(int id, string subFolder);
    ListResult<FolderFile> GetFileList(ListCommand command, int parentFolderId, LibraryFileFilter filter);

    Site Save(Site item, int[] activeCommands, int[] activeStyles);

    Site Update(Site item, int[] activeCommands, int[] activeStyles);
}
