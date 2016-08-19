using System.Collections.Generic;

namespace Quantumart.QP8.ConsoleDbUpdate.Infrastructure.FileSystemReaders
{
    internal class CsvReaderSettings
    {
        public CsvReaderSettings(IList<string> filePathes)
        {
            FilePathes = filePathes;
        }

        public IList<string> FilePathes { get; }
    }
}
