using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Linq;
using Quantumart.QP8.Constants;

namespace qp8dbupdate
{
    internal class QpReplaySettings
    {
        internal readonly bool DisableContentIdentity;
        internal readonly bool DisableFieldIdentity;
        internal readonly string Xmlpath;
        internal readonly string CustomerCode;
        internal readonly string ConfigPath;
        internal readonly bool SkipLogging;
        internal readonly bool CreateTable;

        public QpReplaySettings(bool disableContentIdentity, bool disableFieldIdentity, string xmlpath, string customerCode, string configPath, bool skipLogging, bool createTable)
        {
            DisableContentIdentity = disableContentIdentity;
            DisableFieldIdentity = disableFieldIdentity;
            Xmlpath = xmlpath;
            CustomerCode = customerCode;
            ConfigPath = configPath;
            SkipLogging = skipLogging;
            CreateTable = createTable;
        }

        public HashSet<string> IdentityTypes
        {
            get
            {
                var identityTypes = new HashSet<string>();
                if (!DisableContentIdentity)
                {
                    identityTypes.Add(EntityTypeCode.Content);
                }

                if (!DisableContentIdentity)
                {
                    identityTypes.Add(EntityTypeCode.ContentGroup);
                }

                if (!DisableFieldIdentity)
                {
                    identityTypes.Add(EntityTypeCode.Field);
                }

                if (!DisableContentIdentity)
                {
                    identityTypes.Add(EntityTypeCode.ContentLink);
                }

                return identityTypes;
            }
        }

        public XDocument ReplaySettings
        {
            get
            {
                var sb = new StringBuilder();
                sb.AppendLine(@"<fingerprint>");

                if (!DisableContentIdentity)
                {
                    sb.AppendLine(@"<entityType code=""content"" />");
                }

                if (!DisableFieldIdentity)
                {
                    sb.AppendLine(@"<entityType code=""field"" />");
                }

                sb.AppendLine(@"</fingerprint>");
                return XDocument.Load(new StringReader(sb.ToString()));
            }
        }
    }
}
