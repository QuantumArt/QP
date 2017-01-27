namespace Quantumart.QPublishing.FileSystem
{
    public interface IFileSystem
    {
        void RemoveDirectory(string path);

        void CreateDirectory(string path);

        void CopyFile(string sourceName, string destName);
    }
}
