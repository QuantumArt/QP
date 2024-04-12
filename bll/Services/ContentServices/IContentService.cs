namespace Quantumart.QP8.BLL.Services.ContentServices
{
    public interface IContentService
    {
        Content Get(int contentId);

        ListResult<FolderFile> GetFileList(ListCommand command, int parentFolderId, LibraryFileFilter filter);
    }
}
