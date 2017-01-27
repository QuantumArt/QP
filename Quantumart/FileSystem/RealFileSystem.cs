using System.IO;

namespace Quantumart.QPublishing.FileSystem
{
    public class RealFileSystem : IFileSystem
    {
        public void RemoveDirectory(string path)
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
        }

        public void CreateDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        public void CopyFile(string sourceName, string destName)
        {
            if (!File.Exists(sourceName))
            {
                return;
            }

            if (File.Exists(destName))
            {
                File.Delete(destName);
            }

            File.Copy(sourceName, destName);
        }
    }
}
