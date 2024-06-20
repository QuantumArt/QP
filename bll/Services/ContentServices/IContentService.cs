using Quantumart.QP8.BLL.Services.DTO;

namespace Quantumart.QP8.BLL.Services.ContentServices
{
    public interface IContentService
    {
        Content Get(int contentId);

        ListResult<FolderFile> GetFileList(ListCommand command, int parentFolderId, LibraryFileFilter filter);

        LibraryResult Library(int id, string subFolder);
    }
}
