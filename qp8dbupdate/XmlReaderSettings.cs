using System.Collections.Generic;

namespace qp8dbupdate
{
    internal class XmlReaderSettings
    {
        internal const string ConfigElementNodeName = "RecordedActionsData";
        internal const string ConfigElementPathAttribute = "RelativePath";

        public XmlReaderSettings(IList<string> recordedXmlFilePathes)
        {
            RecordedXmlFilePathes = recordedXmlFilePathes;
        }

        public IList<string> RecordedXmlFilePathes { get; }
    }
}
