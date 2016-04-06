using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quantumart.QPublishing.FileSystem
{
    public interface IFileSystem
    {
        void RemoveDirectory(string path);

        void CreateDirectory(string path);

        void CopyFile(string sourceName, string destName);
    }
}
