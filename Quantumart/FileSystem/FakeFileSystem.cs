using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quantumart.QPublishing.FileSystem
{
    public class FakeFileSystem : IFileSystem
    {
        public void RemoveDirectory(string path)
        {
        }

        public void CreateDirectory(string path)
        {
        }

        public void CopyFile(string sourceName, string destName)
        {
        }
    }
}
