using System.Collections.Generic;

namespace Quantumart.QP8.ConsoleDbUpdate.Infrastructure.FileSystemReaders
{
    internal class XmlReaderSettings
    {
        internal const string ConfigElementNodeName = "RecordedActionsData";
        internal const string ConfigElementPathAttribute = "RelativePath";

        public XmlReaderSettings(IList<string> filePathes)
        {
            FilePathes = filePathes;
        }

        public IList<string> FilePathes { get; }
    }
}
